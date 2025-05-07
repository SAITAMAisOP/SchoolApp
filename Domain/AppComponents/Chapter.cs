namespace SoftyConsoleApp.Domain.AppComponents;

public class Chapter(string? name)
{
    public int Id { get; init; }

    public string? Name { get; init; } = name;
    
    public Course? Course { get; set; }
}