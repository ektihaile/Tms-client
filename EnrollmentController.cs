using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentController : ControllerBase
{
    private readonly IEnrollmentService _service;

    public EnrollmentController(IEnrollmentService service)
    {
        _service = service;
    }

    // ---------------- CREATE (POST) ----------------
    [HttpPost]
    public async Task<IActionResult> Enroll(string studentId, string courseCode)
    {
        var result = await _service.EnrollAsync(studentId, courseCode);
        return Ok(result);
    }

    // ---------------- GET BY ID ----------------
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    // ---------------- DELETE ----------------
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result)
            return NotFound();

        return Ok(result);
    }
}




