using Microsoft.EntityFrameworkCore;
using SoftyConsoleApp.Context;
using SoftyConsoleApp.Domain.AppComponents;

namespace SoftyConsoleApp.Domain.StudentsManagement;

public abstract class StudentsManager
{
    private static void Create()
    {
        Console.WriteLine("Enter Student Name:");
        var studentName = Console.ReadLine() ?? "";

        while (string.IsNullOrWhiteSpace(studentName))
        {
            Console.WriteLine("Invalid Name! Please try again: ");
            studentName = Console.ReadLine() ?? "";
        }

        using var context = new AppDbContext();
        if (studentName != null)
        {
            var student = new Student(studentName);
            context.Students.Add(student);
        }

        context.SaveChanges();
        
        ToManage();
    }
    
    private static void Display()
    {
        using var context = new AppDbContext();
        Console.WriteLine("Enter Student Identifier:");
        var id = int.Parse(Console.ReadLine() ?? "0");
        
        while (!context.Students.Select(s => s.Id).Contains(id))
        {
            Console.WriteLine("Invalid Id! Please try again:");
            id = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var student = context.Students.Include(hasCourse => hasCourse.Courses).First(s => s.Id == id);
        
        Console.WriteLine($"Student : {student.Name}");
        
        if (student.Courses == null) return;
        
        Utilities.RefactorCourses(context,student);
        
        context.Students.Update(student);
    }
    
    private static void Assign()
    {
        Console.WriteLine("1: Assign to School");
        Console.WriteLine("2: Assign to Classroom");
        Console.WriteLine("0: Back to Main Menu");
        
        var userSelection = int.Parse(Console.ReadLine() ?? "0");

        while (userSelection is not 1 and not 2 and not 0)
        {
            Console.WriteLine("Invalid Selection! Please try again: ");
            userSelection = int.Parse(Console.ReadLine() ?? "0");
        }
        
        Console.WriteLine("Enter Student Identifier:");
        var studentId = int.Parse(Console.ReadLine() ?? "0");
        
        using var context = new AppDbContext();
        while (!context.Students.Select(s => s.Id).Contains(studentId))
        {
            Console.WriteLine("Invalid Course Identifier! Please try again: ");
            studentId = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var student = context.Students.First(c => c.Id == studentId);

        switch (userSelection)
        {
            case 1:
                AssignToSchool(context,student);
                break;
            case 2:
                AssignToClassroom(context,student);
                break;
            case 0:
                Utilities.BackToMainMenu();
                break;
        }
    }
    
    private static void AssignToSchool(AppDbContext context,Student student)
    {
        context.Students.Attach(student);
        Console.WriteLine("Enter School Identifier:");
        var schoolId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Schools.Select(s => s.Id).Contains(schoolId))
        {
            Console.WriteLine("Invalid School Identifier! Please try again: ");
        }
        
        var school = context.Schools.Include(hasCourse => hasCourse.Courses)
            .ThenInclude(course => course.Chapters)
            .Include(school => school.Students).First(s => s.Id == schoolId);
        
        if (school.Students != null && school.Students.Contains(student)) 
            Console.WriteLine("Student already assigned to this school!");
        else
        {
            context.Entry(student).Collection(e => e.Courses).Load();
            context.Entry(student).Collection(e => e.CoursesOrders).Load();
            student.School = school;
            if (school.Courses != null)
                foreach (var course in school.Courses)
                {
                    if ( student.CoursesOrders.Select(co => co.Id).Contains(course.Id)) continue;
                    var courseOrder =
                        new CourseOrder
                        {
                            CourseId = course.Id,
                            OrderNumber = student.CoursesOrders is { Count: > 0 } ? student.CoursesOrders.Count + 1 : 1,
                            ChaptersOrders = []
                        };
                    foreach (var chapter in course.Chapters)
                    {
                        courseOrder.ChaptersOrders
                            .Add(new ChapterOrder {
                                ChapterId = chapter.Id,
                                OrderNumber = courseOrder.ChaptersOrders.Count + 1});
                    }
                    student.CoursesOrders?.Add(courseOrder);
                }
            school.Students?.Add(student);
            context.SaveChanges();
            Console.WriteLine("Student assigned to school successfully!");
        }
    }

    private static void AssignToClassroom(AppDbContext context,Student student)
    {
        context.Students.Attach(student);
        Console.WriteLine("Enter Classroom Identifier:");
        var classroomId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Classrooms.Select(s => s.Id).Contains(classroomId))
        {
            Console.WriteLine("Invalid Classroom Identifier! Please try again: ");
        }
        
        var classroom = context.Classrooms.Include(classroom => classroom.Students)
            .Include(hasCourse => hasCourse.Courses)
            .ThenInclude(course => course.Chapters)
            .Include(hasCourse => hasCourse.CoursesOrders)
            .ThenInclude(courseOrder => courseOrder.ChaptersOrders)
            .First(c => c.Id == classroomId);
        
        if (classroom.Students != null && classroom.Students.Contains(student)) 
            Console.WriteLine("Student already assigned to this classroom!");
        else
        {
            context.Entry(student).Collection(e => e.Courses).Load();
            context.Entry(student).Collection(e => e.CoursesOrders).Load();
            student.Classrooms?.Add(classroom);
            if (classroom.CoursesOrders != null)
                foreach (var co in classroom.CoursesOrders.OrderBy(c => c.OrderNumber))
                {
                    if ( student.CoursesOrders != null && student.CoursesOrders
                            .Select(c => c.Id).Contains(co.CourseId)) continue;
                    var courseOrder =
                        new CourseOrder
                        {
                            CourseId = co.CourseId,
                            OrderNumber = student.CoursesOrders is { Count: > 0 } ? student.CoursesOrders.Count + 1 : 1,
                            ChaptersOrders = []
                        };
                    
                    foreach (var chapter in co.ChaptersOrders)
                    {
                        courseOrder.ChaptersOrders
                            .Add(new ChapterOrder {
                                ChapterId = chapter.ChapterId,
                                OrderNumber = courseOrder.ChaptersOrders.Count + 1});
                    }
                    student.CoursesOrders?.Add(courseOrder);
                }
            classroom.Students?.Add(student);
            context.SaveChanges();
            Console.WriteLine("Student assigned to classroom successfully!");
        }
    }
    
