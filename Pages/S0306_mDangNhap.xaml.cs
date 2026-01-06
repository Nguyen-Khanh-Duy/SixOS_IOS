using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using SixOSDatKhamAppMobile.Services;
using SixOSDatKhamAppMobile.Services.S0305;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mDangNhap : ContentPage
    {
        private bool isPasswordVisible = false;
        private readonly S0305_AuthService _authService;
        private readonly S0305_PatientInfoService _patientInfoService;
        private bool isLoading = false;

        public S0306_mDangNhap()
        {
            InitializeComponent();
            _patientInfoService = new S0305_PatientInfoService();
            _authService = new S0305_AuthService();
            SetupSafeArea();
        }
        private void SetupSafeArea()
        {
            // Xử lý cho Android
#if ANDROID
            var mainDisplayInfo = DeviceDisplay.Current.MainDisplayInfo;
            var density = mainDisplayInfo.Density;

            // Tính toán status bar height
            var statusBarHeight = density > 0 ? density * 15 : 15;

            // Tính toán navigation bar height (khoảng 48-56dp)
            var bottomPadding = density > 0 ? density * 15 : 15;

            // Set padding cho cả trên và dưới
            this.BackgroundColor = Colors.Black;
            this.Padding = new Thickness(0, statusBarHeight, 0, bottomPadding);

            // Đảm bảo content bắt đầu sau status bar
            if (Content is Layout layout)
            {
                layout.BackgroundColor = Colors.White;
            }
#endif

            // Xử lý cho iOS
#if IOS
    // Sử dụng SafeAreaInsets để lấy chính xác hơn
    var topPadding = 40; // hoặc tính toán dựa trên SafeAreaInsets
    var bottomPadding = 34; // cho iPhone có Home indicator
    
    this.BackgroundColor = Colors.White;
    this.Padding = new Thickness(0, topPadding, 0, bottomPadding);
#endif
        }
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (isLoading) return;

            string cccd = SoCCCDEntry.Text?.Trim();
            string password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(cccd))
            {
                await DisplayAlert("Lỗi", "Vui lòng nhập số căn cước công dân", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Lỗi", "Vui lòng nhập mật khẩu", "OK");
                return;
            }

            isLoading = true;

            try
            {
                // 1️⃣ Đăng nhập (BẮT BUỘC chờ)
                var result = await _authService.LoginAsync(cccd, password);

                if (!result.Success)
                {
                    await DisplayAlert("Lỗi", result.Message, "OK");
                    return;
                }

                // 2️⃣ LƯU SECURE STORAGE SONG SONG (KHÔNG BLOCK UI)
                if (!string.IsNullOrEmpty(result.AccessToken))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.WhenAll(
                                S0305_SecureStorage.SaveTokenAsync(result.AccessToken),
                                S0305_SecureStorage.SaveRefreshTokenAsync(result.RefreshToken),
                                S0305_SecureStorage.SaveUserIdAsync(result.UserInfo.Id.ToString()),
                                S0305_SecureStorage.SaveUserCCCDAsync(cccd),
                                S0305_SecureStorage.SaveUserPhoneAsync(result.UserInfo.DienThoai)
                            );
                        }
                        catch
                        {
                            // tránh crash UI
                        }
                    });
                }

                // 3️⃣ Check thông tin cá nhân (vẫn giữ logic cũ)
                bool hasPatientInfo =
                    await _patientInfoService.CheckPatientInfoExistsAsync(result.UserInfo.Id);

                // 4️⃣ Điều hướng (KHÔNG animation để mượt)
                if (hasPatientInfo)
                {
                    await Navigation.PushAsync(new S0306_mChonCoSoKham(), false);
                }
                else
                {
                    await Navigation.PushAsync(
                        new S0306_mNhapThongTinCaNhan(
                            result.UserInfo.Id,
                            cccd,
                            result.UserInfo.DienThoai),
                        false
                    );
                }
            }
            finally
            {
                isLoading = false;
            }
        }

        private void OnTogglePasswordVisibility(object sender, EventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;
            PasswordEntry.IsPassword = !isPasswordVisible;
            TogglePasswordButton.Source = isPasswordVisible ? "dong.png" : "mo.png";
        }

        private async void OnForgotPasswordClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mQuenMatKhau(), false);
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mTaoTaiKhoan(), false);
        }

        private async void OnCallSupportClicked(object sender, EventArgs e)
        {
            bool call = await DisplayAlert(
                "Gọi hỗ trợ",
                "Bạn có muốn gọi số 028 3841 2637?",
                "Gọi", "Huỷ");

            if (!call) return;

            try
            {
                if (PhoneDialer.Default.IsSupported)
                {
                    PhoneDialer.Default.Open("02838412637");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể thực hiện cuộc gọi: {ex.Message}", "OK");
            }
        }

        private async void OnEmailSupportClicked(object sender, EventArgs e)
        {
            bool sendEmail = await DisplayAlert(
                "Email hỗ trợ",
                "Gửi email đến hotro@benhvienungbuou.vn?",
                "Mở Email", "Huỷ");

            if (!sendEmail) return;

            try
            {
                if (Email.Default.IsComposeSupported)
                {
                    var message = new EmailMessage
                    {
                        Subject = "Yêu cầu hỗ trợ - Ứng dụng đặt lịch khám",
                        Body = "Xin chào, tôi cần hỗ trợ về...",
                        To = new List<string> { "hotro@benhvienungbuou.vn" }
                    };

                    await Email.Default.ComposeAsync(message);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể mở ứng dụng email: {ex.Message}", "OK");
            }
        }

        protected override bool OnBackButtonPressed()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopAsync(false);
            });
            return true;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetUseSafeArea(this))
            {
                // Sử dụng safe area
                On<iOS>().SetUseSafeArea(true);
            }
        }
    }
}
