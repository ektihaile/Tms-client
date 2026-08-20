namespace TmsApi.Application.Dtos;

public static class CourseDtoFields
{
    public static readonly HashSet<string> Allowed =
        new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(CourseResponseDto.Id),
            nameof(CourseResponseDto.Code),
            nameof(CourseResponseDto.Title),
            nameof(CourseResponseDto.MaxCapacity),
            nameof(CourseResponseDto.EnrollmentCount)
        };
}