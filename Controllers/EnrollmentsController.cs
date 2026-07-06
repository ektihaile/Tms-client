using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/enrollments")]
// 👈 እዚህ ጋር OpenEnrollmentService የነበረው ወደ ያንተ ትክክለኛ ሰርቪስ ተቀይሯል
public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
{
    // 1. GET /api/enrollments (ሁሉንም መዝገቦች ከሰርቪሱ ያመጣል)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var enrollments = await enrollmentService.GetAllAsync();
        return Ok(enrollments);
    }

    // 2. GET /api/enrollments/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var record = await enrollmentService.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound();
    }

    // 3. POST /api/enrollments
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        var record = await enrollmentService.EnrollAsync(request.StudentId, request.CourseCode);
        
        if (record is null)
        {
            return BadRequest("Enrollment failed.");
        }

        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    // 4. DELETE /api/enrollments/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await enrollmentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

public record CreateEnrollmentRequest(string StudentId, string CourseCode);