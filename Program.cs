using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add this
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("AppDb"));

// Authentication
builder.Services.AddAuthentication();

builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<IdentityUser>()
   .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllers();

// // Exercise 2
// builder.Services.AddSingleton<EnrollmentWorker>();
// builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// // Exercise 3
// builder.Services
//     .AddOptions<PaymentOptions>()
//     .BindConfiguration("Payments")
//     .ValidateDataAnnotations()
//     .ValidateOnStart();

var app = builder.Build();
app.UseMiddleware<RequestLoggingMiddleware>();
app.MapIdentityApi<IdentityUser>();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization();

app.Run();