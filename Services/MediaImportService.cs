using CourseApp.Models;
using CourseApp.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CourseApp.Services;

public record ImportResult(int NodesAdded, int ItemsAdded, int ItemsSkipped);

public interface IMediaImportService
{
    // Odwzorowuje strukturę folderu na drzewo pod wskazanym węzłem. Import można bezpiecznie powtarzać.
    Task<ImportResult> ImportFolderAsync(int targetNodeId, string folderPath);

    // Zamienia ścieżkę zapisaną w bazie (względną do MediaRoot) na pełną ścieżkę pliku.
    string ResolvePath(string storedPath);
}

public partial class MediaImportService : IMediaImportService
{
    private static readonly Dictionary<string, CourseItemType> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        [".mp4"] = CourseItemType.Video, [".mkv"] = CourseItemType.Video, [".mov"] = CourseItemType.Video,
        [".avi"] = CourseItemType.Video, [".webm"] = CourseItemType.Video,
        [".mp3"] = CourseItemType.Audio, [".wav"] = CourseItemType.Audio, [".m4a"] = CourseItemType.Audio,
        [".pdf"] = CourseItemType.Pdf
    };

    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly string _mediaRoot;

    public MediaImportService(IDbContextFactory<AppDbContext> factory, string mediaRoot)
    {
        _factory = factory;
        _mediaRoot = mediaRoot;
    }

    public async Task<ImportResult> ImportFolderAsync(int targetNodeId, string folderPath)
    {
        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"Folder nie istnieje: {folderPath}");

        await using var context = await _factory.CreateDbContextAsync();
        var target = await context.CourseNodes
            .Include(n => n.Children)
            .Include(n => n.Items)
            .FirstAsync(n => n.CourseNodeId == targetNodeId);

        int nodes = 0, items = 0, skipped = 0;
        await ImportRecursive(context, target, folderPath);
        await context.SaveChangesAsync();
        return new ImportResult(nodes, items, skipped);

        async Task ImportRecursive(AppDbContext db, CourseNode node, string dir)
        {
            var known = node.Items.Select(i => i.FilePath).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var order = node.Items.Select(i => i.SortOrder).DefaultIfEmpty(0).Max();

            var files = Directory.EnumerateFiles(dir)
                .Where(f => Extensions.ContainsKey(Path.GetExtension(f)) && !IsHidden(f))
                .OrderBy(f => LeadingNumber(Path.GetFileName(f)))
                .ThenBy(f => Path.GetFileName(f), StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            // Usuwa wpisy-zaślepki z seeda (bez ścieżki), gdy w folderze są prawdziwe pliki.
            if (files.Count > 0)
            {
                foreach (var placeholder in node.Items.Where(i => string.IsNullOrEmpty(i.FilePath)).ToList())
                {
                    node.Items.Remove(placeholder);
                    if (placeholder.CourseItemId != 0) db.CourseItems.Remove(placeholder);
                }
                known = node.Items.Select(i => i.FilePath).ToHashSet(StringComparer.OrdinalIgnoreCase);
                order = node.Items.Select(i => i.SortOrder).DefaultIfEmpty(0).Max();
            }

            foreach (var file in files)
            {
                var stored = ToStoredPath(file);
                if (!known.Add(stored)) { skipped++; continue; }

                node.Items.Add(new CourseItem
                {
                    Title = CleanTitle(Path.GetFileNameWithoutExtension(file)),
                    Type = Extensions[Path.GetExtension(file)],
                    FilePath = stored,
                    SortOrder = ++order
                });
                items++;
            }

            var subdirs = Directory.EnumerateDirectories(dir)
                .OrderBy(d => LeadingNumber(Path.GetFileName(d)))
                .ThenBy(d => Path.GetFileName(d), StringComparer.CurrentCultureIgnoreCase);

            foreach (var sub in subdirs)
            {
                var name = Path.GetFileName(sub);
                var key = Normalize(name);
                var child = node.Children.FirstOrDefault(c => Normalize(c.Title) == key);

                if (child is null)
                {
                    child = new CourseNode
                    {
                        Title = name,
                        SortOrder = node.Children.Select(c => c.SortOrder).DefaultIfEmpty(0).Max() + 1
                    };
                    node.Children.Add(child);
                    nodes++;
                }
                else
                {
                    await db.Entry(child).Collection(c => c.Children).LoadAsync();
                    await db.Entry(child).Collection(c => c.Items).LoadAsync();
                }

                await ImportRecursive(db, child, sub);
            }
        }
    }

    public string ResolvePath(string storedPath) =>
        Path.IsPathRooted(storedPath) || string.IsNullOrWhiteSpace(_mediaRoot)
            ? storedPath
            : Path.Combine(_mediaRoot, storedPath);

    // Pomija śmieci z macOS (._plik, .DS_Store) i pliki ukryte.
    private static bool IsHidden(string file) =>
        Path.GetFileName(file).StartsWith('.') || (File.GetAttributes(file) & FileAttributes.Hidden) != 0;

    private string ToStoredPath(string file)
    {
        if (string.IsNullOrWhiteSpace(_mediaRoot)) return file;
        var relative = Path.GetRelativePath(_mediaRoot, file);
        return relative.StartsWith("..") || Path.IsPathRooted(relative) ? file : relative;
    }

    // "modul1", "Moduł 1" i "MODUŁ-1" dają ten sam klucz, dzięki czemu foldery trafiają do istniejących węzłów.
    private static string Normalize(string s)
    {
        var sb = new StringBuilder();
        foreach (var ch in s.Replace('ł', 'l').Replace('Ł', 'L').Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(ch)) sb.Append(char.ToLowerInvariant(ch));
        }
        return sb.ToString();
    }

    private static int LeadingNumber(string name)
    {
        var m = LeadingNumberRegex().Match(name);
        return m.Success && int.TryParse(m.Value, out var n) ? n : int.MaxValue;
    }

    private static string CleanTitle(string name) => LeadingPrefixRegex().Replace(name, "").Trim();

    [GeneratedRegex(@"^\d+")]
    private static partial Regex LeadingNumberRegex();

    [GeneratedRegex(@"^\d+\s*[.\-)]\s*")]
    private static partial Regex LeadingPrefixRegex();
}
