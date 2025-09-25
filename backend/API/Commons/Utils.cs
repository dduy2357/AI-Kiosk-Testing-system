using API.Helper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace API.Commons
{
    public static class Utils
    {
        public static string ObjToString(this object obj)
        {
            if (obj == null)
            {
                return "";
            }

            return obj.ToString();
        }
        public static bool IsEmpty(this string? text)
        {
            return string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text);
        }
        public static bool IsObjectEmpty<T>(this List<T>? lt)
        {
            return lt is null || lt.Count == 0;
        }
        public static bool IsObjectEmpty<T>(this IEnumerable<T>? enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }
        public static int Generate6Number()
        {
            Random random = new Random();
            return random.Next(100000, 999999);
        }
        public static string GenerateCharacter(int length)
        {
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "@$!%*?&";

            const string allChars = uppercase + lowercase + digits + special;
            var random = new Random();
            var passwordChars = new List<char>
                {
                    uppercase[random.Next(uppercase.Length)],
                    lowercase[random.Next(lowercase.Length)],
                    digits[random.Next(digits.Length)],
                    special[random.Next(special.Length)]
                };

            for (int i = passwordChars.Count; i < length; i++)
            {
                passwordChars.Add(allChars[random.Next(allChars.Length)]);
            }
            return new string(passwordChars.OrderBy(x => random.Next()).ToArray());
        }
        public static string GenRandomCharacter(int number)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, number).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string GetClientIpAddress(HttpContext httpContext)
        {
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor)) return forwardedFor.Split(',').First().Trim();
            return httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
        }
        public static string GetClientBrowser(HttpContext httpContext)
        {
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
            return userAgent.IsEmpty() ? "Unknown" : userAgent;
        }

        public static ContentResult GetCacheResponse(object cacheResponse)
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            var contentResult = new ContentResult
            {
                Content = JsonConvert.SerializeObject(cacheResponse, jsonSerializerSettings),
                ContentType = "application/json",
                StatusCode = 401
            };
            return contentResult;
        }

        public static bool IsObjectEqual<T1, T2>(this T1 obj1, T2 obj2)
        {
            if (obj1 == null || obj2 == null)
                return obj1 == null && obj2 == null;

            var properties1 = typeof(T1).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var properties2 = typeof(T2).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var properties2Dict = properties2.ToDictionary(p => p.Name, p => p);

            foreach (var property1 in properties1)
            {
                if (!properties2Dict.TryGetValue(property1.Name, out var property2) ||
                    property1.PropertyType != property2.PropertyType)
                {
                    return false;
                }

                var value1 = property1.GetValue(obj1);
                var value2 = property2.GetValue(obj2);

                // Nếu là collection, so sánh từng phần tử
                if (value1 is IEnumerable<object> list1 && value2 is IEnumerable<object> list2)
                {
                    if (!list1.SequenceEqual(list2))
                        return false;
                }
                // Nếu là object phức tạp, kiểm tra đệ quy
                else if (property1.PropertyType.IsClass && !property1.PropertyType.IsPrimitive && !property1.PropertyType.IsValueType)
                {
                    if (!IsObjectEqual(value1, value2))
                        return false;
                }
                // So sánh giá trị đơn giản
                else if (!Equals(value1, value2))
                {
                    return false;
                }
            }
            return true;
        }

        public static string DowLoadFileFromUrl(string url, out MemoryStream memoryStream)
        {
            memoryStream = null;
            try
            {
                using (var client = new WebClient())
                {
                    var content = client.DownloadData(url);
                    memoryStream = new MemoryStream(content);
                }
            }
            catch (Exception ex)
            {
                return ex.Message + ". Stacktrace: " + ex.StackTrace;
            }
            return "";
        }

        public static async Task<(string, string?)> GetUrlImage(IFormFile file)
        {
            var (error, fileNames) = await GetUrlImages(new[] { file });
            if (!string.IsNullOrEmpty(error))
            {
                return (error, null);
            }

            return (string.Empty, fileNames?.FirstOrDefault());
        }

        public static async Task<(string, List<string>?)> GetUrlImages(IFormFile[] files)
        {
            List<string> fileNames = new List<string>();
            var allowedExtensions = Constant.IMAGE_EXTENDS; // Định dạng file được phép

            foreach (var file in files)
            {
                if (file.Length > 1048576) // Giới hạn kích thước 1MB
                {
                    return ("The file is too large (<= 1MB).", null);
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return ("Invalid file format.", null);
                }

                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), Constant.UrlImagePath);
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + extension; // Tạo tên file duy nhất
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                fileNames.Add(uniqueFileName);
            }

            return (string.Empty, fileNames);
        }
    }

    public static class GuidHelper
    {
        public static Guid ToGuid(this string guid)
        {
            return string.IsNullOrEmpty(guid) ? Guid.Empty : Guid.Parse(guid);
        }
        public static string ToString(this Guid? guid)
        {
            // Kiểm tra nếu Guid là null, trả về chuỗi rỗng
            return guid?.ToString() ?? string.Empty;
        }
        public static bool IsGuidEmpty(this Guid guid)
        {
            return guid == Guid.Empty;
        }
        public static bool IsGuid(string guid)
        {
            return Guid.TryParse(guid, out _); // Trả về true nếu chuỗi là GUID hợp lệ, ngược lại trả về false
        }
        public static bool IsNullOrEmpty(this Guid? guid)
        {
            return !guid.HasValue || guid == Guid.Empty;
        }

    }

    //add more class extensions here ...

}
