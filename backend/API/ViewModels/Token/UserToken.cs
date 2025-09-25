namespace API.ViewModels.Token
{
    public class UserToken
    {
        public string? UserID { get; set; } = string.Empty;
        public string? FullName { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; } = string.Empty;
        public string? LastLoginIp { get; set; } = string.Empty;
        public List<int>? RoleId { get; set; } = new();
    }

    public class ResultObject
    {
        public bool isOk { get; set; } = false;
        public string Message { get; set; } = string.Empty;
    }
}
