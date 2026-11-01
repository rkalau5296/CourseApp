using CourseApp.Models;
using CourseApp.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseApp.Repository;

public class CourseRepository : ICourseRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public CourseRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<CourseNode>> GetRootCoursesAsync()
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.CourseNodes
            .AsNoTracking()
            .Where(n => n.ParentId == null)
            .OrderBy(n => n.SortOrder)
            .ToListAsync();
    }

    public async Task<CourseNode?> GetCourseTreeAsync(int courseNodeId)
    {
        await using var context = await _factory.CreateDbContextAsync();

        // Całe drzewo jest małe, więc ładujemy je jednym zapytaniem; EF sam połączy Parent/Children.
        var nodes = await context.CourseNodes
            .Include(n => n.Items)
            .ToListAsync();

        foreach (var node in nodes)
        {
            node.Children = node.Children.OrderBy(c => c.SortOrder).ToList();
            node.Items = node.Items.OrderBy(i => i.SortOrder).ToList();
        }

        return nodes.FirstOrDefault(n => n.CourseNodeId == courseNodeId);
    }
}
