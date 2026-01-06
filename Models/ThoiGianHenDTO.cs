namespace SixOSDatKhamAppMobile.Models
{
    public class ThoiGianHenDTO
    {
        public long ID { get; set; }
        public TimeSpan? ThoiGianBatDau { get; set; }
        public TimeSpan? ThoiGianKetThuc { get; set; }
        public long IdBuoi { get; set; }
        public bool Active { get; set; }
        public int TrangThai { get; set; }
    }
}
