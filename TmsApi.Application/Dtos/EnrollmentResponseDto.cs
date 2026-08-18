namespace TmsApi.Application.Dtos;

public class EnrollmentResponseDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string StudentEmail { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public DateTime EnrolledAtUtc { get; set; }
}
