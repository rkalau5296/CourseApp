using CourseApp.Models;
using CourseApp.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseApp.Repository;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;

    public CourseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Course>> GetCoursesBySectionAsync(string section)
    {
        return await _context.Courses
            .Where(c => c.Section == section)
            .Include(c => c.Modules)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddCourseAsync(Course course)
    {
        await _context.Courses.AddAsync(course);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
