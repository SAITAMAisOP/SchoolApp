using Microsoft.EntityFrameworkCore;
using SoftyConsoleApp.Context;
using SoftyConsoleApp.Contracts;
using SoftyConsoleApp.Domain.AppComponents;
using SoftyConsoleApp.Domain.ChaptersManagement;
using SoftyConsoleApp.Domain.ClassroomsManagement;
using SoftyConsoleApp.Domain.CoursesManagement;
using SoftyConsoleApp.Domain.SchoolsManagement;
using SoftyConsoleApp.Domain.StudentsManagement;
using Console = System.Console;

namespace SoftyConsoleApp;
public abstract class Utilities
{
    public static void StartApp()
    { 
        Console.Clear();
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
    
        string? userSelection;
    
        do
        {
            userSelection = Console.ReadLine();
            switch (userSelection)
            {
                case "1":
                    SchoolsManager.ToManage();
                    break;
                case "2":
                    ClassroomsManager.ToManage();
                    break;
                case "3":
                    StudentsManager.ToManage();
                    break;
                case "4":
                    CoursesManager.ToManage();
                    break;
                case "5":
                    ChaptersManager.ToManage();
                    break;
                case "0":
                    break;
                default:
                    Console.WriteLine("Invalid selection please try again!");
                    break;
            }
    
        } while (userSelection != "0");
        
    }

