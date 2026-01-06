namespace SixOSDatKhamAppMobile.Models
{
    public class DatKhamInfo
    {
        public BenhVien BenhVien { get; set; }
        public GoiKham GoiKham { get; set; }
        public ChuyenGia ChuyenGia { get; set; }
        public DateTime NgayKham { get; set; }
        public TimeSpan GioKham { get; set; }
        public KhungGioKham KhungGio { get; set; }
        public DateTime ThoiGianDat { get; set; }
        public decimal TongTien { get; set; }
    }
}
