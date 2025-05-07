using SoftyConsoleApp.Contracts;

namespace SoftyConsoleApp.Domain.AppComponents;

public class Classroom(string name) : HasCourse
{
    public int Id { get; init; }
    public string? Name { get; init; } = name;
    
    public School? School { get; set; }
    
    public List<Student>? Students { get; init; }
}