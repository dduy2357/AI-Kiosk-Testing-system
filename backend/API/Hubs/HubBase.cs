using API.Helper; 
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace API.Hubs
{
    public abstract class HubBase : Hub
    {
        protected static readonly ConcurrentDictionary<string, string> _connectedUsers = new();

        public override async Task OnConnectedAsync()
        {
            var user = Context.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                await Clients.Caller.SendAsync("ConnectionFailed", "Not authenticated. Please login.");
                Context.Abort();
                await OnDisconnectedAsync(null);
                return;
            }

            var userId = user.FindFirst("UserID")?.Value;
            if (!string.IsNullOrEmpty(userId))
                _connectedUsers.TryAdd(Context.ConnectionId, userId);

            var roleIds = user.FindFirst("RoleIds")?.Value?
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(int.Parse).ToList() ?? new();

            if (!roleIds.Contains((int)RoleEnum.Student))
                await Groups.AddToGroupAsync(Context.ConnectionId, ConstMessage.LOG_EXAM_VIEWERS);

            if (roleIds.Contains((int)RoleEnum.Admin))
                await Groups.AddToGroupAsync(Context.ConnectionId, ConstMessage.LOG_VIEWERS);

            await Clients.Caller.SendAsync("ConnectionSuccess", $"Welcome, {user.Identity?.Name ?? "user"}");

            // Cho phép hub con thực thi logic riêng
            await HandleCustomOnConnected(userId, roleIds);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connectedUsers.TryRemove(Context.ConnectionId, out var userId))
            {
                Console.WriteLine($"User [{userId}] disconnected: {Context.ConnectionId}, Reason: {exception?.Message ?? "Unknown"}");
            }
            else
            {
                Console.WriteLine($"Unknown user disconnected: {Context.ConnectionId}, Reason: {exception?.Message ?? "Unknown"}");
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ConstMessage.LOG_EXAM_VIEWERS);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ConstMessage.LOG_VIEWERS);

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Cho Hub con override nếu cần xử lý thêm.
        /// </summary>
        protected virtual Task HandleCustomOnConnected(string? userId, List<int> roleIds)
        {
            return Task.CompletedTask;
        }
    }
}
