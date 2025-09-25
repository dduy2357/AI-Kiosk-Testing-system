using API.Strategy.Interface;
using Spire.Doc;

namespace API.Strategy
{
    public class WordFileParser : IFileParser
    {
        public Task<string> ParseAsync(Stream stream)
        {
            var doc = new Document(stream);
            return Task.FromResult(doc.GetText().Replace("\r\n", "\n").Trim());
        }
    }

}
