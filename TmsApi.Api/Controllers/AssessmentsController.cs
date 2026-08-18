// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using TmsApi.Infrastructure.Persistence;

// namespace TmsApi.Api.Controllers;

// [ApiController]
// [Route("api/[controller]")]
// public class AssessmentsController(TmsDbContext context) : ControllerBase
// {
//     [HttpGet]
//     public async Task<ActionResult<IEnumerable<Assessment>>> GetAssessments()
//     {
//         return await context.Assessments.ToListAsync();
//     }

//     [HttpPost]
//     public async Task<ActionResult<Assessment>> CreateAssessment(Assessment assessment)
//     {
//         context.Assessments.Add(assessment);
//         await context.SaveChangesAsync();
//         return CreatedAtAction(nameof(GetAssessments), new { id = assessment.Id }, assessment);
//     }
// }