namespace SixOSDatKhamAppMobile.Models
{
    public class PatientInfoRequest
    {
        public long UserId { get; set; }
        public string HoTen { get; set; }
        public string NgaySinh { get; set; }
        public long GioiTinhId { get; set; }
        public string CCCD { get; set; }
        public long QuocTichId { get; set; }
        public long NgheNghiepId { get; set; }
        public long DanTocId { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChi { get; set; }
        public long TinhThanhId { get; set; }
        public long PhuongXaId { get; set; }
        public string Email { get; set; }
    }
}
