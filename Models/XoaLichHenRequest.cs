namespace SixOSDatKhamAppMobile.Models
{
    public class XoaLichHenRequest
    {
        public DateTime? Ngay { get; set; }
        public long Id { get; set; }
        public long IdGoi { get; set; }
        public string LoaiLich { get; set; } // "GOI", "NGAY", "CHUYENGIA"
    }
}
