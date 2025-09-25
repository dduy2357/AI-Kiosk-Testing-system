using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.Net;
using System.Reflection;

namespace API.Utilities
{
    public static class FileHandler
    {
        #region Import data
        public static string ImportFromExcel<T>(IFormFile excelFile, out List<T> result) where T : new()
        {
            result = new List<T>();
            if (excelFile == null || excelFile.Length == 0)
                return "No file found or file is empty.";

            try
            {
                using var stream = new MemoryStream();
                excelFile.CopyTo(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                    return "No found worksheet in file.";

                var properties = typeof(T).GetProperties();
                var columnMapping = GetColumnMapping(worksheet, properties);

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    var item = new T();

                    foreach (var kvp in columnMapping)
                    {
                        int col = kvp.Key;
                        var prop = kvp.Value;

                        var cellValue = worksheet.Cells[row, col].Text?.Trim();
                        if (string.IsNullOrWhiteSpace(cellValue)) continue;

                        try
                        {
                            object? convertedValue = ConvertCellValue(cellValue, prop.PropertyType);
                            prop.SetValue(item, convertedValue);
                        }
                        catch
                        {
                            // Bỏ qua lỗi khi chuyển đổi kiểu
                            continue;
                        }
                    }

                    result.Add(item);
                }
            }
            catch (Exception ex)
            {
                return "Lỗi khi đọc file: " + ex.Message;
            }

            return "";
        }
        private static Dictionary<int, PropertyInfo> GetColumnMapping(ExcelWorksheet worksheet, PropertyInfo[] properties)
        {
            var mapping = new Dictionary<int, PropertyInfo>();

            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                var header = worksheet.Cells[1, col].Text?.Trim();
                if (string.IsNullOrEmpty(header)) continue;

                var prop = properties.FirstOrDefault(p => p.Name.Equals(header, StringComparison.OrdinalIgnoreCase));
                if (prop != null)
                {
                    mapping[col] = prop;
                }
            }

            return mapping;
        }
        private static object? ConvertCellValue(string value, Type targetType)
        {
            if (targetType == typeof(string)) return value;

            var nullableType = Nullable.GetUnderlyingType(targetType);
            if (nullableType != null)
            {
                if (string.IsNullOrEmpty(value)) return null;
                targetType = nullableType;
            }

            if (targetType.IsEnum)
                return Enum.Parse(targetType, value);

            if (targetType == typeof(bool))
                return value.Equals("1") || value.Equals("true", StringComparison.OrdinalIgnoreCase);

            if (targetType == typeof(DateTime))
                return DateTime.Parse(value);

            if (targetType == typeof(int))
                return int.Parse(value);

            if (targetType == typeof(List<int>))
            {
                return value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => int.TryParse(s.Trim(), out var i) ? i : 0)
                            .Where(i => i > 0)
                            .ToList();
            }

