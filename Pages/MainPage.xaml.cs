using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using SixOSDatKhamAppMobile.Pages;
using SixOSDatKhamAppMobile.Services;
using SixOSDatKhamAppMobile.Services.S0305;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;

namespace SixOSDatKhamAppMobile
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private readonly S0305_TrangChuService _trangChuService;
        private ObservableCollection<ChuyenGiaTrangChuDTO> _danhSachChuyenGia;
        private ObservableCollection<GoiKhamTrangChuDTO> _danhSachGoiKham;
        private System.Timers.Timer _carouselTimer;
        private System.Timers.Timer _quangCaoTimer;

        private int _currentCarouselIndex = 0;
        private int _currentQuangCaoIndex = 0;
        private bool _hasMoved;
        private const int AUTO_SCROLL_INTERVAL_MS = 3000;
        private const int QUANG_CAO_INTERVAL_MS = 3000;

        // Thêm các biến mới vào class MainPage
        private double _startX, _startY;
        private bool _isDragging = false;
        private const double DRAG_THRESHOLD = 50; // Ngưỡng để phân biệt tap và drag
        public ObservableCollection<ChuyenGiaTrangChuDTO> DanhSachChuyenGia
        {
            get => _danhSachChuyenGia;
            set
            {
                _danhSachChuyenGia = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<GoiKhamTrangChuDTO> DanhSachGoiKham
        {
            get => _danhSachGoiKham;
            set
            {
                _danhSachGoiKham = value;
                OnPropertyChanged();
            }
        }
        private async void OnHowToBookAppointmentTapped(object sender, EventArgs e)
        {
            // Mở trang video hướng dẫn
            await Navigation.PushAsync(new S0306_mVideoPlayerPage());
        }
        // Thêm phương thức này vào MainPage.xaml.cs
        private async void OnHowToCheckAppointmentTapped(object sender, EventArgs e)
        {
            // Mở trang video tra cứu lịch hẹn
            await Navigation.PushAsync(new S0306_mVideoTraCuu());
        }
        public MainPage()
        {
            InitializeComponent();
            _trangChuService = new S0305_TrangChuService();
            DanhSachChuyenGia = new ObservableCollection<ChuyenGiaTrangChuDTO>();
            DanhSachGoiKham = new ObservableCollection<GoiKhamTrangChuDTO>();
            this.BindingContext = this;
            // Khởi tạo timer
            InitializeCarouselTimer();
            InitializeQuangCaoTimer();
            SetupSafeArea();
            // Liên kết Indicator với Carousel
            AutoCarousel.IndicatorView = CarouselIndicator;
            QuangCaoCarousel.IndicatorView = QuangCaoIndicator;
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
            // Đảm bảo status bar có màu trắng
            if (Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetUseSafeArea(this))
            {
                // Sử dụng safe area
                On<iOS>().SetUseSafeArea(true);
            }
            await S0305_SecureStorage.SaveDaQuaTrangChuAsync(false);
            await LoadDuLieuMainPageAsync();
            StartCarouselTimer();
            StartQuangCaoTimer();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Dừng timer khi trang biến mất
            StopCarouselTimer();
            StopQuangCaoTimer();
        }
        private void InitializeCarouselTimer()
        {
            _carouselTimer = new System.Timers.Timer(AUTO_SCROLL_INTERVAL_MS);
            _carouselTimer.Elapsed += OnCarouselTimerElapsed;
            _carouselTimer.AutoReset = true;
        }

        private void StartCarouselTimer()
        {
            if (AutoCarousel.ItemsSource != null && _carouselTimer != null)
            {
                _carouselTimer.Start();
            }
        }

        private void StopCarouselTimer()
        {
            if (_carouselTimer != null)
            {
                _carouselTimer.Stop();
            }
        }

        private async void OnCarouselTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (AutoCarousel.ItemsSource != null)
                {
                    var itemCount = ((System.Collections.IList)AutoCarousel.ItemsSource).Count;
                    if (itemCount > 0)
                    {
                        _currentCarouselIndex = (_currentCarouselIndex + 1) % itemCount;
                        AutoCarousel.Position = _currentCarouselIndex;
                    }
                }
            });
        }

        private void AutoCarousel_PositionChanged(object sender, PositionChangedEventArgs e)
        {
            _currentCarouselIndex = e.CurrentPosition;
        }
        #region Quảng Cáo Carousel Timer
        private void InitializeQuangCaoTimer()
        {
            _quangCaoTimer = new System.Timers.Timer(QUANG_CAO_INTERVAL_MS);
            _quangCaoTimer.Elapsed += OnQuangCaoTimerElapsed;
            _quangCaoTimer.AutoReset = true;
        }

        private void StartQuangCaoTimer()
        {
            if (QuangCaoCarousel.ItemsSource != null && _quangCaoTimer != null)
            {
                _quangCaoTimer.Start();
            }
        }

        private void StopQuangCaoTimer()
        {
            if (_quangCaoTimer != null)
            {
                _quangCaoTimer.Stop();
            }
        }

        private async void OnQuangCaoTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (QuangCaoCarousel.ItemsSource != null)
                {
                    var itemCount = ((System.Collections.IList)QuangCaoCarousel.ItemsSource).Count;
                    if (itemCount > 0)
                    {
                        _currentQuangCaoIndex = (_currentQuangCaoIndex + 1) % itemCount;
                        QuangCaoCarousel.Position = _currentQuangCaoIndex;
                    }
                }
            });
        }

        private void QuangCaoCarousel_PositionChanged(object sender, PositionChangedEventArgs e)
        {
            _currentQuangCaoIndex = e.CurrentPosition;
        }
        #endregion

        private async Task LoadDuLieuMainPageAsync()
        {
            try
            {
                // Load chuyên gia
                var resultCG = await _trangChuService.LayNgauNhienChuyenGiaAsync(3);
                if (resultCG.Success && resultCG.Data != null)
                {
                    DanhSachChuyenGia = new ObservableCollection<ChuyenGiaTrangChuDTO>(resultCG.Data);
                }

                // Load gói khám
                var resultGK = await _trangChuService.LayNgauNhienGoiKhamAsync(3);
                if (resultGK.Success && resultGK.Data != null)
                {
                    DanhSachGoiKham = new ObservableCollection<GoiKhamTrangChuDTO>(resultGK.Data);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể tải dữ liệu: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void OnDangNhapClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mDangNhap());
        }

        private async void OnViewAllNewsClicked(object sender, System.EventArgs e)
        {
            //await Navigation.PushAsync(new S0306_mDangNhap());
            await DisplayAlert("Tin tức", "Tạm thời chưa có tin tức nào.", "OK");
        }

        private async void OnConsultationClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mDangNhap());
        }

        private async void OnProfileClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mDangNhap());
        }

        private async void OnAppointmentClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mDangNhap());
        }

        private async void OnMedicalFormClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mDangNhap());
        }

        private async void OnAccountClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mDangNhap());
        }

        private void OnBookAppointmentClicked(object sender, EventArgs e)
        {
            DisplayAlert("Thông báo", "Bạn đã nhấn Đặt lịch khám", "OK");
        }

        private async void OnBookDoctorClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mDangNhap());
        }

        private async void OnViewAllDoctorsClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mDangNhap());
        }

        private async void OnViewAllServicesClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mDangNhap());
        }

        private async void OnBookServiceClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mDangNhap());
        }

        private async void OnBookService1Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mDangNhap());
        }

        private async void OnBookService2Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mDangNhap());
        }

        private async void OnBookService3Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mDangNhap());
        }

        private async void OpenCS1Map(object sender, EventArgs e)
        {
            var url = "https://www.google.com/maps?q=47 Nguyễn Huy Lượng, Phường Bình Thạnh, TP.HCM";
            await Launcher.OpenAsync(url);
        }

        private async void OpenCS2Map(object sender, EventArgs e)
        {
            var url = "https://www.google.com/maps?q=Số 12 Đường 400, Phường Tăng Nhơn Phú,+TPHCM";
            await Launcher.OpenAsync(url);
        }

        // Phương thức xử lý kéo thả
        private void OnPanGestureUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    // Lưu vị trí ban đầu
                    _startX = FloatingCallButton.TranslationX;
                    _startY = FloatingCallButton.TranslationY;
                    _isDragging = true;
                    _hasMoved = false;
                    break;

                case GestureStatus.Running:
                    // Di chuyển button theo cử chỉ
                    FloatingCallButton.TranslationX = _startX + e.TotalX;
                    FloatingCallButton.TranslationY = _startY + e.TotalY;

                    // Kiểm tra nếu di chuyển đủ xa để coi là drag
                    if (Math.Abs(e.TotalX) > DRAG_THRESHOLD || Math.Abs(e.TotalY) > DRAG_THRESHOLD)
                    {
                        _hasMoved = true;
                    }
                    break;

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    _isDragging = false;

                    // Nếu đã kéo thả, không thực hiện sự kiện tap
                    if (_hasMoved)
                    {
                        // Tự động "dính" vào cạnh gần nhất
                        SnapToNearestEdge();
                        _hasMoved = false;
                    }
                    break;
            }
        }

        // Phương thức tự động dính vào cạnh gần nhất
        private void SnapToNearestEdge()
        {
            var screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            var screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;

            var buttonX = FloatingCallButton.X + FloatingCallButton.TranslationX;
            var buttonY = FloatingCallButton.Y + FloatingCallButton.TranslationY;
            var buttonWidth = FloatingCallButton.Width;
            var buttonHeight = FloatingCallButton.Height;

            // Tính khoảng cách đến các cạnh
            var distanceToLeft = buttonX;
            var distanceToRight = screenWidth - (buttonX + buttonWidth);
            var distanceToTop = buttonY;
            var distanceToBottom = screenHeight - (buttonY + buttonHeight);

            // Tìm cạnh gần nhất
            var minHorizontal = Math.Min(distanceToLeft, distanceToRight);
            var minVertical = Math.Min(distanceToTop, distanceToBottom);

            if (minHorizontal < minVertical)
            {
                // Dính vào cạnh trái hoặc phải
                if (distanceToLeft < distanceToRight)
                {
                    // Dính vào cạnh trái
                    FloatingCallButton.TranslateTo(0, FloatingCallButton.TranslationY, 250, Easing.SpringOut);
                }
                else
                {
                    // Dính vào cạnh phải
                    var targetX = screenWidth - buttonWidth - FloatingCallButton.X;
                    FloatingCallButton.TranslateTo(targetX, FloatingCallButton.TranslationY, 250, Easing.SpringOut);
                }
            }
            else
            {
                // Dính vào cạnh trên hoặc dưới
                if (distanceToTop < distanceToBottom)
                {
                    // Dính vào cạnh trên
                    FloatingCallButton.TranslateTo(FloatingCallButton.TranslationX, 0, 250, Easing.SpringOut);
                }
                else
                {
                    // Dính vào cạnh dưới
                    var targetY = screenHeight - buttonHeight - FloatingCallButton.Y;
                    FloatingCallButton.TranslateTo(FloatingCallButton.TranslationX, targetY, 250, Easing.SpringOut);
                }
            }
        }

        // Sửa phương thức OnCallHospitalClicked để không block khi đang kéo
        private async void OnCallHospitalClicked(object sender, EventArgs e)
        {
            // Nếu đang kéo thì không gọi
            if (_isDragging || _hasMoved)
                return;

            // Hiệu ứng nhấn
            await FloatingCallButton.ScaleTo(0.9, 50);
            await FloatingCallButton.ScaleTo(1.0, 50);

            // GỌI ĐIỆN
            try
            {
                PhoneDialer.Open("19001234");
            }
            catch
            {
                await DisplayAlert("Lỗi", "Thiết bị không hỗ trợ gọi điện", "OK");
            }
        }

    }
}