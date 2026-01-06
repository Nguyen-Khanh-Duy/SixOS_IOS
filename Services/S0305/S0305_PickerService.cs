using SixOSDatKhamAppMobile.Configurations;
using SixOSDatKhamAppMobile.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SixOSDatKhamAppMobile.Services.S0305
{
    public class S0305_PickerService
    {
        private readonly string _baseUrl = S0305_ApiConfig.BaseUrl;
        private readonly HttpClient _httpClient;

        public S0305_PickerService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(S0305_ApiConfig.DefaultTimeoutSeconds);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // Lấy tất cả dữ liệu picker một lần
        public async Task<PickerDataResponse> GetAllPickerDataAsync()
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync($"{_baseUrl}/DoDuLieuPicker/all-data");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PickerDataResponse>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return result ?? new PickerDataResponse();
                }

                return new PickerDataResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting picker data: {ex.Message}");
                return new PickerDataResponse();
            }
        }

        // Lấy quận/huyện theo tỉnh/thành
        public async Task<List<PickerDataDto>> GetQuanHuyenByTinhIdAsync(long tinhId)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync($"{_baseUrl}/DoDuLieuPicker/quan-huyen/{tinhId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<PickerDataDto>>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return result ?? new List<PickerDataDto>();
                }

                return new List<PickerDataDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting quan/huyen: {ex.Message}");
                return new List<PickerDataDto>();
            }
        }

        // Lấy xã/phường theo tỉnh/thành
        public async Task<List<PickerDataDto>> GetXaPhuongByTinhIdAsync(long tinhId)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync($"{_baseUrl}/DoDuLieuPicker/xa-phuong/tinh/{tinhId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<PickerDataDto>>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return result ?? new List<PickerDataDto>();
                }

                return new List<PickerDataDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting xa/phuong: {ex.Message}");
                return new List<PickerDataDto>();
            }
        }

        // Lấy xã/phường theo quận/huyện
        public async Task<List<PickerDataDto>> GetXaPhuongByQuanIdAsync(long quanId)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync($"{_baseUrl}/DoDuLieuPicker/xa-phuong/quan/{quanId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<PickerDataDto>>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return result ?? new List<PickerDataDto>();
                }

                return new List<PickerDataDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting xa/phuong by quan: {ex.Message}");
                return new List<PickerDataDto>();
            }
        }
    }

    // Các model cho dữ liệu picker
    public class PickerDataDto
    {
        public long Id { get; set; }
        public string Ma { get; set; }
        public string Ten { get; set; }
        public string VietTat { get; set; }
        public bool Active { get; set; }
    }

    public class PickerDataResponse
    {
        public List<PickerDataDto> GioiTinh { get; set; } = new List<PickerDataDto>();
        public List<PickerDataDto> DanToc { get; set; } = new List<PickerDataDto>();
        public List<PickerDataDto> TinhThanh { get; set; } = new List<PickerDataDto>();
        public List<PickerDataDto> QuocGia { get; set; } = new List<PickerDataDto>();
        public List<PickerDataDto> NgheNghiep { get; set; } = new List<PickerDataDto>();
    }
}