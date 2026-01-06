using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using SixOSDatKhamAppMobile.Configurations;
using SixOSDatKhamAppMobile.Models;
using SixOSDatKhamAppMobile.Models.Response;

namespace SixOSDatKhamAppMobile.Services.S0305
{
    public class S0305_HoSoService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = S0305_ApiConfig.BaseUrl;

        public S0305_HoSoService()
        {
            _httpClient = new HttpClient();
        }

        /// Lấy danh sách hồ sơ bệnh nhân
        public async Task<HoSoListResponse> LayDanhSachHoSoAsync(string? keyTimKiem = null, bool timTatCa = false)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return new HoSoListResponse
                    {
                        Success = false,
                        Message = "Chưa đăng nhập"
                    };
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var queryParams = $"?timTatCa={timTatCa}";
                if (!string.IsNullOrEmpty(keyTimKiem))
                {
                    queryParams += $"&keyTimKiem={Uri.EscapeDataString(keyTimKiem)}";
                }

                var response = await _httpClient.GetAsync($"{_baseUrl}/HoSo/danh-sach{queryParams}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.statusCode == 200)
                    {
                        var data = JsonSerializer.Deserialize<List<HoSoBenhNhan>>(
                            result.data?.ToString() ?? "[]",
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        return new HoSoListResponse
                        {
                            Success = true,
                            Message = result.message ?? "Thành công",
                            Data = data ?? new List<HoSoBenhNhan>()
                        };
                    }

                    return new HoSoListResponse
                    {
                        Success = false,
                        Message = result?.message ?? "Lỗi không xác định"
                    };
                }

                return new HoSoListResponse
                {
                    Success = false,
                    Message = $"Lỗi kết nối: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new HoSoListResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        /// Chọn bệnh nhân để xem thông tin
        public async Task<BaseResponse> ChonBenhNhanAsync(long idBenhNhan)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return new BaseResponse { Success = false, Message = "Chưa đăng nhập" };
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/HoSo/chon-benh-nhan/{idBenhNhan}",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new BaseResponse
                    {
                        Success = result?.statusCode == 200,
                        Message = result?.message ?? "Không xác định"
                    };
                }

                return new BaseResponse
                {
                    Success = false,
                    Message = $"Lỗi: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse { Success = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        /// Lấy thông tin chi tiết bệnh nhân
        public async Task<HoSoDetailResponse> LayThongTinBenhNhanAsync(long idBenhNhan)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return new HoSoDetailResponse
                    {
                        Success = false,
                        Message = "Chưa đăng nhập"
                    };
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{_baseUrl}/HoSo/{idBenhNhan}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.statusCode == 200)
                    {
                        var data = JsonSerializer.Deserialize<HoSoDetail>(
                            result.data?.ToString() ?? "{}",
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        return new HoSoDetailResponse
                        {
                            Success = true,
                            Message = result.message ?? "Thành công",
                            Data = data
                        };
                    }

                    return new HoSoDetailResponse
                    {
                        Success = false,
                        Message = result?.message ?? "Lỗi không xác định"
                    };
                }

                return new HoSoDetailResponse
                {
                    Success = false,
                    Message = $"Lỗi: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new HoSoDetailResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        // Helper classes
        private class ApiResponse
        {
            public int statusCode { get; set; }
            public string? message { get; set; }
            public object? data { get; set; }
        }
    }
}