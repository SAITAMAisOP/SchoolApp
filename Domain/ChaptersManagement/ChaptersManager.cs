using Microsoft.EntityFrameworkCore;
using SoftyConsoleApp.Context;
using SoftyConsoleApp.Domain.AppComponents;
using SoftyConsoleApp.Domain.CoursesManagement;

namespace SoftyConsoleApp.Domain.ChaptersManagement;

public static class ChaptersManager
{
    private static void Create()
    {
        Console.WriteLine("Enter Chapter Name:");
        var chapterName = Console.ReadLine() ?? "";

        while (string.IsNullOrWhiteSpace(chapterName))
        {
            Console.WriteLine("Invalid Name! Please try again: ");
            chapterName = Console.ReadLine() ?? "";
        }

        using var context = new AppDbContext();
        var chapter = new Chapter(chapterName);
        context.Chapters.Add(chapter);
        context.SaveChanges();

        ToManage();
    }
    
    private static void Display()
    {
        using var context = new AppDbContext();
        
        Console.WriteLine("Chapters:");
        foreach (var ch in context.Chapters)
        {
            Console.WriteLine($"-> {ch.Id}. {ch.Name}");
        }
        
        Console.WriteLine("1: Assign Chapter");
        Console.WriteLine("2: Remove Chapter");
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
         
    }

