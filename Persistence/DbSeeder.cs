using CourseApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CourseApp.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.CourseNodes.AnyAsync()) return;

        context.CourseNodes.AddRange(BuildIws(), BuildAlchemia(), BuildEnglish());
        await context.SaveChangesAsync();
    }

    private static CourseNode BuildIws()
    {
        var iws = Node("IWS", 1);

        var adept = Add(iws, "Adept", 1);
        for (int i = 1; i <= 6; i++) AddWithVideos(adept, $"Moduł {i}", i);

        var adept2 = Add(iws, "Adept 2.0", 2);
        for (int i = 1; i <= 7; i++)
        {
            if (i == 6)
            {
                var m6 = Add(adept2, "Moduł 6", i);
                for (int s = 1; s <= 4; s++) AddWithVideos(m6, $"Podmoduł {s}", s);
            }
            else
            {
                AddWithVideos(adept2, $"Moduł {i}", i);
            }
        }

        var berserker = Add(iws, "Berserker", 3);
        for (int i = 1; i <= 14; i++) AddWithVideos(berserker, $"Moduł {i}", i);

        return iws;
    }

    private static CourseNode BuildAlchemia()
    {
        var alchemia = Node("Alchemia mocy", 2);
        AddVideos(alchemia, 12);
        return alchemia;
    }

    private static CourseNode BuildEnglish()
    {
        var english = Node("English", 3);

        foreach (var (exam, order) in new[] { ("FCE", 1), ("CAE", 2) })
        {
            var examNode = Add(english, exam, order);
            for (int t = 1; t <= 4; t++)
            {
                var test = Add(examNode, $"Test {t}", t);
                for (int p = 1; p <= 4; p++)
                {
                    var part = Add(test, $"Part {p}", p);
                    for (int a = 1; a <= 12; a++)
                    {
                        part.Items.Add(new CourseItem { Title = $"Nagranie {a}", Type = CourseItemType.Audio, SortOrder = a });
                    }
                    part.Items.Add(new CourseItem { Title = "Materiały PDF", Type = CourseItemType.Pdf, SortOrder = 13 });
                }
            }
        }

        Add(english, "Henry", 3);
        return english;
    }

    private static CourseNode Node(string title, int order) => new() { Title = title, SortOrder = order };

    private static CourseNode Add(CourseNode parent, string title, int order)
    {
        var child = Node(title, order);
        parent.Children.Add(child);
        return child;
    }

    private static void AddWithVideos(CourseNode parent, string title, int order) =>
        AddVideos(Add(parent, title, order), 12);

    private static void AddVideos(CourseNode node, int count)
    {
        foreach (var n in Enumerable.Range(1, count))
        {
            node.Items.Add(new CourseItem { Title = $"Film {n}", Type = CourseItemType.Video, SortOrder = n });
        }
    }
}
