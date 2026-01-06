using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using SixOSDatKhamAppMobile.Services;
using SixOSDatKhamAppMobile.Services.S0305;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mTaiKhoan : ContentPage
    {
        private readonly S0305_TaiKhoanService _taiKhoanService;
        private readonly S0305_AuthService _authService;
        private bool _isDataLoaded = false;
        private long _idTk = 0;
        private string _hoTen = "";
        private string _sdt = "";

        // Thêm các control cho loading
        private ActivityIndicator _loadingIndicator;
        private Grid _mainContentGrid;

        public S0306_mTaiKhoan()
        {
            InitializeComponent();
            _taiKhoanService = new S0305_TaiKhoanService();
            _authService = new S0305_AuthService();
            SetupSafeArea();
            // Tìm các control trong XAML
            InitializeLoadingControls();
        }

        private void InitializeLoadingControls()
        {
            // Tìm Grid chính
            _mainContentGrid = this.FindByName<Grid>("mainGrid");

            // Tạo loading indicator nếu không có trong XAML
            if (_mainContentGrid != null)
            {
                _loadingIndicator = new ActivityIndicator
                {
                    IsRunning = false,
                    IsVisible = false,
                    Color = Color.FromArgb("#007bff"),
                    Scale = 1.5,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };
            }
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
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetUseSafeArea(this))
            {
                // Sử dụng safe area
                On<iOS>().SetUseSafeArea(true);
            }
            if (!_isDataLoaded)
            {
                await LoadUserData();
                _isDataLoaded = true;
            }
        }

        private async Task LoadUserData()
        {
            try
            {
                ShowLoading(true);

                // Lấy thông tin tài khoản
                var userInfo = await _taiKhoanService.LayThongTinTaiKhoanHienTai();
                _idTk = userInfo.IdTK;
                _sdt = userInfo.DienThoai ?? "";
                _hoTen = userInfo.HoTen ?? "";


                if (userInfo != null)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        // Cập nhật tên người dùng
                        userNameLabel.Text = userInfo.HoTen ?? "Khách hàng";
                    });
                }
                else
                {
                    await DisplayAlert("Thông báo", "Không thể lấy thông tin tài khoản", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể tải thông tin: {ex.Message}", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void ShowLoading(bool isLoading)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (isLoading)
                {
                    // Tạo loading nếu chưa có
                    if (_loadingIndicator == null)
                    {
                        _loadingIndicator = new ActivityIndicator
                        {
                            Color = Colors.Blue,
                            Scale = 1.5,
                            IsRunning = true,
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center
                        };

                        // Thêm vào layout
                        var mainLayout = this.Content as Grid;
                        if (mainLayout != null)
                        {
                            mainLayout.Children.Add(_loadingIndicator);
                            Grid.SetRowSpan(_loadingIndicator, 2);
                        }
                    }

                    _loadingIndicator.IsVisible = true;
                    _loadingIndicator.IsRunning = true;
                }
                else if (_loadingIndicator != null)
                {
                    _loadingIndicator.IsVisible = false;
                    _loadingIndicator.IsRunning = false;
                }
            });
        }

        private async void OnDoiMatKhauClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mDoiMatKhau(_idTk, _sdt, _hoTen));
        }

        private async void OnHoSoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mHoSoBenhNhan());
        }

        private async void OnQuyDinhSuDungClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Thông báo", "Hiển thị quy định sử dụng", "OK");
            // await Navigation.PushAsync(new S0306_mQuyDinhSuDungPage());
        }

        private async void OnTongDaiCSKHClicked(object sender, EventArgs e)
        {
            var phoneNumber = "19002115";
            if (await DisplayAlert("Gọi tổng đài", $"Bạn có muốn gọi đến {phoneNumber}?", "Gọi", "Hủy"))
            {
                try
                {
                    if (PhoneDialer.Default.IsSupported)
                    {
                        PhoneDialer.Default.Open(phoneNumber);
                    }
                    else
                    {
                        await DisplayAlert("Lỗi", "Thiết bị không hỗ trợ gọi điện", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Lỗi", $"Không thể gọi: {ex.Message}", "OK");
                }
            }
        }

        private async void OnDangXuatClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "Xác nhận đăng xuất",
                "Bạn có chắc chắn muốn đăng xuất?",
                "Đăng xuất",
                "Hủy"
            );

            if (!confirm)
                return;

            try
            {
                ShowLoading(true);
                var result = await _authService.LogoutAsync();

                if (result.Success)
                {
                    Microsoft.Maui.Controls.Application.Current.MainPage = new Microsoft.Maui.Controls.NavigationPage(new S0306_mDangNhap());
                    await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
                        "Thành công",
                        "Đăng xuất thành công",
                        "OK"
                    );
                }
                else
                {
                    S0305_SecureStorage.ClearAllData();
                    Microsoft.Maui.Controls.Application.Current.MainPage = new Microsoft.Maui.Controls.NavigationPage(new S0306_mDangNhap());

                    await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
                        "Thông báo",
                        "Đã đăng xuất khỏi thiết bị này",
                        "OK"
                    );
                }
            }
            catch (Exception ex)
            {
                try
                {
                    S0305_SecureStorage.ClearAllData();
                    Microsoft.Maui.Controls.Application.Current.MainPage = new Microsoft.Maui.Controls.NavigationPage(new S0306_mDangNhap());
                }
                catch { }

                await DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async void OnTrangChuClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mTrangChu());
        }

        private async void OnLichHenClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mLichSuDatHen());
        }

        private async void OnPhieuKhamClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mLichSuKhamBenh());
        }

        private async void OnTaiKhoanClicked(object sender, EventArgs e)
        {
            // Đã ở trang tài khoản rồi, không cần navigate
        }
    }
}