namespace CourseApp.Models;

public enum CourseItemType
{
    Video,
    Audio,
    Pdf
}

public class CourseItem
{
    public int CourseItemId { get; set; }
    public int CourseNodeId { get; set; }
    public CourseNode CourseNode { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public CourseItemType Type { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public string Display => Type switch
    {
        CourseItemType.Video => $"🎬 {Title}",
        CourseItemType.Audio => $"🎧 {Title}",
        _ => $"📄 {Title}"
    };
}
