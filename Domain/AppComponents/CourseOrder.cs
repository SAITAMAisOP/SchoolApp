namespace SoftyConsoleApp.Domain.AppComponents;

public class CourseOrder
{
    public int Id { get; set; }
    
    public int CourseId { get; set; } 
    
    public int OrderNumber { get; set; }
    
    public List<ChapterOrder> ChaptersOrders { get; set;}
}