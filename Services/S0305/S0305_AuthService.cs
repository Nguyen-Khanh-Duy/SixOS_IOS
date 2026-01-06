using SixOSDatKhamAppMobile.Configurations;
using SixOSDatKhamAppMobile.Models.Auth;
using System.Text;
using System.Text.Json;

namespace SixOSDatKhamAppMobile.Services.S0305
{
    public class S0305_AuthService
    {
        private readonly string _baseUrl = S0305_ApiConfig.BaseUrl;
        private readonly HttpClient _httpClient;

        public S0305_AuthService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(S0305_ApiConfig.DefaultTimeoutSeconds);
        }

        public async Task<LoginResponse> LoginAsync(string cccd, string password)
        {
            try
            {
                var loginRequest = new LoginRequest
                {
                    Cccd = cccd,
                    MatKhau = password
                };

                var jsonContent = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/Auth/login",
                    content);

                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return loginResponse ?? new LoginResponse
                    {
                        Success = false,
                        Message = "Không thể xử lý phản hồi từ server"
                    };
                }
                else
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = $"Kiểm tra lại số điện thoại hoặc mật khẩu"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Lỗi kết nối: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<SendOtpResponse> SendOtpAsync(string cccd, string dienThoai)
        {
            try
            {
                var sendOtpRequest = new SendOtpRequest
                {
                    Cccd = cccd,
                    DienThoai = dienThoai
                };

                var jsonContent = JsonSerializer.Serialize(sendOtpRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/Auth/send-otp",
                    content);

                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var sendOtpResponse = JsonSerializer.Deserialize<SendOtpResponse>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return sendOtpResponse ?? new SendOtpResponse
                    {
                        Success = false,
                        Message = "Không thể xử lý phản hồi từ server"
                    };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<SendOtpResponse>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return errorResponse ?? new SendOtpResponse
                    {
                        Success = false,
                        Message = $"Lỗi {response.StatusCode}: Gửi OTP thất bại"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                return new SendOtpResponse
                {
                    Success = false,
                    Message = $"Lỗi kết nối: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new SendOtpResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<VerifyOtpResponse> VerifyOtpAndRegisterAsync(string cccd, string dienThoai, string otp, string matKhau)
        {
            try
            {
                var verifyRequest = new VerifyOtpRequest
                {
                    Cccd = cccd,
                    DienThoai = dienThoai,
                    Otp = otp,
                    MatKhau = matKhau,
                };

                var jsonContent = JsonSerializer.Serialize(verifyRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/Auth/verify-otp-and-register",
                    content);

                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var verifyResponse = JsonSerializer.Deserialize<VerifyOtpResponse>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return verifyResponse ?? new VerifyOtpResponse
                    {
                        Success = false,
                        Message = "Không thể xử lý phản hồi từ server"
                    };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<VerifyOtpResponse>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return errorResponse ?? new VerifyOtpResponse
                    {
                        Success = false,
                        Message = $"Lỗi {response.StatusCode}: Đăng ký thất bại"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                return new VerifyOtpResponse
                {
                    Success = false,
                    Message = $"Lỗi kết nối: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new VerifyOtpResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        /// Logout 
        public async Task<LogoutResponse> LogoutAsync()
        {
            try
            {
                var refreshToken = await S0305_SecureStorage.GetRefreshTokenAsync();
                var accessToken = await S0305_SecureStorage.GetTokenAsync();

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var logoutRequest = new LogoutRequest
                    {
                        RefreshToken = refreshToken
                    };

                    var jsonContent = JsonSerializer.Serialize(logoutRequest);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    }

                    try
                    {
                        var response = await _httpClient.PostAsync($"{_baseUrl}/Auth/logout", content);
                        var responseString = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode)
                        {
                            System.Diagnostics.Debug.WriteLine($"Logout API failed: {responseString}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Logout API error: {ex.Message}");
                    }
                }

                await ClearAllAuthDataAsync();

                return new LogoutResponse
                {
                    Success = true,
                    Message = "Đăng xuất thành công"
                };
            }
            catch (Exception ex)
            {
                try
                {
                    await ClearAllAuthDataAsync();
                }
                catch { }

                return new LogoutResponse
                {
                    Success = false,
                    Message = $"Lỗi khi đăng xuất: {ex.Message}"
                };
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        /// xóa tất cả dữ liệu authentication được lưu trữ local
        private async Task ClearAllAuthDataAsync()
        {
            try
            {
                await S0305_SecureStorage.SaveTokenAsync("");
                await S0305_SecureStorage.SaveRefreshTokenAsync("");
                await S0305_SecureStorage.SaveUserIdAsync("");
                await S0305_SecureStorage.SaveDaQuaTrangChuAsync(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing auth data: {ex.Message}");
                throw;
            }
        }
    }
}