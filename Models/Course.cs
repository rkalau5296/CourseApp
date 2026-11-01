using System;
using System.Collections.Generic;
using System.Text;

namespace CourseApp.Models;

public class Course
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty; 
    public ICollection<CourseModule> Modules { get; set; } = new List<CourseModule>();
}
