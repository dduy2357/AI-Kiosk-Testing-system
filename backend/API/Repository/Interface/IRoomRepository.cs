namespace API.Repository.Interface
{
    public interface IRoomRepository
    {
        Task<bool> ExistsAsync(string roomId);

    }
}
