
namespace SixOSDatKhamAppMobile.Models.ChuyenGiaPaginate
{
    public class ChuyenGiaPaginatedResponse
    {
        public List<ChuyenGiaDTO> Data { get; set; }
        public PaginationInfo Pagination { get; set; }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
    }
}
