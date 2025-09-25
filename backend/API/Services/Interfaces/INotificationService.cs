using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface INotificationService
    {
        public Task<(string, SearchResult?)> GetAlert(NotifySearchVM search, string usertoken);
        public Task<(string, NotifyDetailVM?)> GetOne(string id);
        public Task<string> Create(NotificationCreateVM notificationVM, string usertoken);
        public Task<string> MarkAsRead(string id);
        public Task<string> MarkAllAsRead(string usertoken);
        public Task<(string, int)> GetTotalUnread(string usertoken);
        public Task<string> Delete(List<string> ids);
        public Task SendToAdmins(string usertoken, string message);

    }
}
