using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 🛑 ይህንን መስመር አጥፋው (አያስፈልግም፣ launchSettings.json ይቆጣጠረዋል)
// builder.Environment.EnvironmentName = "Production"; 

// DI Validation
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Database & Identity Config
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("AppDb"));

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<IdentityUser>()
   .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllers();

// =============================================================
// ✅ ማስተካከያ፦ በ Development ጊዜ የ HTML ገጹን የሚቀይር የሰርቪስ ቅንብር
// =============================================================
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        // ይህ ባዶ ቅጥያ (Extension) .NET የ HTML ገጹን እንዳያሳይ ያስገድደዋል
        context.ProblemDetails.Extensions["tms_error"] = "ProblemDetails active";
    };
});

builder.Services.AddOpenApi();        

// Services & Workers
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Options Validation
builder.Services
    .AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

// =============================================================
// MIDDLEWARE PIPELINE (የሚድልዌር ቅደም ተከተል)
// =============================================================

// 🛑 ማስተካከያ፦ በ Dev ሞድም ቢሆን Exceptionን በ JSON እንዲተረጉም እንነግረዋለን
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    AllowStatusCode404Response = true
});

app.UseStatusCodePages();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// OpenAPI & Scalar ድጋፍ 
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); 
}

app.MapControllers();

// የቆየው የቴስት ዳታ አጠቃቀም (የእኛ ሎግ ተርሚናል ላይ ያየነው)
using (var scope = app.Services.CreateScope())
{
    var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
    enrollmentService.EnrollAsync("S-001", "CS-101").GetAwaiter().GetResult();
    enrollmentService.EnrollAsync("S-001", "CS-101").GetAwaiter().GetResult();
}

app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization();

// Exercise 6: ሆን ብሎ ስህተት የሚፈጥር የቴስት ኤንድፖይንት
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});

app.Run();

public class TmsDatabaseException(string message) : System.Exception(message);