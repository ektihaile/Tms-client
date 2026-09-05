using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using TmsApi.Application.Interfaces;

namespace TmsApi.Infrastructure.Workers;

public class EnrollmentRequest
{
    public string EnrollmentId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}

public class EnrollmentWorker : BackgroundService
{
    readonly ILogger<EnrollmentWorker> _logger;
    readonly ChannelReader<EnrollmentRequest> _reader;
    readonly IServiceScopeFactory _scopeFactory;

    public EnrollmentWorker(
        ILogger<EnrollmentWorker> logger,
        Channel<EnrollmentRequest> channel,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _reader = channel.Reader;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Enrollment worker started.");

        await foreach (var request in _reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

                _logger.LogInformation("Processing background enrollment task for ID: {EnrollmentId}", request.EnrollmentId);

            

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing enrollment task.");
            }
        }
    }
}