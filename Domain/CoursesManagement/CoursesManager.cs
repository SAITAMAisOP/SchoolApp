using Microsoft.EntityFrameworkCore;
using SoftyConsoleApp.Context;
using SoftyConsoleApp.Domain.AppComponents;

namespace SoftyConsoleApp.Domain.CoursesManagement;

public static class CoursesManager
{
    private static void Create()
    {
        Console.WriteLine("Enter Chapter Name:");
        var courseName = Console.ReadLine() ?? "";

        while (string.IsNullOrWhiteSpace(courseName))
        {
            Console.WriteLine("Invalid Name! Please try again: ");
            courseName = Console.ReadLine() ?? "";
        }

        using var context = new AppDbContext();
        var course = new Course(courseName);
        context.Courses.Add(course);
        context.SaveChanges();
        
        ToManage();
    }

    private static void Display()
    {
        using var context = new AppDbContext();
        foreach (var course in context.Courses.Include(course => course.Chapters))
        {
            Console.WriteLine($"{course.Id}. {course.Name}");
            if (course.Chapters == null) continue;
            foreach (var chapter in course.Chapters)
                Console.WriteLine($"Chapter : {chapter.Name}");
        }
        
        Console.WriteLine("1: Assign Course");
        Console.WriteLine("2: Remove Course");
        Console.WriteLine("0: Back to Main Menu");
        
        var userSelection = int.Parse(Console.ReadLine() ?? "0");

        while (userSelection is not 1 and not 2 and not 0)
        {
            Console.WriteLine("Invalid Selection! Please try again: ");
            userSelection = int.Parse(Console.ReadLine() ?? "0");
        }

        switch (userSelection)
        {
            case 1:
                Assign(context);
                break;
            case 2:
                Remove(context);
                break;
            case 0:
                Utilities.BackToMainMenu();
                break;
        }
        if (userSelection == 0) return;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("************************");    
        Console.WriteLine("* Softy Management App *");    
        Console.WriteLine("************************");  
        Console.ResetColor();
        
        Console.WriteLine("1: Schools Management");
        Console.WriteLine("2: Classrooms Management");
        Console.WriteLine("3: Students Management");
        Console.WriteLine("4: Courses Management");
        Console.WriteLine("5: Chapters Management");
        Console.WriteLine("0: Close Application!");
    }

    private static void Assign(AppDbContext context)
    {
        Console.WriteLine("1: Assign to School");
        Console.WriteLine("2: Assign to Classroom");
        Console.WriteLine("3: Assign to Student");
        
        var userSelection = int.Parse(Console.ReadLine() ?? "0");

        while (userSelection is not 1 and not 2 and not 3)
        {
            Console.WriteLine("Invalid Selection! Please try again: ");
            userSelection = int.Parse(Console.ReadLine() ?? "0");
        }
        
        Console.WriteLine("Enter Course Identifier:");
        var courseId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Courses.Select(c => c.Id).Contains(courseId))
        {
            Console.WriteLine("Invalid Course Identifier! Please try again: ");
            courseId = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var selectedCourse = context.Courses.First(c => c.Id == courseId);

        switch (userSelection)
        {
            case 1:
                AssignToSchool(context,selectedCourse);
                break;
            case 2:
                AssignToClassroom(context,selectedCourse);
                break;
            case 3:
                AssignToStudent(context,selectedCourse);
                break;
        }
    }

