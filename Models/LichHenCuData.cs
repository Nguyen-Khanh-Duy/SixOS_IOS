namespace SixOSDatKhamAppMobile.Models
{
    public class LichHenCuData
    {
        public string LoaiDangKy { get; set; }
        public string TenGoi { get; set; }
        public string Thoigian { get; set; }
        public List<List<string>> Congviec { get; set; }
        public long? IdGoi { get; set; }
        public long? IdBenhNhan { get; set; }
        public string NgayDangKy { get; set; }
    }
}
