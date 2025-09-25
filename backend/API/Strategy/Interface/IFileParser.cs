namespace API.Strategy.Interface
{
    public interface IFileParser
    {
        Task<string> ParseAsync(Stream stream);
    }

}
