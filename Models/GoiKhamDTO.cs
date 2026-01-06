namespace SixOSDatKhamAppMobile.Models
{
    public class GoiKhamDTO
    {
        public int Stt { get; set; }
        public long Id { get; set; }
        public string TenGoi { get; set; }
        public bool? GoiCoBan { get; set; }
        public bool? GoiNangCao { get; set; }
        public int? TuoiMin { get; set; }
        public int? TuoiMax { get; set; }
        public string HuongDan { get; set; }
        public bool? GoiKemTheo { get; set; }
        public decimal TongTien { get; set; }
        public List<GoiKhamChiTietItemDTO> ChiTiet { get; set; }
    }

    public class GoiKhamChiTietDTO
    {
        public List<GoiKhamChiTietItemDTO> ChiTiet { get; set; }
    }

    public class GoiKhamChiTietItemDTO
    {
        public long Id { get; set; }
        public int? Stt { get; set; }
        public string TenDichVu { get; set; }
        public decimal? DonGiaDichVu { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; }
    }
}
