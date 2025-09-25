namespace API.Hubs
{
    public class NotifyHub : HubBase
    {
        public static readonly string RECEIVE_NEW_NOTIFY= "ReceiveNewNotification";
        public static readonly string READ_NOTIFY = "NotificationRead";
        public static readonly string READ_ALL_NOTIFY = "AllNotificationsRead";

        protected override async Task HandleCustomOnConnected(string? userId, List<int> roleIds)
        {
            if (!string.IsNullOrEmpty(userId))
            { 
                // Khi user connect, cho user join group riêng
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
               // Console.WriteLine($"NotifyHub: User {userId} joined group {userId}");
            }
        }

    }
}
