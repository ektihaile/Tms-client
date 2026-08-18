using System;
namespace TmsApi.Domain.Entities;


public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    
    
    public string? Grade { get; set; }
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public int Year { get; set; }
    public bool IsArchived { get; set; }
}