using System.Threading.Channels;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TmsApi.Api.Hubs;
using TmsApi.Api.RateLimiting;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

using TmsApi.Infrastructure.ExternalServices;
using TmsApi.Application.Transcripts;

using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Infrastructure.Transcripts;
using TmsApi.Infrastructure.Workers;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Antiforgery;
using HealthChecks.NpgSql;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Register ProblemDetails support once
builder.Services.AddProblemDetails();

builder.Services.AddControllers();

const string ServiceName = "tms-api";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(
        serviceName: ServiceName,
        serviceVersion: "1.0.0"))
    .WithTracing(t => t
        .AddSource(ServiceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(m => m
        .AddMeter(ServiceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter());

builder.Services.AddOpenApi();

builder.Services.AddMemoryCache();
builder.Services.AddControllers()
.AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddHealthChecks()
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy("alive"),
        tags: ["live"])
  .AddNpgSql(
    connectionString: builder.Configuration.GetConnectionString("TmsDatabase")!,
    name: "postgres",
    tags: ["ready"]);

builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase")));

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ITranscriptNotifier, SignalRTranscriptNotifier>();

builder.Services.AddSingleton<
    ITranscriptStatusStore,
    InMemoryTranscriptStatusStore>();

// Transcript Channel & Worker
builder.Services.AddSingleton(
    Channel.CreateBounded<TranscriptRequest>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        }));

builder.Services.AddHostedService<TranscriptWorker>();

// Enrollment Channel & Worker (Added)
builder.Services.AddSingleton(
    Channel.CreateBounded<EnrollmentRequest>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        }));

builder.Services.AddHostedService<EnrollmentWorker>();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(2, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddMvc(); 

builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.JsonWriterOptions = new()
    {
        Indented = false
    };
});

var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());

    options.AddPolicy("TmsClient", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origin =>
            {
                var uri = new Uri(origin);
                return uri.Host == "localhost" || uri.Host == "127.0.0.1";
            });
        }
        else
        {
            policy.WithOrigins(allowedOrigins);
        }

        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("*");
    });
});

builder.Services.AddSignalR();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext =>
            {
                var (partitionKey, tier) =
                    ApiKeyResolver.Resolve(httpContext);

                return tier switch
                {
                    ApiKeyTier.Paid =>
                        RateLimitPartition.GetTokenBucketLimiter(
                            partitionKey: $"paid:{partitionKey}",
                            factory: _ =>
                                new TokenBucketRateLimiterOptions
                                {
                                    TokenLimit = 200,
                                    TokensPerPeriod = 100,
                                    ReplenishmentPeriod =
                                        TimeSpan.FromSeconds(10),
                                    QueueLimit = 0,
                                    AutoReplenishment = true
                                }),

                    ApiKeyTier.Free =>
                        RateLimitPartition.GetTokenBucketLimiter(
                            partitionKey: $"free:{partitionKey}",
                            factory: _ =>
                                new TokenBucketRateLimiterOptions
                                {
                                    TokenLimit = 30,
                                    TokensPerPeriod = 10,
                                    ReplenishmentPeriod =
                                        TimeSpan.FromSeconds(10),
                                    QueueLimit = 0,
                                    AutoReplenishment = true
                                }),

                    _ =>
                        RateLimitPartition.GetTokenBucketLimiter(
                            partitionKey: $"anon:{partitionKey}",
                            factory: _ =>
                                new TokenBucketRateLimiterOptions
                                {
                                    TokenLimit = 10,
                                    TokensPerPeriod = 5,
                                    ReplenishmentPeriod =
                                        TimeSpan.FromSeconds(10),
                                    QueueLimit = 0,
                                    AutoReplenishment = true
                                })
                };
            });

    options.AddConcurrencyLimiter(
        "transcripts",
        opt =>
        {
            opt.PermitLimit = 5;
            opt.QueueLimit = 20;
            opt.QueueProcessingOrder =
                QueueProcessingOrder.OldestFirst;
        });

    options.AddTokenBucketLimiter(
        "search",
        opt =>
        {
            opt.TokenLimit = 10;
            opt.TokensPerPeriod = 5;
            opt.ReplenishmentPeriod =
                TimeSpan.FromSeconds(10);
            opt.QueueLimit = 2;
        });

    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, ct) =>
    {
        var retryAfter = "10";

        if (context.Lease.TryGetMetadata(
                MetadataName.RetryAfter,
                out var ts))
        {
            retryAfter =
                ((int)ts.TotalSeconds).ToString();
        }

        context.HttpContext.Response.Headers.RetryAfter =
            retryAfter;

        context.HttpContext.Response.ContentType =
            "application/problem+json";

        await context.HttpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Title = "Rate limit exceeded",
                Detail =
                    $"Too many requests. Retry after {retryAfter} seconds.",
                Status =
                    StatusCodes.Status429TooManyRequests,
                Type =
                    "https://tms.local/errors/rate_limit_exceeded"
            },
            ct);
    };
});

