using API.Strategy.Interface;

namespace API.Strategy
{
    public class PdfFileParser : IFileParser
    {
        public Task<string> ParseAsync(Stream stream)
        {
            throw new NotSupportedException("Currently the function only supports DOC and TXT files.");
        }
    }
}
