namespace TmsApi.Domain.Entities;


public class Student
{
    public int Id { get; set; } 
    public required string RegistrationNumber { get; set; } 
    public required string Name { get; set; }
     public string Email { get; set; } = "";
        public decimal GPA { get; set; }
    public bool IsActive { get; set; } = true;

    public uint Version { get; set; }

    // Exercise 9 - Soft delete
    public bool IsDeleted { get; set; } = false;

    // Navigation property for many-to-many relationship
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}