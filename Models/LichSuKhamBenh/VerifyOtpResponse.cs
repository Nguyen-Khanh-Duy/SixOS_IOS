namespace SixOSDatKhamAppMobile.Models.LichSuKhamBenh
{
    public class VerifyOtpResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public long? IdTK { get; set; }
        public string Token { get; set; }
    }
}
