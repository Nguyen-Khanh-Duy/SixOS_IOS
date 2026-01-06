using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using SixOSDatKhamAppMobile.Services.S0305;
using System.Text.RegularExpressions;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mDoiMatKhau : ContentPage
    {
        private readonly S0305_ForgotPasswordService _changePasswordService;
        private bool _isCurrentPasswordVisible = false;
        private bool _isNewPasswordVisible = false;
        private bool _isConfirmPasswordVisible = false;
        private long _idTk = 0;
        private string _hoTen = "";
        private string _sdt = "";

        public S0306_mDoiMatKhau()
        {
            InitializeComponent();
            _changePasswordService = new S0305_ForgotPasswordService();

            // Setup events
            CurrentPasswordEntry.TextChanged += OnPasswordTextChanged;
            NewPasswordEntry.TextChanged += OnPasswordTextChanged;
            ConfirmPasswordEntry.TextChanged += OnConfirmPasswordTextChanged;
            SetupSafeArea();
            // Load thông tin người dùng
            LoadUserInfo();
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
        layout.BackgroundColor = Colors.Black;
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
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetUseSafeArea(this))
            {
                // Sử dụng safe area
                On<iOS>().SetUseSafeArea(true);
            }
        }
        public S0306_mDoiMatKhau(long idTK, string phone, string hoTen)
        {
            InitializeComponent();
            _changePasswordService = new S0305_ForgotPasswordService();
            _idTk = idTK;
            _sdt = phone;
            _hoTen = hoTen;

            // Setup events
            CurrentPasswordEntry.TextChanged += OnPasswordTextChanged;
            NewPasswordEntry.TextChanged += OnPasswordTextChanged;
            ConfirmPasswordEntry.TextChanged += OnConfirmPasswordTextChanged;

            // Load thông tin người dùng
            LoadUserInfo();
        }

        private async void LoadUserInfo()
        {
            try
            {
                // Lấy thông tin người dùng từ SecureStorage hoặc API
                //string userName = await SecureStorage.Default.GetAsync("UserName") ?? "Người dùng";
                //string phone = await SecureStorage.Default.GetAsync("UserPhone") ?? "";
                string userName = _hoTen ?? "Người dùng";
                string phone = _sdt ?? "";

                // Hiển thị thông tin
                UserNameLabel.Text = userName;

                if (!string.IsNullOrEmpty(phone) && phone.Length >= 10)
                {
                    string maskedPhone = phone.Substring(0, 4) + "***" + phone.Substring(phone.Length - 3);
                    PhoneLabel.Text = $"Số điện thoại: {maskedPhone}";
                }
                else
                {
                    PhoneLabel.Text = "Số điện thoại: Chưa cập nhật";
                }
            }
            catch (Exception)
            {
                // Nếu không lấy được thông tin, dùng mặc định
                UserNameLabel.Text = "Người dùng";
                PhoneLabel.Text = "Số điện thoại: Chưa cập nhật";
            }
        }

        private void OnPasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            // Kiểm tra độ mạnh mật khẩu nếu là new password
            if (sender == NewPasswordEntry)
            {
                string password = e.NewTextValue;
                CheckPasswordStrength(password);
            }

            // Kiểm tra xác nhận mật khẩu
            CheckPasswordConfirmation();

            // Kiểm tra điều kiện đổi mật khẩu
            ValidateChangeButton();
        }

        private void OnConfirmPasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            // Kiểm tra xác nhận mật khẩu
            CheckPasswordConfirmation();

            // Kiểm tra điều kiện đổi mật khẩu
            ValidateChangeButton();
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
            string newPassword = NewPasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ValidationLabel.IsVisible = false;
                return;
            }

            if (newPassword == confirmPassword)
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

        private void ValidateChangeButton()
        {
            string currentPassword = CurrentPasswordEntry.Text;
            string newPassword = NewPasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            // Kiểm tra các điều kiện
            bool isCurrentPasswordValid = !string.IsNullOrEmpty(currentPassword);
            bool isNewPasswordValid = !string.IsNullOrEmpty(newPassword) && newPassword.Length >= 6;
            bool isPasswordMatch = newPassword == confirmPassword;
            bool allFieldsFilled = !string.IsNullOrEmpty(currentPassword) &&
                                  !string.IsNullOrEmpty(newPassword) &&
                                  !string.IsNullOrEmpty(confirmPassword);

            ChangePasswordButton.IsEnabled = isCurrentPasswordValid &&
                                           isNewPasswordValid &&
                                           isPasswordMatch &&
                                           allFieldsFilled;
        }

        private void OnToggleCurrentPasswordClicked(object sender, EventArgs e)
        {
            _isCurrentPasswordVisible = !_isCurrentPasswordVisible;
            CurrentPasswordEntry.IsPassword = !_isCurrentPasswordVisible;

            // Thay đổi icon
            string icon = _isCurrentPasswordVisible ? "dong.png" : "mo.png";
            ToggleCurrentPasswordButton.Source = icon;
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

        private async void OnChangePasswordClicked(object sender, EventArgs e)
        {
            string currentPassword = CurrentPasswordEntry.Text;
            string newPassword = NewPasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            // Validate lần cuối
            if (string.IsNullOrEmpty(currentPassword))
            {
                await DisplayAlert("Lỗi", "Vui lòng nhập mật khẩu hiện tại", "OK");
                return;
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                await DisplayAlert("Lỗi", "Mật khẩu mới phải có ít nhất 6 ký tự", "OK");
                return;
            }

            if (newPassword != confirmPassword)
            {
                await DisplayAlert("Lỗi", "Mật khẩu xác nhận không khớp", "OK");
                return;
            }

            if (currentPassword == newPassword)
            {
                await DisplayAlert("Lỗi", "Mật khẩu mới không được trùng với mật khẩu hiện tại", "OK");
                return;
            }

            // Hiển thị loading
            ChangePasswordButton.IsEnabled = false;
            ChangePasswordButton.Text = "ĐANG XỬ LÝ...";

            try
            {
                // Gọi API đổi mật khẩu
                var result = await _changePasswordService.ResetPasswordWithoutOtpAsync(
                    currentPassword,
                    newPassword,
                    confirmPassword);

                if (result.Success)
                {
                    await DisplayAlert("Thành công", result.Message ?? "Đổi mật khẩu thành công", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Lỗi", result.Message ?? "Đổi mật khẩu thất bại", "OK");
                    ResetForm();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
                ResetForm();
            }
        }

        private void ResetForm()
        {
            ChangePasswordButton.IsEnabled = true;
            ChangePasswordButton.Text = "ĐỔI MẬT KHẨU";
        }

        private void ShowSuccessMessage()
        {
            // Ẩn form đổi mật khẩu
            ChangePasswordForm.IsVisible = false;

            // Hiển thị thông báo thành công
            SuccessContainer.IsVisible = true;

            // Animation
            SuccessContainer.Scale = 0.8;
            SuccessContainer.Opacity = 0;

            SuccessContainer.ScaleTo(1, 300, Easing.SpringOut);
            SuccessContainer.FadeTo(1, 300, Easing.CubicIn);
        }

        private async void OnGoBackClicked(object sender, EventArgs e)
        {
            // Quay lại trang trước đó (tài khoản)
            await Navigation.PopAsync();
        }

        private async void OnGoToLoginClicked(object sender, EventArgs e)
        {
            // Đăng xuất và quay lại trang đăng nhập
            SecureStorage.Default.RemoveAll();
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