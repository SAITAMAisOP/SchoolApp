using Microsoft.EntityFrameworkCore;
using SoftyConsoleApp.Context;
using SoftyConsoleApp.Domain.AppComponents;

namespace SoftyConsoleApp.Domain.ClassroomsManagement;

public static class ClassroomsManager
{
    private static void Create()
    {
        Console.WriteLine("Enter Classroom Name:");
        var classroomName = Console.ReadLine() ?? "";

        while (string.IsNullOrWhiteSpace(classroomName))
        {
            Console.WriteLine("Invalid Name! Please try again: ");
            classroomName = Console.ReadLine() ?? "";
        }

        using var context = new AppDbContext();
        {
            var classroom = new Classroom(classroomName);
            context.Classrooms.Add(classroom);
        }

        context.SaveChanges();
        
        ToManage();
    }
    
    private static void Display()
    {
        Console.WriteLine("Enter Classroom Identifier:");
        var id = int.Parse(Console.ReadLine() ?? "0");
        using var context = new AppDbContext();
        while (!context.Classrooms.Select(s => s.Id).Contains(id))
        {
            Console.WriteLine("Invalid Id! Please try again: ");
            id = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var  classroom = context.Classrooms
            .Include(hasCourse => hasCourse.Courses).First(c=> c.Id == id);
        
        Console.WriteLine($"Classroom : {classroom.Name}");
        
        if (classroom.Courses == null) return;
        
        Utilities.RefactorCourses(context,classroom);
        
        context.Classrooms.Update(classroom);
    } 
    
    private static void Assign()
    {
        Console.WriteLine("1: Assign to School");
        Console.WriteLine("0: Back to Main Menu");
        
        var userSelection = int.Parse(Console.ReadLine() ?? "0");

        while (userSelection is not 1 and not 0)
        {
            Console.WriteLine("Invalid Selection! Please try again: ");
            userSelection = int.Parse(Console.ReadLine() ?? "0");
        }
        
        Console.WriteLine("Enter Classroom Identifier:");
        var studentId = int.Parse(Console.ReadLine() ?? "0");
        
        using var context = new AppDbContext();
        while (!context.Classrooms.Select(c => c.Id).Contains(studentId))
        {
            Console.WriteLine("Invalid Course Identifier! Please try again: ");
            studentId = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var classroom = context.Classrooms.First(c => c.Id == studentId);


        switch (userSelection)
        {
            case 1:
                AssignToSchool(context,classroom);
                break;
            case 0:
                Utilities.BackToMainMenu();
                break;
        }
        
    }
    
    private static void AssignToSchool(AppDbContext context,Classroom classroom)
    {
        context.Classrooms.Attach(classroom);
        Console.WriteLine("Enter School Identifier:");
        var schoolId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Schools.Select(s => s.Id).Contains(schoolId))
        {
            Console.WriteLine("Invalid School Identifier! Please try again: ");
            schoolId = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var school = context.Schools.Include(school => school.Classrooms)
            .Include(hasCourse => hasCourse.Courses)
            .ThenInclude(course => course.Chapters).First(s => s.Id == schoolId);
        
        if (school.Classrooms != null && school.Classrooms.Contains(classroom)) 
            Console.WriteLine("Classroom already assigned to this school!");
        else
        {
            context.Entry(classroom).Collection(e => e.Courses).Load();
            context.Entry(classroom).Collection(e => e.CoursesOrders).Load();
            classroom.School = school;
            if (school.Courses != null)
                foreach (var course in school.Courses.Where(course =>  
                             classroom.CoursesOrders != null && !classroom.CoursesOrders.Select(co => co.CourseId)
                                 .Contains(course.Id)))
                {
                    
                    var courseOrder =
                        new CourseOrder
                        {
                            CourseId = course.Id,
                            OrderNumber = classroom.CoursesOrders is { Count: > 0 } ? classroom.CoursesOrders.Count + 1 : 1,
                            ChaptersOrders = []
                        };
                    if (course.Chapters != null)
                    {
                        foreach (var chapter in course.Chapters)
                        {
                            courseOrder.ChaptersOrders
                                .Add(new ChapterOrder {
                                    ChapterId = chapter.Id,
                                    OrderNumber = courseOrder.ChaptersOrders.Count + 1});
                        }
                    }
                    classroom.CoursesOrders?.Add(courseOrder);
                }

            school.Classrooms?.Add(classroom);
            context.SaveChanges();
            Console.WriteLine("Classroom assigned to school successfully!");
        }
    }    
    private static void Remove()
    {
        Console.WriteLine("1: Remove from School");
        Console.WriteLine("0: Back to Main Menu");
        
        var userSelection = int.Parse(Console.ReadLine() ?? "0");

        while (userSelection is not 1 and not 2 and not 0)
        {
            Console.WriteLine("Invalid Selection! Please try again: ");
            userSelection = int.Parse(Console.ReadLine() ?? "0");
        }
        
        Console.WriteLine("Enter Classroom Identifier:");
        var classroomId = int.Parse(Console.ReadLine() ?? "0");

        using var context = new AppDbContext();
        while (!context.Classrooms.Select(c => c.Id).Contains(classroomId))
        {
            Console.WriteLine("Invalid Classroom Identifier! Please try again: ");
            classroomId = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var classroom = context.Classrooms.First(c => c.Id == classroomId);
        
        switch (userSelection)
        {
            case 1:
                RemoveFromSchool(classroom);
                break;
            case 0:
                Utilities.BackToMainMenu();
                break;
        }
    }
    
    private static void RemoveFromSchool(Classroom classroom)
    {
        using var context = new AppDbContext();
        context.Classrooms.Attach(classroom);
        Console.WriteLine("Enter School Identifier:");
        var schoolId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Schools.Select(s => s.Id).Contains(schoolId))
        {
            Console.WriteLine("Invalid School Identifier! Please try again: ");
            schoolId = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var school = context.Schools.Include(hasCourse => hasCourse.Courses)
            .First(s => s.Id == schoolId);

        classroom.School = null;
        if (school.Courses != null)
            foreach (var course in school.Courses)
            {
                if (classroom.Courses != null && classroom.Courses.Select(c => c.Id).Contains(course.Id)) continue;
                classroom.CoursesOrders?.Remove(classroom.CoursesOrders.First(co => co.CourseId == course.Id));
            }

        school.Classrooms?.Remove(classroom);
        context.SaveChanges();
        Console.WriteLine("Classroom removed from school successfully!");
    }
    
    private static void Delete()
    {   
        Console.WriteLine("Enter Classroom Identifier:");
        var id = int.Parse(Console.ReadLine() ?? "0");
        
        using var context = new AppDbContext();
        var id1 = id;
        var classroom = context.Classrooms
            .FirstOrDefault(cl => cl != null && cl.Id == id1, null);

        while (classroom == null)
        {
            Console.WriteLine("Invalid Id! Please try again: ");
            id = int.Parse(Console.ReadLine() ?? "0");
            var id2 = id;
            classroom = context.Classrooms
                .FirstOrDefault(cl => cl != null && cl.Id == id2, null);
        }
        
        context.Classrooms.Remove(classroom);
        context.SaveChanges();
        Console.WriteLine("Classroom Deleted Successfully!");
        Utilities.BackToMainMenu();
    }

    
    public static void ToManage()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("*****************************");    
        Console.WriteLine("* Classrooms Management App *");    
        Console.WriteLine("*****************************");  
        Console.ResetColor();
    
        Console.WriteLine("1: Create Classroom");
        Console.WriteLine("2: Display Classroom Study Program");
        Console.WriteLine("3: Delete Classroom!");
        Console.WriteLine("4: Assign Classroom");
        Console.WriteLine("0: Back to Main Menu!");
        
        var userSelection = Console.ReadLine();

        while (userSelection != "0" && userSelection != "1" && userSelection != "2" && userSelection != "3" && userSelection != "4")
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