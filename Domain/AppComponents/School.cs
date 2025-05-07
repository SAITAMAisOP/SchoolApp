using SoftyConsoleApp.Contracts;

namespace SoftyConsoleApp.Domain.AppComponents;

public class School(string? name) : HasCourse
{
    public int Id { get; init; }
    
    public string? Name { get; init; } = name;
    
    public List<Classroom>? Classrooms { get; init; }
    
    public List<Student>? Students { get; init; }
}