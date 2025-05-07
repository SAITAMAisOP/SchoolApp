using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SoftyConsoleApp.Domain.AppComponents;


namespace SoftyConsoleApp.Context;
public class AppDbContext : DbContext
{
    private readonly string? _connectionString;
    public AppDbContext()
    {
        var curDir = AppContext.BaseDirectory[..AppContext.BaseDirectory.IndexOf("bin", StringComparison.Ordinal)];

        var configPath = Path.Combine(curDir, "appsettings.json");
            
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configPath)
            .Build();

        _connectionString = configuration
                                .GetConnectionString("DefaultConnection") 
                            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }
    
    public DbSet<School> Schools { get; set; }
    public DbSet<Classroom> Classrooms { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Chapter> Chapters { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);
    }
}
