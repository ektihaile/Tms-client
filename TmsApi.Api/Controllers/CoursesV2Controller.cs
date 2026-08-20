using Microsoft.AspNetCore.Mvc;

using TmsApi.Application.Dtos;
using TmsApi.Application.Exceptions;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Utilities;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v2/courses")]
[Tags("Courses v2")]
[Produces("application/json")]
public class CoursesV2Controller(
    ICourseService courseService) : ControllerBase
{
    private static readonly HashSet<string> AllowedCourseFields =
        new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(CourseResponseDto.Id),
            nameof(CourseResponseDto.Code),
            nameof(CourseResponseDto.Title),
            nameof(CourseResponseDto.MaxCapacity),
            nameof(CourseResponseDto.EnrollmentCount)
        };

    // ------------------------------------------------------------
    // GET /api/v2/courses
    // ------------------------------------------------------------

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request,
        [FromQuery] string? fields,
        CancellationToken ct)
    {
        try
        {
            var result =
                await courseService.GetCoursesAsync(request, ct);

            var shapedItems = result.Items.ShapeData(
                fields,
                AllowedCourseFields);

            var links = new List<LinkDto>
            {
                new(
                    Url.Action(
                        nameof(GetCourses),
                        new
                        {
                            page = result.Page,
                            pageSize = result.PageSize,
                            fields
                        })!,
                    "self",
                    "GET")
            };

            // Next page
            if (result.Page < result.TotalPages)
            {
                links.Add(
                    new LinkDto(
                        Url.Action(
                            nameof(GetCourses),
                            new
                            {
                                page = result.Page + 1,
                                pageSize = result.PageSize,
                                fields
                            })!,
                        "next",
                        "GET"));
            }

            // Previous page
            if (result.Page > 1)
            {
                links.Add(
                    new LinkDto(
                        Url.Action(
                            nameof(GetCourses),
                            new
                            {
                                page = result.Page - 1,
                                pageSize = result.PageSize,
                                fields
                            })!,
                        "prev",
                        "GET"));
            }

            return Ok(new
            {
                data = shapedItems,

                meta = new
                {
                    result.TotalCount,
                    result.Page,
                    result.PageSize,
                    result.TotalPages,

                    hasNext = result.Page < result.TotalPages,
                    hasPrevious = result.Page > 1
                },

                links
            });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid fields",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
    }
}