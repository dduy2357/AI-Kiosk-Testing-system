using API.Strategy.Interface;

namespace API.Strategy
{
    public class TextFileParser : IFileParser
    {
        public async Task<string> ParseAsync(Stream stream)
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
    }
}
