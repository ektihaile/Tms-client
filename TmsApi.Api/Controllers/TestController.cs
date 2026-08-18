using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    // Step 3 ሙከራ
    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        var query = context.Students.Where(s => s.GPA >= 3.0m);
        var orderedQuery = query.OrderBy(s => s.Name);
        var results = orderedQuery.ToList(); 
        return Ok(results);
    }

    [HttpGet("registrar/active-count")]
    public async Task<IActionResult> GetActiveCount()
    {
        var count = await context.Students.Where(s => s.IsActive && s.GPA >= 3.0m).CountAsync();
        return Ok(count);
    }
[HttpGet("average-gpa")]
public async Task<IActionResult> AverageGpa()
{
    var list = await context.Enrollments
        .GroupBy(e => e.Course.Title)
        .Select(g => new
        {
            Course = g.Key,
            AverageGPA = g.Average(e => e.Student.GPA)
        })
        .ToListAsync();

    return Ok(list);
}
    
}




