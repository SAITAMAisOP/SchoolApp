using SoftyConsoleApp.Contracts;

namespace SoftyConsoleApp.Domain.AppComponents;

public class Student(string name) : HasCourse
{
    public int Id { get; init; }
    public string? Name { get; set; } = name;
    public School? School { get; set; }
    public List<Classroom>? Classrooms { get; set; }
}