using System.Collections.Generic;

namespace CourseApp.Models;

// Jeden węzeł drzewa: kurs, podkurs, moduł lub część (Part). Korzeń ma ParentId == null.
public class CourseNode
{
    public int CourseNodeId { get; set; }
    public int? ParentId { get; set; }
    public CourseNode? Parent { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public ICollection<CourseNode> Children { get; set; } = new List<CourseNode>();
    public ICollection<CourseItem> Items { get; set; } = new List<CourseItem>();
}
