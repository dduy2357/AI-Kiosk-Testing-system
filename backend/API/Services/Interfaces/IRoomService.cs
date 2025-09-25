using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IRoomService
    {
        public Task<(string, SearchResult?)> GetAllRooms(SearchRoomVM search);
        public Task<(string, RoomVM?)> GetRoomByIdAsync(string roomId);
        public Task<string> ChangeActivateRoom(string roomId, string usertoken);
        public Task<string> CreateUpdateRoomVM(CreateUpdateRoomVM roomVM, string usertoken);
        public Task<string> DoRemoveRoom(string roomId, string usertoken);
    }

}
