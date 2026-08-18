using Microsoft.AspNetCore.SignalR;

namespace TmsApi.Api.Hubs;

public class TmsHub : Hub<ITmsHubClient>
{
    public override async Task OnConnectedAsync()
    {
        var studentId = Context.GetHttpContext()?.Request.Query["studentId"].ToString();

        if (!string.IsNullOrWhiteSpace(studentId))
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"student:{studentId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var studentId = Context.GetHttpContext()?.Request.Query["studentId"].ToString();

        if (!string.IsNullOrWhiteSpace(studentId))
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"student:{studentId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}