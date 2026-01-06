namespace SixOSDatKhamAppMobile.Models
{
    public class GoiKham
    {
        public long Id { get; set; }
        public string TenGoiKham { get; set; }
        public string MoTa { get; set; }
        public string HuongDanHtml { get; set; }
        public decimal GiaTien { get; set; }
        public int ThoiGianHieuLuc { get; set; }
        public string DanhMuc { get; set; }
        public string GioiTinh { get; set; }
        public string DoTuoi { get; set; }
        public string LoaiGoi { get; set; }
        public string BadgeText { get; set; }
        public string BadgeColor { get; set; }
        public List<string> CacXetNghiem { get; set; }
    }
}