    private static void Remove()
    {
        Console.WriteLine("1: Remove from School");
        Console.WriteLine("2: Remove from Classroom");
        Console.WriteLine("0: Back to Main Menu");
        
        var userSelection = int.Parse(Console.ReadLine() ?? "0");

        while (userSelection is not 1 and not 2 and not 0)
        {
            Console.WriteLine("Invalid Selection! Please try again: ");
            userSelection = int.Parse(Console.ReadLine() ?? "0");
        }
        
        Console.WriteLine("Enter Student Identifier:");
        var studentId = int.Parse(Console.ReadLine() ?? "0");

        using var context = new AppDbContext();
        while (!context.Students.Select(c => c.Id).Contains(studentId))
        {
            Console.WriteLine("Invalid Student Identifier! Please try again: ");
            studentId = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var student = context.Students.First(c => c.Id == studentId);
        
        switch (userSelection)
        {
            case 1:
                RemoveFromSchool(student);
                break;
            case 2:
                RemoveFromClassroom(student);
                break;
            case 0:
                Utilities.BackToMainMenu();
                break;
        }
    }
    
    private static void RemoveFromSchool(Student student)
    {
        
        using var context = new AppDbContext();
        context.Students.Attach(student);
        Console.WriteLine("Enter School Identifier:");
        var schoolId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Schools.Select(s => s.Id).Contains(schoolId))
        {
            Console.WriteLine("Invalid School Identifier! Please try again: ");
            schoolId = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var school = context.Schools.Include(hasCourse => hasCourse.Courses)
            .First(s => s.Id == schoolId);
        
        student.School = null;
        if (school.Courses != null)
            foreach (var course in school.Courses)
            {
                if (student.Courses != null && student.Courses.Select(c => c.Id).Contains(course.Id)) continue;
                student.CoursesOrders?.Remove(student.CoursesOrders.First(co => co.CourseId == course.Id));
            }
        
        school.Students?.Remove(student);
        context.SaveChanges();
        Console.WriteLine("Student removed from school successfully!");
    }

    private static void RemoveFromClassroom(Student student)
    {
        using var context = new AppDbContext();
        Console.WriteLine("Enter Classroom Identifier:");
        var classroomId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Classrooms.Select(s => s.Id).Contains(classroomId))
        {
            Console.WriteLine("Invalid Classroom Identifier! Please try again: ");
            classroomId = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var classroom = context.Classrooms.Include(classroom => classroom.Courses)
            .First(c => c.Id == classroomId);
        
        student.Classrooms?.Remove(classroom);
        
        if (classroom.Courses != null)
            foreach (var course in classroom.Courses)
            {
                if (student.Courses != null && student.Courses.Select(c => c.Id).Contains(course.Id)) continue;
                student.CoursesOrders?.Remove(student.CoursesOrders.First(co => co.CourseId == course.Id));
            }
        
        classroom.Students?.Remove(student);
        context.SaveChanges();
        Console.WriteLine("Student removed from classroom successfully!");
    }
    
    private static void Delete()
    {   
        Console.WriteLine("Enter Student Identifier:");
        var id = int.Parse(Console.ReadLine() ?? "0");
        
        using var context = new AppDbContext();
        var id1 = id;
        var student = context.Students
            .FirstOrDefault(student => student != null && student.Id == id1, null);

        while (student == null)
        {
            Console.WriteLine("Invalid Id! Please try again: ");
            id = int.Parse(Console.ReadLine() ?? "0");
            var id2 = id;
            student = context.Students
                .FirstOrDefault(student1 => student1 != null && student1.Id == id2, null);
        }
        
        context.Students.Remove(student); 
        context.SaveChanges();
        
        Console.WriteLine("Student Deleted Successfully!");
        Utilities.BackToMainMenu();
    }
    
    public static void ToManage()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("***************************");    
        Console.WriteLine("* Students Management App *");    
        Console.WriteLine("***************************");  
        Console.ResetColor();
    
        Console.WriteLine("1: Create Student");
        Console.WriteLine("2: Display Student Study Program");
        Console.WriteLine("3: Delete Student!");
        Console.WriteLine("4: Assign Student");
        Console.WriteLine("5: Remove Student");
        Console.WriteLine("0: Back to Main Menu!");
        
        var userSelection = Console.ReadLine();

        while (userSelection != "0" && userSelection != "1" && userSelection != "2" 
               && userSelection != "3" && userSelection != "4" && userSelection != "5")
        {
            Console.WriteLine("Invalid selection. Please try again: ");
            
            userSelection = Console.ReadLine();
        }
        
        switch (userSelection)
        {
            case "1":
                Create();
                break;
            case "2":
                Display();
                break;
            case "3":
                Delete();
                break;
            case "4":
                Assign();
                break;
            case "5":
                Remove();
                break;
            case "0":
                Utilities.StartApp();
                break;
        }
    }
}