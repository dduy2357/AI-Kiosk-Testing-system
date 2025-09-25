namespace API.ViewModels
{
    public class ConfigsVM
    {
        public List<string>? ShortcutKeys { get; set; }
        public List<string>? BlockedApps { get; set; }
        public List<string>? WhiteListApps { get; set; } 
        public string ProtectedUrl { get; set; } = string.Empty;  
    }
}