    public static void RefactorCourses(AppDbContext context, HasCourse entity)
    {
        context.Attach(entity);
        context.Entry(entity).Collection(e => e.CoursesOrders).Load();
        if (entity.CoursesOrders != null)
            foreach (var co in entity.CoursesOrders.OrderBy(c => c.OrderNumber))
            {
                var course = context.Courses.Include(course => course.Chapters)
                    .First(c => c.Id == co.CourseId);
                if (course.Chapters != null)
                    Console.WriteLine(course.IsOneChapter
                        ? $"• Single Chapter :"
                        : $"• Course {co.OrderNumber}: {context.Courses
                            .First(c => c.Id == co.CourseId).Name}\t ID: {co.CourseId}");
                context.Entry(co).Collection(e => e.ChaptersOrders).Load();
            foreach (var chapter in co.ChaptersOrders.OrderBy(c => c.OrderNumber))
                {
                    Console.WriteLine($" - Chapter {chapter.OrderNumber}: {context.Chapters
                        .First(c => c.Id == chapter.ChapterId).Name}\t ID: {chapter.ChapterId}");
                }
            }
        
        Console.WriteLine("\n");
        Console.WriteLine("1: Edit Course");
        Console.WriteLine("2: Edit Courses Order");
        Console.WriteLine("0: Back to Main Menu");
        
        var userSelection = int.Parse(Console.ReadLine() ?? "0");
        while (userSelection != 1 && userSelection != 2 && userSelection != 0)
        {
            Console.Write("Invalid selection. Please try again: ");
            userSelection = int.Parse(Console.ReadLine() ?? "0");
        }

        switch (userSelection)
        {
            case 1:
                EditCourse(context, entity);
                break;
            case 2:
                ReOrderCourses(context, entity);
                break;
            case 0:
                StartApp();
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

    private static void EditCourse(AppDbContext context, HasCourse entity)
    {
        Console.WriteLine("Enter Course Identifier:");
        var courseId = int.Parse(Console.ReadLine() ?? "0");
        
        var courseOrder = entity.CoursesOrders?
            .FirstOrDefault(c => c != null && c.CourseId == courseId, null);

        while (entity.CoursesOrders != null
               && courseOrder == null)
        {
            Console.Write("Invalid selection. Please try again: ");
            courseId = int.Parse(Console.ReadLine() ?? "0");
            courseOrder = entity.CoursesOrders?.FirstOrDefault(c => c != null && c.Id == courseId, null);
        }
        
        Console.WriteLine("Enter Chapter Identifier:");
        var chapterId = int.Parse(Console.ReadLine() ?? "0");
        
        var chapterOrder = courseOrder?.ChaptersOrders
            .FirstOrDefault(c => c != null && c.ChapterId == chapterId, null);

        while (chapterOrder == null)
        {
            Console.Write("Invalid selection. Please try again: ");
            chapterId = int.Parse(Console.ReadLine() ?? "0");
            chapterOrder = courseOrder?.ChaptersOrders
                .FirstOrDefault(c => c != null && c.ChapterId == chapterId, null);
        }
        
        Console.WriteLine("Enter new position:");
        var placeIndex = int.Parse(Console.ReadLine() ?? "0");

        while (placeIndex <= 0 || placeIndex > courseOrder?.ChaptersOrders.Count)
        {
            Console.Write("Invalid selection. Please try again: ");
            placeIndex = int.Parse(Console.ReadLine() ?? "0");
        }

        if (placeIndex < chapterOrder.OrderNumber)
        {
            courseOrder?.ChaptersOrders
                .Where(c => c.OrderNumber >= placeIndex)
                .ToList()
                .ForEach(c => c.OrderNumber++);
        }

        if (placeIndex < chapterOrder.OrderNumber)
        {
            courseOrder?.ChaptersOrders
                .Where(c => c.OrderNumber <= placeIndex)
                .ToList()
                .ForEach(c => c.OrderNumber--);
        }
        
        chapterOrder.OrderNumber = placeIndex;
        
        var n = 0;
        foreach (var course in courseOrder?.ChaptersOrders.OrderBy(c => c.OrderNumber)!)
        {
            course.OrderNumber = ++n;
        }
        
        context.SaveChanges();
        Console.WriteLine("Course updated successfully!");
        BackToMainMenu();
    }

    private static void ReOrderCourses(AppDbContext context, HasCourse entity)
    {
        Console.WriteLine("Enter Course Identifier:");
        var courseId = int.Parse(Console.ReadLine() ?? "0");
        
        var courseOrder = entity.CoursesOrders?.FirstOrDefault(c => c != null && c.CourseId == courseId, null);

        while (entity.CoursesOrders != null
               && courseOrder == null)
        {
                Console.Write("Invalid selection. Please try again: ");
                courseId = int.Parse(Console.ReadLine() ?? "0");
                courseOrder = entity.CoursesOrders?.FirstOrDefault(c => c != null && c.Id == courseId, null);
        }
        
        Console.WriteLine("Enter new position:");
        var placeIndex = int.Parse(Console.ReadLine() ?? "0");

        while (entity.CoursesOrders != null && (placeIndex <= 0 || placeIndex > entity.CoursesOrders.Count ))
        {
            Console.Write("Invalid selection. Please try again: ");
            placeIndex = int.Parse(Console.ReadLine() ?? "0") - 1;
        }

        if (courseOrder != null && placeIndex < courseOrder.OrderNumber) 
        {
            entity.CoursesOrders?
                .Where(c => c.OrderNumber >= placeIndex )
                .ToList().ForEach(c => c.OrderNumber++);
        }
        else if (courseOrder != null && placeIndex > courseOrder.OrderNumber)
        {
            entity.CoursesOrders?
                .Where(c => c.OrderNumber <= placeIndex)
                .ToList().ForEach(c => c.OrderNumber--);
        }

        if (courseOrder != null) courseOrder.OrderNumber = placeIndex;

        var n = 0;
        if (entity.CoursesOrders != null)
            foreach (var course in entity.CoursesOrders.OrderBy(c => c.OrderNumber))
            {
                course.OrderNumber = ++n;
            }
        

        context.SaveChanges();
        Console.WriteLine("Order updated successfully!");
        BackToMainMenu();
    }

    public static void BackToMainMenu()
    {
        Console.WriteLine("Press any key to return to the main menu...");
        Console.ReadKey();
        StartApp();
    }
}
