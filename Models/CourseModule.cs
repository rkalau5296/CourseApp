using System;
using System.Collections.Generic;
using System.Text;

namespace CourseApp.Models;

public class CourseModule
{
    public int ModuleId { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string SnapshotDescription { get; set; } = string.Empty; 
}
