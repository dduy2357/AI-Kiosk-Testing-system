using API.Models;
using API.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace API.Repository
{
    public class RoomRepository : IRoomRepository
    {
        private readonly Sep490Context _context;

        public RoomRepository(Sep490Context context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(string roomId)
        {
            return await _context.Rooms.AnyAsync(r => r.RoomId == roomId);
        }
    }

}