    private static void Assign(AppDbContext context)
    {
        Console.WriteLine("1: Assign to School");
        Console.WriteLine("2: Assign to Classroom");
        Console.WriteLine("3: Assign to Student");
        Console.WriteLine("4: Assign to Course");
        Console.WriteLine("0: Back to Main Menu");
        
        var userSelection = int.Parse(Console.ReadLine() ?? "0");
        while (userSelection is not 1 and not 2 and not 3 and not 4 and not 0)
        {
            Console.WriteLine("Invalid Selection! Please try again: ");
            userSelection = int.Parse(Console.ReadLine() ?? "0");
        }
        
        Console.WriteLine("Enter Chapter Identifier:");
        var chapterId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Chapters.Select(c => c.Id).Contains(chapterId))
        {
            Console.WriteLine("Invalid Course Identifier! Please try again: ");
            chapterId = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var selectedChapter = context.Chapters.First(c => c.Id == chapterId);

        switch (userSelection)
        {
            case 1:
                var oneChapterCourse = GetChapterCourse(context, selectedChapter);
                CoursesManager.AssignToSchool(context,oneChapterCourse);
                break;
            case 2:
                var oneChapterCourse1 = GetChapterCourse(context, selectedChapter);
                CoursesManager.AssignToClassroom(context,oneChapterCourse1);
                break;
            case 3:
                var oneChapterCourse2 = GetChapterCourse(context, selectedChapter);
                CoursesManager.AssignToStudent(context,oneChapterCourse2);
                break;
            case 4:
                AssignToCourse(context,selectedChapter);
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

    private static Course? GetChapterCourse(AppDbContext context, Chapter selectedChapter)
    {
        var oneChapterCourse = new Course("One Chapter Course")
        {
            Chapters =
            [
                selectedChapter
            ],
            IsOneChapter = true
        };

        context.Courses.Add(oneChapterCourse);
        context.SaveChanges();

        return oneChapterCourse;
    }

    private static void AssignToCourse(AppDbContext context,Chapter chapter)
    {
        Console.WriteLine("Enter Course Identifier:");
        var courseId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Courses.Select(c => c.Id).Contains(courseId))
        {
            Console.WriteLine("Invalid Course Identifier! Please try again: ");
            courseId = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var selectedCourse = context.Courses.Include(course => course.Chapters)
            .FirstOrDefault(c => c.Id == courseId);
        
        if (selectedCourse?.Chapters != null && selectedCourse.Chapters.Contains(chapter))
        {
            Console.WriteLine("Course already assigned to this course!");
        }
        else
        {
            //Update every school linked to the course
            if (selectedCourse?.SchoolsIds is { Count: > 0 }) foreach (var id in selectedCourse.SchoolsIds)
            {
                var school = context.Schools.Include(hasCourse => hasCourse.CoursesOrders)
                    .FirstOrDefault(s => s.Id == id);

                school?.CoursesOrders?
                    .Add(new CourseOrder(){CourseId = selectedCourse.Id, OrderNumber = school.CoursesOrders.Count + 1});
            }
            //Update every classroom linked to the course
            if (selectedCourse?.ClassroomsIds is { Count: > 0 }) foreach (var id in selectedCourse.ClassroomsIds)
            {
                var classroom = context.Schools
                    .Include(hasCourse => hasCourse.CoursesOrders)
                    .FirstOrDefault(s => s.Id == id);

                classroom?.CoursesOrders?
                    .Add(new CourseOrder(){CourseId = selectedCourse.Id, OrderNumber = classroom.CoursesOrders.Count + 1});
            }
            //Update every student linked to the course
            if (selectedCourse?.StudentsIds is { Count: > 0 }) foreach (var id in selectedCourse.StudentsIds)
            {
                var student = context.Schools
                    .Include(hasCourse => hasCourse.CoursesOrders)
                    .FirstOrDefault(s => s.Id == id);

                student?.CoursesOrders?
                    .Add(new CourseOrder(){CourseId = selectedCourse.Id, OrderNumber = student.CoursesOrders.Count + 1});
            }
            
            selectedCourse?.Chapters?.Add(chapter);
            context.SaveChanges();
            Console.WriteLine("Course assigned to course successfully!");
        }
    }
    
    private static void Remove(AppDbContext context)
    {
        Console.WriteLine("1: Remove from School");
        Console.WriteLine("2: Remove from Classroom");
        Console.WriteLine("3: Remove from Student");
        Console.WriteLine("4: Remove from Course");
        Console.WriteLine("0: Back to Main Menu");
        
        var userSelection = int.Parse(Console.ReadLine() ?? "0");
        while (userSelection is not 1 and not 2 and not 3 and not 4 and not 0)
        {
            Console.WriteLine("Invalid Selection! Please try again: ");
            userSelection = int.Parse(Console.ReadLine() ?? "0");
        }
        
        Console.WriteLine("Enter Chapter Identifier:");
        var chapterId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Chapters.Select(c => c.Id).Contains(chapterId))
        {
            Console.WriteLine("Invalid Course Identifier! Please try again: ");
            chapterId = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var selectedChapter = context.Chapters.First(c => c.Id == chapterId);

        switch (userSelection)
        {
            case 1:
                RemoveFromSchool(selectedChapter);
                break;
            case 2:
                RemoveFromClassroom(selectedChapter);
                break;
            case 3:
                RemoveFromStudent(selectedChapter);
                break;
            case 4:
                RemoveFromCourse(selectedChapter);
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

    private static void RemoveFromSchool(Chapter chapter)
    {
        using var context = new AppDbContext();
        Console.WriteLine("Enter School Identifier:");
        var schoolId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Schools.Select(s => s.Id).Contains(schoolId))
        {
            Console.WriteLine("Invalid School Identifier! Please try again: ");
        }
        
        var school = context.Schools.Include(hasCourse => hasCourse.Courses)!
            .ThenInclude(course => course.Chapters).Include(hasCourse => hasCourse.CoursesOrders)
            .First(s => s.Id == schoolId);

        if (school.Courses != null)
        {
            var course =
                school.Courses.First(c => c.Chapters != null && c.Chapters.Contains(chapter) && c.IsOneChapter);
            
            course.SchoolsIds?.Remove(schoolId);

            var toDelete = school.CoursesOrders?.FirstOrDefault(co => co.CourseId == course.Id);
            if (toDelete != null) school.CoursesOrders?.Remove(toDelete);
        }
        

        context.Schools.Update(school);
        context.SaveChanges();
        Console.WriteLine("Chapter removed from school successfully!");
    }

    private static void RemoveFromClassroom(Chapter chapter)
    {
        using var context = new AppDbContext();
        Console.WriteLine("Enter Classroom Identifier:");
        var classroomId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Classrooms.Select(s => s.Id).Contains(classroomId))
        {
            Console.WriteLine("Invalid Classroom Identifier! Please try again: ");
        }
        
        var classroom = context.Classrooms.Include(hasCourse => hasCourse.Courses)
            .ThenInclude(course => course.Chapters).Include(hasCourse => hasCourse.CoursesOrders)
            .First(s => s.Id == classroomId);

        if (classroom.Courses != null)
        {
            var course =
                classroom.Courses.First(c => c.Chapters != null && c.Chapters.Contains(chapter) && c.IsOneChapter);
            
            course.SchoolsIds?.Remove(classroomId);

            var toDelete = classroom.CoursesOrders?.FirstOrDefault(co => co.CourseId == course.Id);
            if (toDelete != null) classroom.CoursesOrders?.Remove(toDelete);
        }

        context.Classrooms.Update(classroom);
        context.SaveChanges();
        Console.WriteLine("Chapter removed from classroom successfully!");
    }

    private static void RemoveFromStudent(Chapter chapter)
    {
        using var context = new AppDbContext();
        Console.WriteLine("Enter Student Identifier:");
        var studentId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Students.Select(s => s.Id).Contains(studentId))
        {
            Console.WriteLine("Invalid Student Identifier! Please try again: ");
        }
        
        var student = context.Students.Include(hasCourse => hasCourse.Courses)
            .ThenInclude(course => course.Chapters).Include(hasCourse => hasCourse.CoursesOrders)
            .First(s => s.Id == studentId);

        if (student.Courses != null)
        {
            var course =
                student.Courses.First(c => c.Chapters != null && c.Chapters.Contains(chapter) && c.IsOneChapter);
            
            course.SchoolsIds?.Remove(studentId);

            var toDelete = student.CoursesOrders?.FirstOrDefault(co => co.CourseId == course.Id);
            if (toDelete != null) student.CoursesOrders?.Remove(toDelete);
        }

        context.Students.Update(student);
        context.SaveChanges();
        Console.WriteLine("Chapter removed from student successfully!");
    }

    private static void RemoveFromCourse(Chapter chapter)
    {
        using var context = new AppDbContext();
        Console.WriteLine("Enter Course Identifier:");
        var courseId = int.Parse(Console.ReadLine() ?? "0");

        while (!context.Courses.Select(s => s.Id).Contains(courseId))
        {
            Console.WriteLine("Invalid Course Identifier! Please try again: ");
            courseId = int.Parse(Console.ReadLine() ?? "0");
        }
        
        var course = context.Courses.First(c => c.Id == courseId);
        
        course.Chapters?.Remove(chapter);
        context.Courses.Update(course);
        context.SaveChanges();
        Console.WriteLine("Chapter removed from Course successfully!");
    }
    
    private static void Delete()
    {   
        Console.WriteLine("Enter Chapter Identifier:");
        var toDeleteId = int.Parse(Console.ReadLine() ?? "0");
        
        using var context = new AppDbContext();
        var id1 = toDeleteId;
        var chapter = context.Chapters
            .FirstOrDefault(chapter => chapter != null && chapter.Id == id1, null);

        while (chapter == null)
        {
            Console.WriteLine("Invalid Id! Please try again: ");
            toDeleteId = int.Parse(Console.ReadLine() ?? "0");
            var id2 = toDeleteId;
            chapter = context.Chapters
                .FirstOrDefault(chapter1 => chapter1 != null && chapter1.Id == id2, null);
        }
        var selectedCourse = context.Courses
            .First(c => c.Chapters != null && c.Chapters.Contains(chapter) && c.IsOneChapter);
        
        //Update every school linked to the course
        if (selectedCourse?.SchoolsIds is { Count: > 0 }) foreach (var id in selectedCourse.SchoolsIds)
        {
            var school = context.Schools.Include(hasCourse => hasCourse.CoursesOrders)
                .FirstOrDefault(s => s.Id == id);

            var toRemove = school?.CoursesOrders?.FirstOrDefault(co => co.CourseId == selectedCourse.Id);
            if (toRemove != null)
                school?.CoursesOrders?
                    .Remove(toRemove);
        }
        //Update every classroom linked to the course
        if (selectedCourse?.ClassroomsIds is { Count: > 0 }) foreach (var id in selectedCourse.ClassroomsIds)
        {
            var classroom = context.Schools
                .Include(hasCourse => hasCourse.CoursesOrders)
                .FirstOrDefault(s => s.Id == id);

            var toRemove = classroom?.CoursesOrders?.FirstOrDefault(co => co.CourseId == selectedCourse.Id);
            if (toRemove != null)
                classroom?.CoursesOrders?
                    .Remove(toRemove);
        }
        //Update every student linked to the course
        if (selectedCourse?.StudentsIds is { Count: > 0 }) foreach (var id in selectedCourse.StudentsIds)
        {
            var student = context.Schools
                .Include(hasCourse => hasCourse.CoursesOrders)
                .FirstOrDefault(s => s.Id == id);
                
            var toRemove = student?.CoursesOrders?.FirstOrDefault(co => co.CourseId == selectedCourse.Id);
            if (toRemove != null)
                student?.CoursesOrders?
                    .Remove(toRemove);
        }
        context.Chapters.Remove(chapter); 
        context.SaveChanges();
        
        Utilities.BackToMainMenu();
    }
    
    public static void ToManage()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("***************************");    
        Console.WriteLine("* Chapters Management App *");    
        Console.WriteLine("***************************");  
        Console.ResetColor();
    
        Console.WriteLine("1: Create Chapter");
        Console.WriteLine("2: Display All Chapters");
        Console.WriteLine("3: Delete Chapter");
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