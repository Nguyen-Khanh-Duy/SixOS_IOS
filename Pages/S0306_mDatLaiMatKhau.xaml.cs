using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using SixOSDatKhamAppMobile.Services.S0305;
using System.Text.RegularExpressions;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mDatLaiMatKhau : ContentPage
    {
        private readonly string _cccd;
        private readonly string _phone;
        private readonly string _otp;
        private readonly S0305_ForgotPasswordService _forgotPasswordService;
        private bool _isNewPasswordVisible = false;
        private bool _isConfirmPasswordVisible = false;

        // Constructor nhận CCCD, Phone và OTP
        public S0306_mDatLaiMatKhau(string cccd, string phone, string otp)
        {
            InitializeComponent();
            _cccd = cccd;
            _phone = phone;
            _otp = otp;
            _forgotPasswordService = new S0305_ForgotPasswordService();

            // Hiển thị thông tin người dùng
            InitializeUserInfo();
            SetupSafeArea();
            // Setup events
            NewPasswordEntry.TextChanged += OnPasswordTextChanged;
            ConfirmPasswordEntry.TextChanged += OnConfirmPasswordTextChanged;
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
        // Constructor cũ để tương thích (nếu có)
        public S0306_mDatLaiMatKhau(string phone)
        {
            InitializeComponent();
            _phone = phone;
            _forgotPasswordService = new S0305_ForgotPasswordService();

            // Hiển thị thông tin người dùng
            InitializeUserInfo();

            // Setup events
            NewPasswordEntry.TextChanged += OnPasswordTextChanged;
            ConfirmPasswordEntry.TextChanged += OnConfirmPasswordTextChanged;
        }

        private void InitializeUserInfo()
        {
            if (!string.IsNullOrEmpty(_phone) && _phone.Length >= 10)
            {
                // Hiển thị dạng: 0912***789
                string maskedPhone = _phone.Substring(0, 4) + "***" + _phone.Substring(_phone.Length - 3);
                UserInfoLabel.Text = $"Tạo mật khẩu mới cho số điện thoại: {maskedPhone}";
            }
        }

        private void OnPasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            string password = e.NewTextValue;

            // Kiểm tra độ mạnh mật khẩu
            CheckPasswordStrength(password);

            // Kiểm tra xác nhận mật khẩu
            CheckPasswordConfirmation();

            // Kiểm tra điều kiện đặt lại mật khẩu
            ValidateResetButton();
        }

        private void OnConfirmPasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            // Kiểm tra xác nhận mật khẩu
            CheckPasswordConfirmation();

            // Kiểm tra điều kiện đặt lại mật khẩu
            ValidateResetButton();
        }

        private void CheckPasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                PasswordStrengthContainer.IsVisible = false;
                return;
            }

            PasswordStrengthContainer.IsVisible = true;

            int score = CalculatePasswordScore(password);

            // Cập nhật thanh strength
            UpdateStrengthBars(score);

            // Cập nhật label
            UpdateStrengthLabel(score);
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
        private int CalculatePasswordScore(string password)
        {
            int score = 0;

            // Độ dài - tối thiểu 6 ký tự
            if (password.Length >= 6) score += 1;
            if (password.Length >= 8) score += 1;
            if (password.Length >= 12) score += 1;

            // Chữ hoa/thường (bonus nếu có cả 2)
            if (Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]")) score += 1;

            // Số
            if (Regex.IsMatch(password, "\\d")) score += 1;

            return Math.Min(score, 4); // Tối đa 4 điểm
        }

        private void UpdateStrengthBars(int score)
        {
            // Reset all bars
            StrengthBar1.WidthRequest = 0;
            StrengthBar2.WidthRequest = 0;
            StrengthBar3.WidthRequest = 0;
            StrengthBar4.WidthRequest = 0;

            // Set color based on score
            if (score >= 1)
            {
                StrengthBar1.WidthRequest = 40;
                StrengthBar1.Color = Color.FromArgb("#FF5252"); // Red
            }

            if (score >= 2)
            {
                StrengthBar2.WidthRequest = 40;
                StrengthBar2.Color = Color.FromArgb("#FFB74D"); // Orange
            }

            if (score >= 3)
            {
                StrengthBar3.WidthRequest = 40;
                StrengthBar3.Color = Color.FromArgb("#FFD740"); // Yellow
            }

            if (score >= 4)
            {
                StrengthBar4.WidthRequest = 40;
                StrengthBar4.Color = Color.FromArgb("#4CAF50"); // Green
            }
        }

        private void UpdateStrengthLabel(int score)
        {
            switch (score)
            {
                case 0:
                    StrengthLabel.Text = "Độ mạnh: Rất yếu";
                    StrengthLabel.TextColor = Color.FromArgb("#F44336");
                    break;
                case 1:
                    StrengthLabel.Text = "Độ mạnh: Yếu";
                    StrengthLabel.TextColor = Color.FromArgb("#FF5252");
                    break;
                case 2:
                    StrengthLabel.Text = "Độ mạnh: Trung bình";
                    StrengthLabel.TextColor = Color.FromArgb("#FFB74D");
                    break;
                case 3:
                    StrengthLabel.Text = "Độ mạnh: Khá";
                    StrengthLabel.TextColor = Color.FromArgb("#FFD740");
                    break;
                case 4:
                    StrengthLabel.Text = "Độ mạnh: Mạnh";
                    StrengthLabel.TextColor = Color.FromArgb("#4CAF50");
                    break;
            }
        }

        private void CheckPasswordConfirmation()
        {
            string password = NewPasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ValidationLabel.IsVisible = false;
                return;
            }

            if (password == confirmPassword)
            {
                ValidationLabel.Text = "✓ Mật khẩu khớp";
                ValidationLabel.TextColor = Color.FromArgb("#4CAF50");
                ValidationLabel.IsVisible = true;
            }
            else
            {
                ValidationLabel.Text = "✗ Mật khẩu không khớp";
                ValidationLabel.TextColor = Color.FromArgb("#F44336");
                ValidationLabel.IsVisible = true;
            }
        }

        private void ValidateResetButton()
        {
            string password = NewPasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            // Kiểm tra các điều kiện - tối thiểu 6 ký tự
            bool isPasswordValid = !string.IsNullOrEmpty(password) && password.Length >= 6;
            bool isPasswordMatch = password == confirmPassword;
            bool isPasswordNotEmpty = !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(confirmPassword);

            ResetPasswordButton.IsEnabled = isPasswordValid && isPasswordMatch && isPasswordNotEmpty;
        }

        private void OnToggleNewPasswordClicked(object sender, EventArgs e)
        {
            _isNewPasswordVisible = !_isNewPasswordVisible;
            NewPasswordEntry.IsPassword = !_isNewPasswordVisible;

            // Thay đổi icon
            string icon = _isNewPasswordVisible ? "dong.png" : "mo.png";
            ToggleNewPasswordButton.Source = icon;
        }

        private void OnToggleConfirmPasswordClicked(object sender, EventArgs e)
        {
            _isConfirmPasswordVisible = !_isConfirmPasswordVisible;
            ConfirmPasswordEntry.IsPassword = !_isConfirmPasswordVisible;

            // Thay đổi icon
            string icon = _isConfirmPasswordVisible ? "dong.png" : "mo.png";
            ToggleConfirmPasswordButton.Source = icon;
        }

        private async void OnResetPasswordClicked(object sender, EventArgs e)
        {
            string newPassword = NewPasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            // Validate lần cuối
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                await DisplayAlert("Lỗi", "Mật khẩu phải có ít nhất 6 ký tự", "OK");
                return;
            }

            if (newPassword != confirmPassword)
            {
                await DisplayAlert("Lỗi", "Mật khẩu xác nhận không khớp", "OK");
                return;
            }

            // Hiển thị loading
            ResetPasswordButton.IsEnabled = false;
            ResetPasswordButton.Text = "ĐANG XỬ LÝ...";

            try
            {
                // Gọi API đặt lại mật khẩu
                var result = await _forgotPasswordService.ResetPasswordAsync(
                    _cccd,
                    _phone,
                    _otp,
                    newPassword,
                    confirmPassword);

                if (result.Success)
                {
                    // Hiển thị thông báo thành công
                    ShowSuccessMessage();
                }
                else
                {
                    await DisplayAlert("Lỗi", result.Message, "OK");
                    ResetPasswordButton.IsEnabled = true;
                    ResetPasswordButton.Text = "ĐẶT LẠI MẬT KHẨU";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
                ResetPasswordButton.IsEnabled = false;
                ResetPasswordButton.Text = "ĐẶT LẠI MẬT KHẨU";
            }
        }

        private void ShowSuccessMessage()
        {
            // Ẩn form đặt lại mật khẩu
            ResetPasswordButton.IsVisible = false;

            // Hiển thị thông báo thành công
            SuccessContainer.IsVisible = true;

            // Animation đơn giản
            SuccessContainer.Scale = 0.8;
            SuccessContainer.Opacity = 0;

            SuccessContainer.ScaleTo(1, 300, Easing.SpringOut);
            SuccessContainer.FadeTo(1, 300, Easing.CubicIn);
        }

        private async void OnGoToLoginClicked(object sender, EventArgs e)
        {
            // Quay lại trang đăng nhập (pop all pages)
            await Navigation.PopToRootAsync();
        }

        private async void OnBackToHomeClicked(object sender, EventArgs e)
        {
            // Quay lại trang chủ
            await Navigation.PopToRootAsync();
        }

        // Xử lý hardware back button
        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return true;
        }
    }
}