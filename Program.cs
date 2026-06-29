using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Database service (Configured to use an In-Memory database for testing)
builder.Services.AddDbContext<DbContext>(options => options.UseInMemoryDatabase("AppDb"));

// 2. Identity and security services configuration
builder.Services.AddAuthentication();

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<DbContext>();

builder.Services.AddControllers();

var app = builder.Build();

// 3. Map Identity API Endpoints (Creates /register and /login automatically)
app.MapIdentityApi<IdentityUser>();

app.MapControllers();

// 4. Protected API Endpoint secured with authentication
app.MapGet("/api/assessments/results", () => Results.Ok(new 
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization(); // <-- This line restricts access to authorized users only

app.Run();