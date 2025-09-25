using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace API.Commons
{
    public static class Converter
    {
        public static string RemoveMarkVN(this string input, int type = 0)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string input2 = input.Normalize(NormalizationForm.FormD);
            string text = regex.Replace(input2, string.Empty).Replace('đ', 'd').Replace('Đ', 'D');
            return type switch
            {
                1 => text.ToLower(),
                2 => text.ToUpper(),
                _ => text,
            };
        }
        public static string RemoveMarkVNToLower(this string input) => input.RemoveMarkVN(1);
        public static string RemoveMarkVNToUpper(this string input) => input.RemoveMarkVN(2);

        public static string Standardizing(this string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            return text.RemoveMarkVNToLower();
        }

        /// xóa mọi khoảng trắng
        public static string RemoveWhitespace(this string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            return Regex.Replace(text, @"\s+", "");
        }
        public static string HashMD5(this string text)
        {
            StringBuilder sBuilder = new StringBuilder();
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(text));

                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
            }
            return sBuilder.ToString();
        }
        public static string StringToMD5(string? s, out string value)
        {
            value = null;
            try
            {
                if (string.IsNullOrEmpty(s)) return "Chuỗi đầu vào trống.";
                byte[] bytes = Encoding.ASCII.GetBytes(s);
                return BytesToMD5(bytes, out value);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public static string Normalize(string input)
        {
            return input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .Aggregate("", (s, c) => s + c);
        }
        public static string BytesToMD5(byte[] bytes, out string value)
        {
            string result = "";
            value = null;
            try
            {
                MD5 mD = MD5.Create();
                byte[] array = mD.ComputeHash(bytes);
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < array.Length; i++)
                {
                    stringBuilder.Append(array[i].ToString("X2"));
                }

                value = stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }

            return result;
        }
        public static string SanitizeJsonString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            input = input.Trim();

            // Tìm đoạn giữa ```json và ```
            int startIndex = input.IndexOf("```json");
            int endIndex = input.LastIndexOf("```");

            if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
            {
                // Lấy phần nằm giữa ```json và ```
                input = input.Substring(startIndex + 7, endIndex - (startIndex + 7)).Trim();
            }
            else
            {
                // Nếu không có ```json thì tìm đoạn JSON bắt đầu bằng [
                int jsonStart = input.IndexOf('[');
                if (jsonStart >= 0)
                {
                    input = input.Substring(jsonStart).Trim();
                }
            }

            return input;
        }

        public static bool Match(this string text, string compareText)
        {
            string text1 = text.Standardizing().Trim();
            string text2 = compareText.Standardizing().Trim();
            return text1.Contains(text2) || text2.Contains(text1);
        }

        public static string ToStringDDMMYYYY(this DateTime obj, string defaultvalue)
        {
            return obj.ToStringCustomFormat("dd/MM/yyyy", defaultvalue);
        }

        public static string ToStringDDMMYYYY24H(this DateTime obj, string defaultvalue)
        {
            return obj.ToStringCustomFormat("dd/MM/yyyy HH:mm:ss", defaultvalue);
        }

        /// xóa khoảng trắng 2 bên
        public static string RemoveExtraSpaces(this string s)
        {
            if (s == null)
                return null;
            return string.Join(" ", s.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
        }

        public static DateTime ConvertToDateTime(this string text)
        {
            DateTime result;
            if (!DateTime.TryParse(text, out result))
            {
                result = DateTime.FromOADate(double.Parse(text));
            }
            return result;
        }
        public static string ReveserString(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            char[] array = value.ToCharArray();
            Array.Reverse((Array)array);
            return new string(array);
        }

        public static string StringToDatetime(string s, out double value)
        {
            value = 0.0;
            try
            {
                value = double.Parse(s);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return "";
        }

        public static string StringToDatetime(string s, out DateTime value)
        {
            return StringToDatetime(s, null, out value);
        }

        public static string StringToDatetime(string s, string cultureInfo, out DateTime value)
        {
            value = DateTime.MinValue;
            try
            {
                if (cultureInfo != null)
                {
                    value = DateTime.Parse(s, new CultureInfo(cultureInfo, useUserOverride: true));
                }
                else
                {
                    value = DateTime.Parse(s);
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return "";
        }

        public static string ToStringCustomFormat(this DateTime obj, string format, string defaultvalue)
        {
            try
            {
                return obj.ToString(format);
            }
            catch (Exception)
            {
                return defaultvalue;
            }
        }

        public static string StringToNumber(string s, out long value)
        {
            value = 0L;
            try
            {
                value = long.Parse(s);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return "";
        }

        public static string StringToNumber(string s, out decimal value)
        {
            value = default(decimal);
            try
            {
                value = decimal.Parse(s);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return "";
        }

        public static long ToLong(this object obj, long defaultvalue)
        {
            try
            {
                return Convert.ToInt64(obj);
            }
            catch
            {
                return defaultvalue;
            }
        }

        public static string StringToDouble(string s, out double value)
        {
            value = 0.0;
            try
            {
                value = double.Parse(s);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return "";
        }
        public static double ToDouble(this object obj, double defaultvalue, IFormatProvider formatProvider = null)
        {
            try
            {
                if (formatProvider == null)
                {
                    return Convert.ToDouble(obj);
                }

                return Convert.ToDouble(obj, formatProvider);
            }
            catch
            {
                return defaultvalue;
            }
        }

        public static string StringToNumber(string s, out int value)
        {
            value = 0;
            try
            {
                value = int.Parse(s);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return "";
        }

        public static int ToNumber(this object obj, int defaultvalue)
        {
            try
            {
                return Convert.ToInt32(obj);
            }
            catch
            {
                return defaultvalue;
            }
        }

    }
}
