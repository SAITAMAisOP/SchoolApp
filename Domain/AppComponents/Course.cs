namespace SoftyConsoleApp.Domain.AppComponents;

public class Course(string? name)
{
    public int Id { get; set; }

    public string? Name { get; init; } = name;
    
    public List<Chapter>? Chapters { get; set; }
    
    public bool IsOneChapter { get; set; }
    
    public List<int>? SchoolsIds { get; set; }
    
    public List<int>? StudentsIds { get; set; }
    
    public List<int>? ClassroomsIds { get; set; }
}
