using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IRoomUserService
    {
        public Task<(string, SearchResult?)> GetUsersNotInRoom(SearchUserRoomExamVM search);
        public Task<(string, SearchResult?)> GetUsersInRoom(SearchRoomUserVM search);
        public Task<string> RemoveUsersFromRoom(string roomId, List<string> userIds, string usertoken);
        public Task<(string, List<string>, List<string>, List<string>)> AddStudentsToRoom(string roomId, List<string> userIds, string usertoken);
        public Task<string> UpdateRoomUser(UpdateRoomUserVM input, string usertoken);
        public Task<string> AssignTeacherToRoom(string roomId, string userId, string usertoken);
        public Task<(string, MemoryStream?)> Export(string? roomId);
        public Task<(string, object?)> Import(IFormFile fileData, string roomId, string usertoken);
        public Task<string> ToggleActive(ToggleActiveRoomUserVM input, string usertoken);
     
    }
}
