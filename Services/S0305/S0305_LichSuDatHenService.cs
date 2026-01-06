using SixOSDatKhamAppMobile.Configurations;
using SixOSDatKhamAppMobile.Models;
using SixOSDatKhamAppMobile.Models.LichSuDatHen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SixOSDatKhamAppMobile.Services.S0305
{
    public class S0305_LichSuDatHenService
    {
        private readonly string _baseUrl = S0305_ApiConfig.BaseUrl;
        private readonly HttpClient _httpClient;

        public S0305_LichSuDatHenService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(S0305_ApiConfig.DefaultTimeoutSeconds);
        }

        public async Task<LichSuKhamBenhResponse> GetLichSuKhamBenhAsync(string ngay)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                var idBN = await S0305_SecureStorage.GetUserIdAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var request = new { ngay = ngay, idBenhNhan = long.Parse(idBN ?? "0") };
                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/LichSuDatHen/GetLichSuKhamBenh",
                    content);

                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<LichSuKhamBenhResponse>(
                        responseString,
                        new JsonSerializerOptions { 
                            PropertyNameCaseInsensitive = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                        });

                    return result ?? new LichSuKhamBenhResponse
                    {
                        StatusCode = 500,
                        Message = "Không thể xử lý phản hồi từ server"
                    };
                }
                else
                {
                    return new LichSuKhamBenhResponse
                    {
                        StatusCode = (int)response.StatusCode,
                        Message = "Lỗi khi lấy dữ liệu lịch sử khám"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                return new LichSuKhamBenhResponse
                {
                    StatusCode = 500,
                    Message = $"Lỗi kết nối: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new LichSuKhamBenhResponse
                {
                    StatusCode = 500,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<DateOnly> LayNgayDatHenGanNhatAsync()
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
                    $"{_baseUrl}/LichSuDatHen/LayNgayDatHenGanNhat",
                    content);

                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<NgayDatHenGanNhatDTO>(
                        responseString,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });

                    return result?.NgayDatHen ?? DateOnly.FromDateTime(DateTime.Now);
                }
                else
                {
                    return DateOnly.FromDateTime(DateTime.Now);
                }
            }
            catch (HttpRequestException)
            {
                return DateOnly.FromDateTime(DateTime.Now);
            }
            catch (Exception)
            {
                return DateOnly.FromDateTime(DateTime.Now);
            }
        }

        public async Task<DeleteAppointmentResponse> XoaTheoNgayAsync(long id)
        {
            try
            {
                var request = new { id = id };
                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/LichSuDatHen/XoaTheoNgay",
                    content);

                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<DeleteAppointmentResponse>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return result ?? new DeleteAppointmentResponse
                    {
                        StatusCode = 500,
                        Message = "Không thể xử lý phản hồi từ server"
                    };
                }
                else
                {
                    return new DeleteAppointmentResponse
                    {
                        StatusCode = (int)response.StatusCode,
                        Message = "Lỗi khi xóa lịch hẹn"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                return new DeleteAppointmentResponse
                {
                    StatusCode = 500,
                    Message = $"Lỗi kết nối: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new DeleteAppointmentResponse
                {
                    StatusCode = 500,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<DeleteAppointmentResponse> XoaTheoGoiAsync(string ngay, long idGoi)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                var idBN = await S0305_SecureStorage.GetUserIdAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var request = new { idBenhNhan = long.Parse(idBN ?? "0"), ngay = ngay, idGoi = idGoi };
                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/LichSuDatHen/XoaTheoGoi",
                    content);

                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<DeleteAppointmentResponse>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return result ?? new DeleteAppointmentResponse
                    {
                        StatusCode = 500,
                        Message = "Không thể xử lý phản hồi từ server"
                    };
                }
                else
                {
                    return new DeleteAppointmentResponse
                    {
                        StatusCode = (int)response.StatusCode,
                        Message = "Lỗi khi xóa lịch hẹn theo gói"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                return new DeleteAppointmentResponse
                {
                    StatusCode = 500,
                    Message = $"Lỗi kết nối: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new DeleteAppointmentResponse
                {
                    StatusCode = 500,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }
    }
}
