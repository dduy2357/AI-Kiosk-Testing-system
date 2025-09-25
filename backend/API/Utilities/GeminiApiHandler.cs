using API.Commons;
using API.Configurations;
using Newtonsoft.Json;
using System.Text;

namespace API.Utilities
{
    public static class GeminiApiHelper
    {
        private static readonly string AIApiKey = ConfigManager.gI().GeminiKey;
        private static readonly string AIUri = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

        public static async Task<T?> CallGeminiApiAsync<T>(HttpClient httpClient, string prompt)
        {
            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } }
            };

            var jsonPayload = JsonConvert.SerializeObject(requestBody, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{AIUri}?key={AIApiKey}", content);
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Gemini raw response: " + responseString);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Gemini API error: {response.StatusCode} - {responseString}");

            dynamic responseData = JsonConvert.DeserializeObject(responseString);
            if (responseData?.candidates == null || responseData.candidates.Count == 0)
                throw new Exception("Lỗi khi gọi API AI: Không nhận được phản hồi hợp lệ.");

            string aiResponse = responseData.candidates[0].content.parts[0].text;
            aiResponse = Converter.SanitizeJsonString(aiResponse);

            return JsonConvert.DeserializeObject<T>(aiResponse);
        }
    }
}
