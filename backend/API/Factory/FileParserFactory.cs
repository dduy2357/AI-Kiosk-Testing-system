using API.Strategy.Interface;
using API.Strategy;

namespace API.Factory
{
    /**  
     *  Không cần dùng interface cho factory để dễ sử dụng
     *  Nếu các parser có tham số thì cần tạo interface để không phải thay đổi factory,
     *  không cần khởi tạo hay thay đổi constructor. 
     */
    public class FileParserFactory               
    {
        public static IFileParser GetParser(string extension)
        {
            return extension.ToLower() switch
            {
                ".doc" or ".docx" => new WordFileParser(),
                ".txt" => new TextFileParser(),
                ".pdf" => new PdfFileParser(),
                _ => throw new NotSupportedException($"Unsupported file type: {extension}")
            };
        }
    }
}
