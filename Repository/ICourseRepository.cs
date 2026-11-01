using CourseApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseApp.Repository;

public interface ICourseRepository
{
    Task<IReadOnlyList<CourseNode>> GetRootCoursesAsync();

    // Zwraca kurs z całym poddrzewem (Children, Items) posortowanym po SortOrder.
    Task<CourseNode?> GetCourseTreeAsync(int courseNodeId);
}
