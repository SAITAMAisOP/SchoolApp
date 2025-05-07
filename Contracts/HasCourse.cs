using SoftyConsoleApp.Domain.AppComponents;

namespace SoftyConsoleApp.Contracts;

public class HasCourse
{
    public List<Course>? Courses { get; set; }
    

    public List<CourseOrder>? CoursesOrders { get; set; }
    
}
