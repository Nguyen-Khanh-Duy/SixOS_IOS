using SixOSDatKhamAppMobile.Configurations;
using System.Net.Http.Json;
using System.Text.Json;

namespace SixOSDatKhamAppMobile.Services.S0305
{
    public class S0305_ForgotPasswordService
    {
        private readonly string _baseUrl = S0305_ApiConfig.BaseUrl;
        private readonly HttpClient _httpClient;

        public S0305_ForgotPasswordService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(S0305_ApiConfig.DefaultTimeoutSeconds);
        }

        // Gửi OTP quên mật khẩu
        public async Task<ForgotPasswordResponse> SendForgotPasswordOtpAsync(string cccd, string phone)
        {
            try
            {
                var request = new ForgotPasswordRequest
                {
                    Cccd = cccd,
                    DienThoai = phone
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/Auth/forgot-password/send-otp",
                    request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
                    return result ?? new ForgotPasswordResponse
                    {
                        Success = false,
                        Message = "Không nhận được phản hồi từ server"
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ForgotPasswordResponse>(
                            errorContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        return errorResponse ?? new ForgotPasswordResponse
                        {
                            Success = false,
                            Message = "Có lỗi xảy ra khi gửi OTP"
                        };
                    }
                    catch
                    {
                        return new ForgotPasswordResponse
                        {
                            Success = false,
                            Message = $"Lỗi: {response.StatusCode}"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new ForgotPasswordResponse
                {
                    Success = false,
                    Message = $"Lỗi kết nối: {ex.Message}"
                };
            }
        }

        public async Task<ForgotPasswordResponse> VerifyForgotPasswordOtpAsync(string cccd, string phone, string otp)
        {
            try
            {
                var request = new VerifyForgotPasswordOtpRequest
                {
                    Cccd = cccd,
                    DienThoai = phone,
                    Otp = otp
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/Auth/forgot-password/verify-otp",
                    request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
                    return result ?? new ForgotPasswordResponse
                    {
                        Success = false,
                        Message = "Không nhận được phản hồi từ server"
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ForgotPasswordResponse>(
                            errorContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        return errorResponse ?? new ForgotPasswordResponse
                        {
                            Success = false,
                            Message = "Mã OTP không chính xác"
                        };
                    }
                    catch
                    {
                        return new ForgotPasswordResponse
                        {
                            Success = false,
                            Message = "Mã OTP không chính xác"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new ForgotPasswordResponse
                {
                    Success = false,
                    Message = $"Lỗi kết nối: {ex.Message}"
                };
            }
        }

        // Đặt lại mật khẩu
        public async Task<ForgotPasswordResponse> ResetPasswordAsync(
            string cccd,
            string phone,
            string otp,
            string newPassword,
            string confirmPassword)
        {
            try
            {
                var request = new ResetPasswordRequest
                {
                    Cccd = cccd,
                    DienThoai = phone,
                    Otp = otp,
                    MatKhauMoi = newPassword,
                    XacNhanMatKhau = confirmPassword
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/Auth/forgot-password/reset",
                    request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
                    return result ?? new ForgotPasswordResponse
                    {
                        Success = false,
                        Message = "Không nhận được phản hồi từ server"
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ForgotPasswordResponse>(
                            errorContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        return errorResponse ?? new ForgotPasswordResponse
                        {
                            Success = false,
                            Message = "Có lỗi xảy ra khi đặt lại mật khẩu"
                        };
                    }
                    catch
                    {
                        return new ForgotPasswordResponse
                        {
                            Success = false,
                            Message = $"Lỗi: {response.StatusCode}"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new ForgotPasswordResponse
                {
                    Success = false,
                    Message = $"Lỗi kết nối: {ex.Message}"
                };
            }
        }

        public async Task<ForgotPasswordResponse> ResetPasswordWithoutOtpAsync(string matKhauCu, string matKhauMoi, string xacNhanMatKhau)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return new ForgotPasswordResponse
                    {
                        Success = false,
                        Message = "Người dùng chưa đăng nhập"
                    };
                }
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var request = new 
                {
                    MatKhauCu = matKhauCu,
                    MatKhauMoi = matKhauMoi,
                    XacNhanMatKhau = xacNhanMatKhau
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/Auth/forgot-password/reset/no-otp",
                    request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
                    return result ?? new ForgotPasswordResponse
                    {
                        Success = false,
                        Message = "Không nhận được phản hồi từ server"
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ForgotPasswordResponse>(
                            errorContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        return errorResponse ?? new ForgotPasswordResponse
                        {
                            Success = false,
                            Message = "Có lỗi xảy ra khi đặt lại mật khẩu"
                        };
                    }
                    catch
                    {
                        return new ForgotPasswordResponse
                        {
                            Success = false,
                            Message = $"Lỗi: {response.StatusCode}"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new ForgotPasswordResponse
                {
                    Success = false,
                    Message = $"Lỗi kết nối: {ex.Message}"
                };
            }
        }

    }

    public class ForgotPasswordRequest
    {
        public string Cccd { get; set; }
        public string DienThoai { get; set; }
    }

    public class VerifyForgotPasswordOtpRequest
    {
        public string Cccd { get; set; }
        public string DienThoai { get; set; }
        public string Otp { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Cccd { get; set; }
        public string DienThoai { get; set; }
        public string Otp { get; set; }
        public string MatKhauMoi { get; set; }
        public string XacNhanMatKhau { get; set; }
    }

    public class ResetPasswordWithoutOtpDTO
    {
        public string Cccd { get; set; }
        public string MatKhauCu { get; set; }
        public string MatKhauMoi { get; set; }
        public string XacNhanMatKhau { get; set; }
    }

    public class ForgotPasswordResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? PhoneHint { get; set; }  

    }

}