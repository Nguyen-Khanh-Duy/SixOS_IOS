namespace SixOSDatKhamAppMobile.Models.Auth
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string ExpiresAt { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
