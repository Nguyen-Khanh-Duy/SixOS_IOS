using SixOSDatKhamAppMobile.Configurations;
using SixOSDatKhamAppMobile.Helpers;
using SixOSDatKhamAppMobile.Models;
using SixOSDatKhamAppMobile.Models.ChuyenGiaPaginate;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SixOSDatKhamAppMobile.Services.S0305
{
    public class S0305_DKGoiKhamService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = S0305_ApiConfig.BaseUrl;

        public S0305_DKGoiKhamService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(S0305_ApiConfig.DefaultTimeoutSeconds);
        }

        #region Lấy thông tin gói khám

        /// Lấy danh sách gói khám theo độ tuổi và giới tính (CÓ PAGINATION)
        public async Task<ApiResponse<List<GoiKhamDTO>>> GetGoiChiDinhTheoTuoiAsync(
            int tuoi,
            int idGioiTinh,
            long? loaiGoi = 0,
            int pageNumber = 1,
            int pageSize = 2)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/DKGoiKham/GetGoiChiDinhTheoTuoi",
                    new { tuoi, IdGT = idGioiTinh, loaiGoi, pageNumber, pageSize });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<List<GoiKhamDTO>>>();
                    return new ApiResponse<List<GoiKhamDTO>>
                    {
                        Success = result.StatusCode == 200,
                        Message = result.Message,
                        Data = result.Data
                    };
                }

                return new ApiResponse<List<GoiKhamDTO>>
                {
                    Success = false,
                    Message = "Không thể kết nối đến server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<GoiKhamDTO>>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        /// Lấy danh sách gói khám kèm theo
        public async Task<ApiResponse<List<GoiKhamDTO>>> GetGoiChiDinhKemTheoAsync()
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/DKGoiKham/GetGoiChiDinhKemTheo", null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<List<GoiKhamDTO>>>();
                    return new ApiResponse<List<GoiKhamDTO>>
                    {
                        Success = result.StatusCode == 200,
                        Message = result.Message,
                        Data = result.Data
                    };
                }

                return new ApiResponse<List<GoiKhamDTO>>
                {
                    Success = false,
                    Message = "Không thể kết nối đến server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<GoiKhamDTO>>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        /// Lấy chi tiết gói khám
        public async Task<ApiResponse<GoiKhamChiTietDTO>> LayGoiKhamChiTietAsync(long idGoiKham)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/DKGoiKham/LayGoiKhamChiTiet",
                    new { IdGoiKham = idGoiKham });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<GoiKhamChiTietDTO>>();
                    return new ApiResponse<GoiKhamChiTietDTO>
                    {
                        Success = result.StatusCode == 200,
                        Message = result.Message,
                        Data = result.Data
                    };
                }

                return new ApiResponse<GoiKhamChiTietDTO>
                {
                    Success = false,
                    Message = "Không thể kết nối đến server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<GoiKhamChiTietDTO>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        #endregion

        #region Lấy thông tin chuyên gia và lịch
        /// Lấy danh sách chuyên gia
        public async Task<ApiResponse<Models.ChuyenGiaPaginate.ChuyenGiaPaginatedResponse>> LayDanhSachChuyenGiaAsync(int pageNumber = 1, int pageSize = 6)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/DKGoiKham/LayDanhSachChuyenGia/DSCT?pageNumber={pageNumber}&pageSize={pageSize}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<Models.ChuyenGiaPaginate.ChuyenGiaPaginatedResponse>>();
                    return new ApiResponse<Models.ChuyenGiaPaginate.ChuyenGiaPaginatedResponse>
                    {
                        Success = result.StatusCode == 200,
                        Message = result.Message,
                        Data = result.Data
                    };
                }

                return new ApiResponse<Models.ChuyenGiaPaginate.ChuyenGiaPaginatedResponse>
                {
                    Success = false,
                    Message = "Không thể kết nối đến server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<Models.ChuyenGiaPaginate.ChuyenGiaPaginatedResponse>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        /// Lấy danh sách chuyên gia theo giới tính và gói khám
        public async Task<ApiResponse<List<ChuyenGiaDTO>>> LayDanhSachChuyenGiaAsync(long? idGioiTinh, long? idGoi)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/DKGoiKham/LayDanhSachChuyenGia?idGioiTinh={idGioiTinh}&idGoi={idGoi}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<List<ChuyenGiaDTO>>>();
                    return new ApiResponse<List<ChuyenGiaDTO>>
                    {
                        Success = result.StatusCode == 200,
                        Message = result.Message,
                        Data = result.Data
                    };
                }

                return new ApiResponse<List<ChuyenGiaDTO>>
                {
                    Success = false,
                    Message = "Không thể kết nối đến server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ChuyenGiaDTO>>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        /// Lấy danh sách ngày có thể đặt lịch
        public async Task<ApiResponse<List<string>>> LayNgayCoTheDatLichAsync(long idGioiTinh, long? idChuyenGia, long idGoi)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/DKGoiKham/LayNgayCoTheDatLich?idGT={idGioiTinh}&idCG={idChuyenGia}&idGoi={idGoi}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<List<string>>>();
                    return new ApiResponse<List<string>>
                    {
                        Success = result.StatusCode == 200,
                        Message = result.Message,
                        Data = result.Data
                    };
                }

                return new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = "Không thể kết nối đến server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        /// Lấy thời gian hẹn theo ngày, chuyên gia và giới tính
        public async Task<ApiResponse<List<ThoiGianHenDTO>>> LayThoiGianHenAsync(string ngay, long idChuyenGia, long idGioiTinh)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/DKGoiKham/LayThoiGianHen",
                    new { ngay, idChuyenGia, idGioiTinh });

                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                options.Converters.Add(new TimeSpanConverterHelper());

                var result = JsonSerializer.Deserialize<ApiResponse<List<ThoiGianHenDTO>>>(
                    jsonString,
                    options
                );

                return result!;
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ThoiGianHenDTO>>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}",
                    Data = null
                };
            }
        }

        #endregion

        #region Quản lý đặt hẹn

        /// Lưu đặt hẹn (bước 1: hệ thống tự xếp lịch)
        public async Task<ApiResponse<LuuDatHenResponseDTO>> LuuDatHenAsync(LuuDatHenRequestDTO request)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/DKGoiKham/LuuDatHen", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<LuuDatHenResponseDTO>>();
                    return new ApiResponse<LuuDatHenResponseDTO>
                    {
                        Success = result.StatusCode == 200 || result.StatusCode == 400,
                        Message = result.Message,
                        Data = result.Data
                    };
                }

                return new ApiResponse<LuuDatHenResponseDTO>
                {
                    Success = false,
                    Message = "Không thể kết nối đến server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<LuuDatHenResponseDTO>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        /// Chốt lịch hẹn (bước 2: xác nhận lịch)
        public async Task<ApiResponse<string>> ChotLichHenAsync(ChotLichHenRequestDTO request)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/DKGoiKham/ChotLichHen", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<string>>();
                    return new ApiResponse<string>
                    {
                        Success = result.StatusCode == 200,
                        Message = result.Message,
                        Data = result.Message
                    };
                }

                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Không thể kết nối đến server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        /// Xóa đặt hẹn
        public async Task<ApiResponse<string>> XoaDatHenAsync()
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                var idBN = await S0305_SecureStorage.GetUserIdAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var request = new { idBenhNhan = long.Parse(idBN ?? "0") };
                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/DKGoiKham/XoaDatHen", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<string>>();
                    return new ApiResponse<string>
                    {
                        Success = result.StatusCode == 200,
                        Message = result.Message
                    };
                }

                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Không thể kết nối đến server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        /// Kiểm tra lịch hẹn cũ
        public async Task<ApiResponse<LichHenCuData>> KiemTraLichHenCuAsync()
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                var idBN = await S0305_SecureStorage.GetUserIdAsync();
                var request = new { idBenhNhan = long.Parse(idBN ?? "0") };
                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/DKGoiKham/KiemTraLichHenCu", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<LichHenCuData>>();

                    // statusCode = 200: Không có lịch cũ
                    // statusCode = 400: Có lịch cũ cần xác nhận
                    return new ApiResponse<LichHenCuData>
                    {
                        Success = result.StatusCode == 200,
                        Message = result.Message,
                        Data = result.Data
                    };
                }

                return new ApiResponse<LichHenCuData>
                {
                    Success = false,
                    Message = "Không thể kết nối đến server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<LichHenCuData>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }
        #endregion

        #region Thông tin bệnh nhân và thanh toán

        /// Lấy độ tuổi và giới tính bệnh nhân
        public async Task<ApiResponse<DoTuoiGioiTinhDTO>> LayDoTuoiGioiTinhBNAsync()
        {
            try
            {
                var idBN = await S0305_SecureStorage.GetUserIdAsync();
                var request = new { idBenhNhan = long.Parse(idBN ?? "0") };
                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var token = await S0305_SecureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/DKGoiKham/LayDoTuoiGioiTinhBN", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<DoTuoiGioiTinhDTO>>();
                    return new ApiResponse<DoTuoiGioiTinhDTO>
                    {
                        Success = result.StatusCode == 200,
                        Message = result.Message,
                        Data = result.Data
                    };
                }

                return new ApiResponse<DoTuoiGioiTinhDTO>
                {
                    Success = false,
                    Message = "Không thể kết nối đến server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<DoTuoiGioiTinhDTO>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        /// Kiểm tra tình trạng khám bệnh
        public async Task<ApiResponse<KiemTraTinhTrangKhamBenhDTO>> KiemTraTinhTrangKhamBenhAsync()
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/DKGoiKham/KiemTraTinhTrangKhamBenh");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<KiemTraTinhTrangKhamBenhDTO>();
                    return new ApiResponse<KiemTraTinhTrangKhamBenhDTO>
                    {
                        Success = result.Success,
                        Message = result.Success ? "Thành công" : "Không thể đăng ký",
                        Data = result
                    };
                }

                return new ApiResponse<KiemTraTinhTrangKhamBenhDTO>
                {
                    Success = false,
                    Message = "Không thể kết nối đến server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<KiemTraTinhTrangKhamBenhDTO>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        /// Lấy thông tin thanh toán gói khám
        public async Task<ApiResponse<List<ThongTinThanhToanGoiDTO>>> LayThongTinThanhToanGoiAsync(List<long> idDkLichList)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/DKGoiKham/LayThongTinThanhToanGoi", idDkLichList);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<List<ThongTinThanhToanGoiDTO>>>();
                    return new ApiResponse<List<ThongTinThanhToanGoiDTO>>
                    {
                        Success = result.StatusCode == 200,
                        Message = result.Message,
                        Data = result.Data
                    };
                }

                return new ApiResponse<List<ThongTinThanhToanGoiDTO>>
                {
                    Success = false,
                    Message = "Không thể kết nối đến server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ThongTinThanhToanGoiDTO>>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        #endregion
    }

    #region Wrapper Response từ API
    public class ApiResponseWrapper<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
    #endregion
}