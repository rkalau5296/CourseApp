using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CourseApp.Models;
using CourseApp.Repository;
using CourseApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CourseApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ICourseRepository _courseRepository;
    private readonly IMediaImportService _importService;

    [ObservableProperty]
    private CourseNode? _selectedCourse;

    [ObservableProperty]
    private CourseNode? _selectedNode;

    [ObservableProperty]
    private CourseItem? _selectedItem;

    [ObservableProperty]
    private string? _errorMessage;

    public ObservableCollection<CourseNode> Courses { get; } = new();

    // Dzieci wybranego kursu (podkursy), wyświetlane w TreeView.
    public ObservableCollection<CourseNode> CourseChildren { get; } = new();

    public ObservableCollection<CourseItem> CurrentItems { get; } = new();

    public MainViewModel(ICourseRepository courseRepository, IMediaImportService importService)
    {
        _courseRepository = courseRepository;
        _importService = importService;
    }

    [ObservableProperty]
    private string? _statusMessage;

    // Importuje folder do zaznaczonego węzła (lub do kursu, gdy nic nie zaznaczono).
    public async Task ImportFolderAsync(string folderPath)
    {
        var target = SelectedNode ?? SelectedCourse;
        if (target is null)
        {
            ErrorMessage = "Najpierw wybierz kurs lub węzeł, do którego chcesz zaimportować pliki.";
            return;
        }

        try
        {
            ErrorMessage = null;
            var result = await _importService.ImportFolderAsync(target.CourseNodeId, folderPath);
            StatusMessage = $"Import do \"{target.Title}\": +{result.ItemsAdded} plików, +{result.NodesAdded} węzłów, pominięto {result.ItemsSkipped}.";
            await ReloadTreeAsync(target.CourseNodeId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Import nie powiódł się: {ex.Message}";
        }
    }

    // Otwiera zaznaczony plik w domyślnym programie systemu (odtwarzacz, przeglądarka PDF).
    public void OpenSelectedItem()
    {
        if (SelectedItem is null) return;

        if (string.IsNullOrEmpty(SelectedItem.FilePath))
        {
            ErrorMessage = $"\"{SelectedItem.Title}\" nie ma przypisanego pliku.";
            return;
        }

        var path = _importService.ResolvePath(SelectedItem.FilePath);
        if (!File.Exists(path))
        {
            ErrorMessage = $"Nie znaleziono pliku: {path}";
            return;
        }

        try
        {
            ErrorMessage = null;
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Nie udało się otworzyć pliku: {ex.Message}";
        }
    }

    private async Task ReloadTreeAsync(int reselectNodeId)
    {
        if (SelectedCourse is null) return;
        var tree = await _courseRepository.GetCourseTreeAsync(SelectedCourse.CourseNodeId);
        if (tree is null) return;

        CourseChildren.Clear();
        foreach (var child in tree.Children) CourseChildren.Add(child);

        SelectedNode = Find(tree, reselectNodeId);

        static CourseNode? Find(CourseNode node, int id) =>
            node.CourseNodeId == id ? node : node.Children.Select(c => Find(c, id)).FirstOrDefault(n => n is not null);
    }

    [RelayCommand]
    public async Task LoadCoursesAsync()
    {
        try
        {
            ErrorMessage = null;
            Courses.Clear();
            foreach (var course in await _courseRepository.GetRootCoursesAsync())
            {
                Courses.Add(course);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Nie udało się wczytać kursów: {ex.Message}";
        }
    }

    async partial void OnSelectedCourseChanged(CourseNode? value)
    {
        CourseChildren.Clear();
        SelectedNode = null;
        if (value is null) return;

        try
        {
            var tree = await _courseRepository.GetCourseTreeAsync(value.CourseNodeId);
            if (tree is null || tree.CourseNodeId != SelectedCourse?.CourseNodeId) return;

            foreach (var child in tree.Children)
            {
                CourseChildren.Add(child);
            }

            // Kurs bez podkursów (np. Alchemia mocy) od razu pokazuje swoje pliki.
            if (tree.Children.Count == 0)
            {
                SelectedNode = tree;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Nie udało się wczytać kursu: {ex.Message}";
        }
    }

    partial void OnSelectedNodeChanged(CourseNode? value)
    {
        CurrentItems.Clear();
        SelectedItem = null;
        if (value is null) return;

        foreach (var item in value.Items)
        {
            CurrentItems.Add(item);
        }
    }
}