            return Convert.ChangeType(value, targetType);
        }
        #endregion

        #region Export data

        public static MemoryStream GenerateExcelFile<T>(List<T>? data)
        {
            if (data?.Any() != true) return new MemoryStream();
            var stream = new MemoryStream();
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Sheet1");

            // 1. Lấy danh sách các property
            var properties = typeof(T).GetProperties();

            // 2. Ghi header (tên cột)
            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];
                var col = i + 1;
                worksheet.Cells[1, col].Value = prop.Name;
                worksheet.Cells[1, col].Style.Font.Bold = true;

                Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                worksheet.Column(col).Width = propType switch
                {
                    Type t when t == typeof(string) => 30,
                    Type t when t == typeof(DateTime) => 18,
                    Type t when t == typeof(bool) => 10,
                    Type t when t.IsEnum => 20,
                    Type t when t == typeof(int) || t == typeof(long) || t == typeof(short) => 12,
                    Type t when t == typeof(decimal) || t == typeof(double) || t == typeof(float) => 15,
                    Type t when t == typeof(Guid) => 36,
                    Type t when t == typeof(byte) => 8,
                    _ => 25 // Default fallback
                };
            }

            // 3. Ghi dữ liệu
            for (int row = 0; row < data.Count; row++)
            {
                var user = data[row];
                for (int col = 0; col < properties.Length; col++)
                {
                    var prop = properties[col];
                    var value = prop.GetValue(user);
                    switch (value)
                    {
                        case DateTime dt:
                            worksheet.Cells[row + 2, col + 1].Value = dt;
                            worksheet.Cells[row + 2, col + 1].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                            break;
                        case decimal or double or float:
                            worksheet.Cells[row + 2, col + 1].Value = Convert.ToDouble(value);
                            worksheet.Cells[row + 2, col + 1].Style.Numberformat.Format = "#,##0.##";
                            break;
                        case IEnumerable<int> intList:
                            worksheet.Cells[row + 2, col + 1].Value = string.Join(", ", intList);
                            break;
                        default:
                            worksheet.Cells[row + 2, col + 1].Value = value?.ToString() ?? "";
                            break;
                    }
                }
            }
            //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            package.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        public static MemoryStream GenerateExcelLargeFile<T>(List<T> list)
        {
            if (list == null || !list.Any())
                return new MemoryStream();

            var dt = ToFormattedDataTable(list);
            var stream = new MemoryStream();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                worksheet.Cells["A1"].LoadFromDataTable(dt, true);

                // Bold header row
                worksheet.Cells[1, 1, 1, dt.Columns.Count].Style.Font.Bold = true;

                // Format columns based on original property types
                var properties = typeof(T).GetProperties();
                for (int col = 1; col <= dt.Columns.Count; col++)
                {
                    var propType = Nullable.GetUnderlyingType(properties[col - 1].PropertyType) ?? properties[col - 1].PropertyType;

                    worksheet.Column(col).Style.Numberformat.Format = propType switch
                    {
                        Type t when t == typeof(DateTime) => "dd/MM/yyyy HH:mm",
                        Type t when t == typeof(decimal) || t == typeof(double) || t == typeof(float) => "#,##0.##",
                        _ => "@" // Text format for all other types
                    };
                }
                //worksheet.Cells.AutoFitColumns();
                package.SaveAs(stream);
            }
            stream.Position = 0;
            return stream;
        }

        public static DataTable ToFormattedDataTable<T>(List<T> data)
        {
            var table = new DataTable(typeof(T).Name);
            var properties = typeof(T).GetProperties();

            // Create columns with correct data types
            foreach (var prop in properties)
            {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                // Use string for collections, bool, enum, or any IEnumerable except string
                bool isCollection = type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
                table.Columns.Add(prop.Name, isCollection || type == typeof(bool) || type.IsEnum ? typeof(string) : type);
            }

            foreach (var item in data)
            {
                var row = table.NewRow();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item);
                    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    row[prop.Name] = value switch
                    {
                        null => DBNull.Value,
                        DateTime dt => dt,
                        IEnumerable<int> intList => string.Join(", ", intList),
                        IEnumerable<string> strList => string.Join(", ", strList),
                        bool b => b ? "Yes" : "No",
                        decimal or double or float => Convert.ToDecimal(value),
                        Enum => value.ToString(),
                        System.Collections.IEnumerable enumerable when propType != typeof(string) => string.Join(", ", enumerable.Cast<object>()),
                        _ => value
                    };
                }
                table.Rows.Add(row);
            }
            return table;
        }

        #endregion
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

        public static string WriteToSheet(IFormFile templateFile, Dictionary<string, List<string>> data, out MemoryStream fileOutput, string? uploadFileName = null, string? userName = null)
        {
            fileOutput = new MemoryStream();
            if (templateFile == null || templateFile.Length == 0)
                return "Template file is empty or null.";

            ExcelStyle styles = null;

            try
            {
                using var fi = new MemoryStream();
                templateFile.CopyTo(fi);
                fi.Position = 0;

                using var package = new ExcelPackage(fi);
                var workbook = package.Workbook;
                var ranges = workbook.Names;

                if (ranges == null || ranges.Count == 0) return "Template file does not contain named ranges.";

                foreach (var namedRange in ranges)
                {
                    if (data.TryGetValue(namedRange.Name, out var columnData))
                    {
                        for (int rowIndex = namedRange.Start.Row, dataIndex = 0; rowIndex <= namedRange.End.Row && dataIndex < columnData.Count; rowIndex++, dataIndex++)
                        {
                            for (int columnIndex = namedRange.Start.Column; columnIndex <= namedRange.End.Column; columnIndex++)
                            {
                                if (rowIndex == namedRange.Start.Row && columnIndex == namedRange.Start.Column)
                                    styles = namedRange.Worksheet.Cells[rowIndex, columnIndex].Style;

                                if (columnIndex == namedRange.Start.Column)
                                    namedRange.Worksheet.Cells[rowIndex, columnIndex].Value = columnData[dataIndex];

                                CopyCellStyle(namedRange.Worksheet.Cells[rowIndex, columnIndex], styles);
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(userName))
                {
                    var ws = workbook.Worksheets.First();
                    ws.Cells[3, 3].Value = DateTime.UtcNow .ToString("dd/MM/yyyy");
                    ws.Cells[4, 3].Value = userName;
                }

                fileOutput = new MemoryStream(); // << tạo mới và KHÔNG dispose
                package.SaveAs(fileOutput);
                fileOutput.Position = 0;
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message + " --StackTrace: " + ex.StackTrace;
            }
        }
        private static void CopyCellStyle(ExcelRange range, ExcelStyle style)
        {
            try
            {
                if (style == null) return;
                range.Style.Font = style.Font;
                range.Style.Font.Bold = style.Font.Bold;
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                range.Style.Border.Top.Style = style.Border.Top.Style;
                range.Style.Border.Left.Style = style.Border.Left.Style;
                range.Style.Border.Right.Style = style.Border.Right.Style;
                range.Style.Border.Bottom.Style = style.Border.Bottom.Style;
                range.Style.HorizontalAlignment = style.HorizontalAlignment;
                range.Style.Fill = style.Fill;
                range.Style.Numberformat = style.Numberformat;
                range.Style.Indent = style.Indent;
                range.Style.WrapText = style.WrapText;
                range.Style.ShrinkToFit = style.ShrinkToFit;
                range.Style.TextRotation = style.TextRotation;
                range.Style.VerticalAlignment = style.VerticalAlignment;
            }
            catch (Exception ex)
            {
                //     Log.WriteErrorLog(ex.Message);
                Console.WriteLine(ex.Message);
                return;
            }
        }
    }
}