    public static void AssignToSchool(AppDbContext context,Course course)
    {
        Console.WriteLine("Enter School Identifier:");
        var schoolId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Schools.Select(s => s.Id).Contains(schoolId))
        {
            Console.WriteLine("Invalid School Identifier! Please try again: ");
            schoolId = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var school = context.Schools.Include(hasCourse => hasCourse.Courses)
            .Include(hasCourse => hasCourse.CoursesOrders)
            .First(s => s.Id == schoolId);
        
        if (school.Courses != null && school.Courses.Contains(course)) 
            Console.WriteLine("Course already assigned to this school!");
        else
        {
            if (school.CoursesOrders != null)
            {
                
                var courseOrder =
                    new CourseOrder
                    {
                        CourseId = course.Id,
                        OrderNumber = school.CoursesOrders.Count + 1,
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
                school.CoursesOrders?.Add(courseOrder);
            }
            course.SchoolsIds?.Add(school.Id);
            school.Courses?.Add(course);
            context.SaveChanges();
            Console.WriteLine("Course assigned to school successfully!");
        }
    }

    public static void AssignToClassroom(AppDbContext context,Course course)
    {
        Console.WriteLine("Enter Classroom Identifier:");
        var classroomId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Classrooms.Select(s => s.Id).Contains(classroomId))
        {
            Console.WriteLine("Invalid Classroom Identifier! Please try again: ");
        }
        
        var classroom = context.Classrooms
            .Include(hasCourse => hasCourse.Courses)
            .Include(hasCourse => hasCourse.CoursesOrders)
            .First(c => c.Id == classroomId);
        
        if (classroom.Courses != null && classroom.Courses.Contains(course)) 
            Console.WriteLine("Course already assigned to this classroom!");
        else
        {
            if (classroom.CoursesOrders != null)
            {
                var courseOrder =
                    new CourseOrder
                    {
                        CourseId = course.Id,
                        OrderNumber = classroom.CoursesOrders.Count + 1,
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
            
            course.ClassroomsIds?.Add(classroom.Id);
            classroom.Courses?.Add(course);
            context.SaveChanges();
            Console.WriteLine("Course assigned to school successfully!");
        }
    }
    
    public static void AssignToStudent(AppDbContext context,Course course)
    {
        context.Courses.Attach(course);
        Console.WriteLine("Enter Student Identifier:");
        var studentId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Students.Select(s => s.Id).Contains(studentId))
        {
            Console.WriteLine("Invalid Student Identifier! Please try again: ");
        }
        
        var student = context.Students
            .Include(hasCourse => hasCourse.Courses)
            .Include(hasCourse => hasCourse.CoursesOrders)
            .First(s => s.Id == studentId);
        
        if (student.Courses != null && student.Courses.Contains(course)) 
            Console.WriteLine("Course already assigned to this student!");
        else
        {
            context.Entry(course).Collection(e => e.Chapters).Load();
            
            if (student.CoursesOrders != null)
            {
                var courseOrder =
                    new CourseOrder
                    {
                        CourseId = course.Id,
                        OrderNumber = student.CoursesOrders.Count + 1,
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
                student.CoursesOrders?.Add(courseOrder);
            }
            course.StudentsIds?.Add(student.Id);
            student.Courses?.Add(course);
            context.SaveChanges();
            Console.WriteLine("Course assigned to student successfully!");
        }
    }
    
    private static void Remove(AppDbContext context)
    {
        Console.WriteLine("1: Remove from School");
        Console.WriteLine("2: Remove from Classroom");
        Console.WriteLine("3: Remove from Student");
        var userSelection = int.Parse(Console.ReadLine() ?? "0");

        while (userSelection is not 1 and not 2 and not 3)
        {
            Console.WriteLine("Invalid Selection! Please try again: ");
            userSelection = int.Parse(Console.ReadLine() ?? "0");
        }
        
        Console.WriteLine("Enter Course Identifier:");
        var courseId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Courses.Select(c => c.Id).Contains(courseId))
        {
            Console.WriteLine("Invalid Course Identifier! Please try again: ");
            courseId = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var selectedCourse = context.Courses.First(c => c.Id == courseId);

        switch (userSelection)
        {
            case 1:
                RemoveFromSchool(context,selectedCourse);
                break;
            case 2:
                RemoveFromClassroom(context,selectedCourse);
                break;
            case 3:
                RemoveFromStudent(context,selectedCourse);
                break;
        }
    }
    
    private static void RemoveFromSchool(AppDbContext context,Course course)
    {
        Console.WriteLine("Enter School Identifier:");
        var schoolId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Schools.Select(s => s.Id).Contains(schoolId))
        {
            Console.WriteLine("Invalid School Identifier! Please try again: ");
        }
        
        var school = context.Schools.Include(hasCourse => hasCourse.CoursesOrders)
            .First(s => s.Id == schoolId);
        
        school.CoursesOrders?.Remove(school.CoursesOrders.First(c => c.CourseId == course.Id));

        var n = 0;
        if (school.CoursesOrders != null)
            foreach (var co in school.CoursesOrders.OrderBy(c => c.OrderNumber))
            {
                co.OrderNumber = ++n;
            }

        course.SchoolsIds?.Remove(school.Id);

        school.Courses?.Remove(course);
        context.SaveChanges();
        Console.WriteLine("Course removed from school successfully!");
    }

    private static void RemoveFromClassroom(AppDbContext context,Course course)
    {
        Console.WriteLine("Enter Classroom Identifier:");
        var classroomId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Classrooms.Select(s => s.Id).Contains(classroomId))
        {
            Console.WriteLine("Invalid Classroom Identifier! Please try again: ");
        }
        
        var classroom = context.Classrooms.Include(hasCourse => hasCourse.Courses)
            .Include(hasCourse => hasCourse.CoursesOrders)
            .First(c => c.Id == classroomId);
        
        classroom.CoursesOrders?.Remove(classroom.CoursesOrders.First(c => c.CourseId == course.Id));
        
        var n = 0;
        if (classroom.CoursesOrders != null)
            foreach (var co in classroom.CoursesOrders.OrderBy(c => c.OrderNumber))
            {
                co.OrderNumber = ++n;
            }
        
        course.SchoolsIds?.Remove(classroom.Id);

        classroom.Courses?.Remove(course);
        context.SaveChanges();
        Console.WriteLine("Course removed from classroom successfully!");
    }
    
