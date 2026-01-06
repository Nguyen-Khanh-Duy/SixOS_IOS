namespace SixOSDatKhamAppMobile.Models
{
    public class ThongTinThanhToanGoiDTO
    {
        public string CoSoKham { get; set; }
        public string KhuKham { get; set; }
        public List<ThanhToanChiTietDTO> DanhSachTaiChinh { get; set; }
    }

    public class ThanhToanChiTietDTO
    {
        public long IdDkL { get; set; }
        public long IdTaiChinh { get; set; }
        public decimal Gia { get; set; }
    }
}
