using Microsoft.AspNetCore.SignalR;

namespace TmsApi.Api.Hubs;

public class TmsHub : Hub<ITmsHubClient>
{
    public override async Task OnConnectedAsync()
    {
        try
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext != null)
            {
                var studentId = httpContext.Request.Query["studentId"].ToString();

                if (!string.IsNullOrWhiteSpace(studentId))
                {
                    await Groups.AddToGroupAsync(
                        Context.ConnectionId,
                        $"student:{studentId}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error in OnConnectedAsync: {ex.Message}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext != null)
            {
                var studentId = httpContext.Request.Query["studentId"].ToString();

                if (!string.IsNullOrWhiteSpace(studentId))
                {
                    await Groups.RemoveFromGroupAsync(
                        Context.ConnectionId,
                        $"student:{studentId}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error in OnDisconnectedAsync: {ex.Message}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}