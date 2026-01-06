using SixOSDatKhamAppMobile.Models.Response;

namespace SixOSDatKhamAppMobile.Models
{
    public class BenhVien : BaseResponse
    {
        public long IdDoiTac { get; set; }
        public string? TenDoiTac { get; set; }
        public string? DiaChi { get; set; }
    }
}
