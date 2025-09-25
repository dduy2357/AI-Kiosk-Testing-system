using API.Commons;
using API.Helper;
using API.Models;
using API.Utilities;
using API.ViewModels;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace API.Hubs
{
    public class LogHub : HubBase
    {
        private readonly ILog _log;

        public LogHub(ILog logService)
        {
            _log = logService;
        }

        public async Task SendLogExam(AddExamLogVM log)
        {
            if (string.IsNullOrEmpty(log.UserId)
                && _connectedUsers.TryGetValue(Context.ConnectionId, out var userId))
            {
                log.UserId = userId;
            }

            await _log.WriteActivity(log);

            var logView = new UserLogListVM
            {
                LogId = Guid.NewGuid().ToString(),
                UserCode = "",
                FullName = "",
                ActionType = log.ActionType,
                Description = log.Description,
                CreatedAt = DateTime.UtcNow
            };

            await Clients.Group(ConstMessage.LOG_EXAM_VIEWERS)
                         .SendAsync("ReceiveLog", logView);
        }
    }


}
