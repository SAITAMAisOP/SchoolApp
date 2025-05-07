using Microsoft.EntityFrameworkCore;
using SoftyConsoleApp.Context;
using SoftyConsoleApp.Contracts;
using SoftyConsoleApp.Domain.AppComponents;

namespace SoftyConsoleApp.Domain.SchoolsManagement;

public static class SchoolsManager 
{
    private static void Create()
    {
        Console.WriteLine("Enter School Name:");
        var schoolName = Console.ReadLine() ?? "";

        while (string.IsNullOrWhiteSpace(schoolName))
        {
            Console.WriteLine("Invalid Name! Please try again: ");
            schoolName = Console.ReadLine() ?? "";
        }

        using var context = new AppDbContext();
        var school = new School(schoolName);
        context.Schools.Add(school);
        context.SaveChanges();
        
        ToManage();
    }

    private static void Display()
    {
        using var context = new AppDbContext();
        Console.WriteLine("Enter School Identifier:");
        var id = int.Parse(Console.ReadLine() ?? "0");
        
        while (!context.Schools.Select(s => s.Id).Contains(id))
        {
            Console.WriteLine("Invalid Id! Please try again: ");
            id = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var school = context.Schools.Include(hasCourse => hasCourse.Courses)!
            .ThenInclude(course => course.Chapters).First(s => s.Id == id);
        
        Console.WriteLine($"School : {school?.Name}");
        
        if (school?.Courses == null) return;
        
        Utilities.RefactorCourses(context,school);

        context.Schools.Update(school);
    }

    private static void Delete()
    {   
        Console.WriteLine("Enter School Identifier:");
        var id = int.Parse(Console.ReadLine() ?? "0");
        
        using var context = new AppDbContext();
        var id1 = id;
        var school = context.Schools
            .FirstOrDefault(school => school != null && school.Id == id1, null);

        while (school == null)
        {
            Console.WriteLine("Invalid Id! Please try again: ");
            id = int.Parse(Console.ReadLine() ?? "0");
            var id2 = id;
            school = context.Schools
                .FirstOrDefault(school1 => school1 != null && school1.Id == id2, null);
        }
        
        context.Schools.Remove(school); 
        context.SaveChanges();
        
        Console.WriteLine("School Deleted Successfully!");
        Utilities.BackToMainMenu();
    }
    
    public static  void ToManage()
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("**************************");    
        Console.WriteLine("* Schools Management App *");    
        Console.WriteLine("**************************");  
        Console.ResetColor();
    
        Console.WriteLine("1: Create School");
        Console.WriteLine("2: Display School Courses");
        Console.WriteLine("3: Delete School!");
        Console.WriteLine("0: Back to Main Menu!");
        
        var userSelection = Console.ReadLine();

        while (userSelection != "0" 
               && userSelection != "1"
               && userSelection != "2" 
               && userSelection != "3"
               && userSelection != null)
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