    private static void RemoveFromStudent(AppDbContext context,Course course)
    {
        Console.WriteLine("Enter Student Identifier:");
        var studentId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Students.Select(s => s.Id).Contains(studentId))
        {
            Console.WriteLine("Invalid Student Identifier! Please try again: ");
        }
        
        var student = context.Students.Include(hasCourse => hasCourse.Courses)
            .Include(hasCourse => hasCourse.CoursesOrders)
            .First(s => s.Id == studentId);
        
        student.CoursesOrders?.Remove(student.CoursesOrders.First(c => c.CourseId == course.Id));
        
        var n = 0;
        if (student.CoursesOrders != null)
            foreach (var co in student.CoursesOrders.OrderBy(c => c.OrderNumber))
            {
                co.OrderNumber = ++n;
            }
        
        course.SchoolsIds?.Remove(student.Id);

        student.Courses?.Remove(course);
        context.SaveChanges();
        Console.WriteLine("Course removed from student successfully!");
    }
    
    private static void Delete()
    {   
        Console.WriteLine("Enter Course Identifier:");
        var id = int.Parse(Console.ReadLine() ?? "0");
        
        using var context = new AppDbContext();
        var id1 = id;
        var course = context.Courses
            .FirstOrDefault(course => course != null && course.Id == id1, null);

        while (course == null)
        {
            Console.WriteLine("Invalid Id! Please try again: ");
            id = int.Parse(Console.ReadLine() ?? "0");
            var id2 = id;
            course = context.Courses
                .FirstOrDefault(course1 => course1 != null && course1.Id == id2, null);
        }
        
        foreach (var contextSchool in context.Schools.Where(s => course.SchoolsIds.Contains(s.Id))
                     .Include(hasCourse => hasCourse.CoursesOrders))
        {
            contextSchool.Courses?.Remove(course);
            contextSchool.CoursesOrders?
                .Remove(contextSchool.CoursesOrders.FirstOrDefault(c => c.CourseId == course.Id));
        }
        foreach (var contextClassroom in context.Classrooms.Where(c => course.ClassroomsIds.Contains(c.Id))
                     .Include(hasCourse => hasCourse.CoursesOrders))
        {
            contextClassroom.Courses?.Remove(course);
            contextClassroom.CoursesOrders?
                .Remove(contextClassroom.CoursesOrders.FirstOrDefault(c => c.CourseId == course.Id,null));
        }
        foreach (var contextStudent in context.Students.Where(s => course.StudentsIds.Contains(s.Id))
                     .Include(hasCourse => hasCourse.CoursesOrders))
        {
            contextStudent.Courses?.Remove(course);
            contextStudent.CoursesOrders?
                .Remove(contextStudent.CoursesOrders.FirstOrDefault(c => c.CourseId == course.Id,null));
        }
        
        context.Courses.Remove(course); 
        context.SaveChanges();
        Utilities.BackToMainMenu();
    }
    
    public static void ToManage()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("**************************");    
        Console.WriteLine("* Courses Management App *");    
        Console.WriteLine("**************************");  
        Console.ResetColor();
    
        Console.WriteLine("1: Create Course");
        Console.WriteLine("2: Display Courses");
        Console.WriteLine("3: Delete Course!");
        Console.WriteLine("0: Back to Main Menu!");
        
        var userSelection = Console.ReadLine();

        while (userSelection != "0" && userSelection != "1" && userSelection != "2" && userSelection != "3")
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
            case "0":
                Utilities.StartApp();
                break;
        }
    }
}