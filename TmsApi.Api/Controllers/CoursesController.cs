
namespace TmsApi.Api.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CoursesController(
    ICourseService courseService,
    LinkGenerator linkGenerator) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List courses with pagination")]
    [EndpointDescription("Returns a paginated, optionally filtered list of TMS courses. PageSize is capped at 50.")]
    [ProducesResponseType(typeof(PagedResponse<CourseResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var result = await courseService.GetCoursesAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [EndpointSummary("Get a course by ID")]
    [EndpointDescription("Returns course details with HATEOAS links. Returns 404 if the course does not exist.")]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);
        if (course is null)
        {
            return NotFound();
        }

        var selfUrl = linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id })
                      ?? $"/api/courses/{id}";

        var enrollmentsUrl = linkGenerator.GetPathByName(HttpContext, "ListCourseEnrollments", new { courseId = id })
                             ?? $"/api/courses/{id}/enrollments";

        var links = new List<LinkDto>
{
    new(selfUrl, "self", "GET"),
    new(enrollmentsUrl, "enrollments", "GET")
};

if (course.EnrollmentCount < course.MaxCapacity)
{
    links.Add(new LinkDto(enrollmentsUrl, "enroll", "POST"));
}

        var detailDto = new CourseDetailDto
        {
            Id = course.Id,
            Code = course.Code,
            Title = course.Title,
            MaxCapacity = course.MaxCapacity,
            EnrollmentCount = course.EnrollmentCount,
            Links = links
        };

        return Ok(detailDto);
    }

    [HttpPost]
    [EndpointSummary("Create a new course")]
    [EndpointDescription("Creates a course with a unique code. Returns 409 if the course code already exists.")]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CourseResponseDto>> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        if (await courseService.CodeExistsAsync(request.Code, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }

        var result = await courseService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
    }
}