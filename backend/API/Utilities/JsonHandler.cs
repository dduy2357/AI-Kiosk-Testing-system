using System.Text.Json;

public static class JsonHandler
{
    public static List<string> SafeDeserializeOptions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<string>();

        try
        {
            return (JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>())
                .OrderBy(_ => Guid.NewGuid()) // 🔀 Random lại vị trí đáp án
                .ToList();
        }
        catch
        {
            return new List<string>();
        }
    }
}