builder.Services.AddResiliencePipeline("certificate-api", pipeline =>
{
    pipeline
        .AddTimeout(TimeSpan.FromSeconds(5))
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = 10,
            SamplingDuration = TimeSpan.FromSeconds(30),
            BreakDuration = TimeSpan.FromSeconds(15),

            ShouldHandle = new PredicateBuilder()
                .Handle<HttpRequestException>()
                .Handle<TimeoutRejectedException>(),

            OnOpened = args =>
            {
                Console.WriteLine("Circuit OPENED - stopping requests to certificate service");
                return ValueTask.CompletedTask;
            },

            OnClosed = args =>
            {
                Console.WriteLine("Circuit CLOSED - certificate service recovered");
                return ValueTask.CompletedTask;
            }
        })
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(500),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,

            ShouldHandle = new PredicateBuilder()
                .Handle<HttpRequestException>()
                .Handle<TimeoutRejectedException>(),

            OnRetry = args =>
            {
                Console.WriteLine(
                    $"Retry #{args.AttemptNumber} after {args.RetryDelay.TotalMilliseconds:F0}ms ({args.Outcome.Exception?.GetType().Name})");

                return ValueTask.CompletedTask;
            }
        });
});

builder.Services.AddHttpClient<ICertificateService, CertificateService>(
    (sp, client) =>
    {
        var baseUrl =
            sp.GetRequiredService<IConfiguration>()
                .GetValue<string>("TmsApi:PublicBaseUrl")
            ?? "http://localhost:5250";

        client.BaseAddress = new Uri(baseUrl);
    })
    .AddStandardResilienceHandler();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseRouting();
app.UseCors("TmsClient");
app.UseAuthentication();
app.UseAuthorization();

// XSRF Token issuance middleware for authenticated / active sessions
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true || context.Request.Cookies.ContainsKey("tms_auth"))
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(context);
        context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
            new CookieOptions
            {
                HttpOnly = false,
                Secure = !app.Environment.IsDevelopment(),
                SameSite = SameSiteMode.Strict
            });
    }
    await next(context);
});

app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
}).DisableRateLimiting();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
}).DisableRateLimiting();

app.MapHub<TmsHub>("/hubs/tms").RequireCors("TmsClient");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var context =
        scope.ServiceProvider.GetRequiredService<TmsDbContext>();

    await DataSeeder.SeedAsync(context);
}

var attempts = 0;

app.MapPost("/fake/certificates", async () =>
{
    var n = Interlocked.Increment(ref attempts);

    if (n % 7 == 0)
    {
        await Task.Delay(TimeSpan.FromSeconds(20));

        return Results.Ok(new
        {
            Status = "issued",
            Attempt = n
        });
    }

    if (n % 3 != 0)
    {
        return Results.StatusCode(
            StatusCodes.Status503ServiceUnavailable);
    }

    if (n % 11 == 0)
    {
        return Results.BadRequest(new
        {
            error = "validation_failed"
        });
    }

    return Results.Ok(new
    {
        Status = "issued",
        Attempt = n
    });
})
.WithTags("lab-fixtures");

app.Run();