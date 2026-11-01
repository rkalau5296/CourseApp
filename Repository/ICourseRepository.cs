using CourseApp.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CourseApp.Repository;

public interface ICourseRepository
{
    Task<IEnumerable<Course>> GetCoursesBySectionAsync(string section);
    Task AddCourseAsync(Course course);
    Task SaveChangesAsync();
}
