using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using SixOSDatKhamAppMobile.Services.S0305;
using SixOSDatKhamAppMobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mKhamTheoNgayPage : ContentPage, INotifyPropertyChanged
    {
        #region Properties
        private readonly S0305_DKGoiKhamService _service;
        private readonly S0305_DoiTacService _doiTacService;
        public DoiTacViewModel DoiTacVM { get; }

        public class NgayKhamModel : INotifyPropertyChanged
        {
            public DateTime Ngay { get; set; }
            public string TenNgay { get; set; }
            public string TenNgayVietTat { get; set; }
            public string SoNgay { get; set; }
            public bool LaHomNay { get; set; }
            public bool CoTheChon { get; set; }
            public bool LaCuoiTuan { get; set; }
            public bool LaNgayQuaKhu { get; set; }
            public int SoLuotConLai { get; set; }

            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected != value)
                    {
                        _isSelected = value;
                        OnPropertyChanged();
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<NgayKhamModel> _danhSachNgay = new();
        private NgayKhamModel _selectedNgay;
        private DateTime _thangHienTai = DateTime.Today;
        private List<string> _ngayKhaDungTuAPI = new();
        private bool _isLoading = false;
        private long _selectedGoiKhamId = 0;
        private long _selectedChuyenGiaId = 0;
        private long _idGioiTinh = 0;

        public NgayKhamModel SelectedNgay
        {
            get => _selectedNgay;
            set
            {
                if (_selectedNgay != value)
                {
                    if (_selectedNgay != null)
                        _selectedNgay.IsSelected = false;

                    _selectedNgay = value;

                    if (_selectedNgay != null)
                        _selectedNgay.IsSelected = true;

                    OnPropertyChanged();
                    UpdateTiepTucButton();

                    // Cập nhật UI khi thay đổi ngày chọn
                    if (_selectedNgay != null && _selectedNgay.CoTheChon)
                    {
                        UpdateSelectedDayUI();
                    }
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowLoadingOverlay));
                OnPropertyChanged(nameof(ShowContent));
            }
        }

        public bool ShowLoadingOverlay => IsLoading;
        public bool ShowContent => !IsLoading;

        public event EventHandler<DateTime> NgayKhamSelected;
        #endregion

        #region Constructor
        public S0306_mKhamTheoNgayPage()
        {
            InitializeComponent();
            _service = new S0305_DKGoiKhamService();
            _doiTacService = new S0305_DoiTacService();
            DoiTacVM = new DoiTacViewModel();
            BindingContext = this;

            Dispatcher.DispatchAsync(async () =>
            {
                await Task.Delay(300);
                await LoadDataAsync();
            });
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
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetUseSafeArea(this))
            {
                // Sử dụng safe area
                On<iOS>().SetUseSafeArea(true);
            }
            await DoiTacVM.LoadDataAsync(_doiTacService);
        }

        public S0306_mKhamTheoNgayPage(DateTime? initialDate = null) : this()
        {
            _thangHienTai = initialDate ?? DateTime.Today;
        }
        #endregion

        #region Private Methods
        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                // Lấy thông tin từ SecureStorage
                var goiKhamIdStr = await SecureStorage.GetAsync("SelectedGoiKhamId");
                if (string.IsNullOrEmpty(goiKhamIdStr) || !long.TryParse(goiKhamIdStr, out _selectedGoiKhamId))
                {
                    await DisplayAlert("Lỗi", "Vui lòng chọn gói khám trước", "OK");
                    await Navigation.PopAsync();
                    return;
                }

                var chuyenGiaIdStr = await SecureStorage.GetAsync("SelectedChuyenGiaId");
                _selectedChuyenGiaId = !string.IsNullOrEmpty(chuyenGiaIdStr) && long.TryParse(chuyenGiaIdStr, out var cgId) ? cgId : 0;

                // Lấy thông tin giới tính
                var doTuoiGioiTinhResult = await _service.LayDoTuoiGioiTinhBNAsync();
                if (!doTuoiGioiTinhResult.Success || doTuoiGioiTinhResult.Data == null)
                {
                    await DisplayAlert("Lỗi", doTuoiGioiTinhResult.Message ?? "Không thể lấy thông tin bệnh nhân", "OK");
                    return;
                }

                _idGioiTinh = doTuoiGioiTinhResult.Data.GioiTinh ?? 0;

                // Gọi API lấy danh sách ngày có thể đặt lịch
                var result = await _service.LayNgayCoTheDatLichAsync(_idGioiTinh, _selectedChuyenGiaId, _selectedGoiKhamId);

                if (!result.Success || result.Data == null)
                {
                    await DisplayAlert("Thông báo", result.Message ?? "Không có ngày khả dụng", "OK");
                    _ngayKhaDungTuAPI.Clear();
                }
                else
                {
                    _ngayKhaDungTuAPI = result.Data;
                }

                // Load calendar với dữ liệu từ API
                LoadCalendarData();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể tải dữ liệu: {ex.Message}", "OK");
            }
            finally
            {
                await Task.Delay(300);
                IsLoading = false;
            }
        }

        private void LoadCalendarData()
        {
            _danhSachNgay.Clear();
            NgayGrid.Children.Clear();

            var culture = new CultureInfo("vi-VN");
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(_thangHienTai.Year, _thangHienTai.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(_thangHienTai.Year, _thangHienTai.Month);

            // T2 = 0 ... CN = 6
            int startColumn = (int)firstDayOfMonth.DayOfWeek;
            startColumn = startColumn == 0 ? 6 : startColumn - 1;

            int row = 0;
            int col = startColumn;

            for (int day = 1; day <= daysInMonth; day++)
            {
                var ngay = new DateTime(_thangHienTai.Year, _thangHienTai.Month, day);

                bool isToday = ngay.Date == today;
                bool isWeekend = ngay.DayOfWeek == DayOfWeek.Saturday || ngay.DayOfWeek == DayOfWeek.Sunday;
                bool isPast = ngay.Date < today;

                string ngayStr = ngay.ToString("yyyy-MM-dd");
                bool isAvailableFromAPI = _ngayKhaDungTuAPI.Contains(ngayStr);

                bool canSelect = !isPast && !isWeekend && isAvailableFromAPI;

                var ngayKham = new NgayKhamModel
                {
                    Ngay = ngay,
                    TenNgayVietTat = GetTenNgayVietTat(ngay.DayOfWeek),
                    SoNgay = day.ToString(),
                    LaHomNay = isToday,
                    CoTheChon = canSelect,
                    LaCuoiTuan = isWeekend,
                    LaNgayQuaKhu = isPast,
                    SoLuotConLai = canSelect ? 5 : 0
                };

                _danhSachNgay.Add(ngayKham);

                var frame = CreateNgayFrame(ngayKham);

                // 👇 ADD THEO CỘT & HÀNG
                NgayGrid.Add(frame, col, row);

                col++;
                if (col > 6)
                {
                    col = 0;
                    row++;
                }
            }

            UpdateThangLabel();
        }


        private Frame CreateNgayFrame(NgayKhamModel ngayKham)
        {
            var frame = new Frame
            {
                BackgroundColor = GetBackgroundColor(ngayKham),
                BorderColor = GetBorderColor(ngayKham),
                CornerRadius = 25,
                HeightRequest = 50,
                WidthRequest = 40,
                Padding = 0,
                HasShadow = false,
                Margin = 2,
                BindingContext = ngayKham
            };

            var stackLayout = new VerticalStackLayout
            {
                Spacing = 2,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var lblTenNgay = new Label
            {
                Text = ngayKham.TenNgayVietTat,
                FontSize = 10,
                TextColor = GetTextColor(ngayKham),
                HorizontalOptions = LayoutOptions.Center
            };

            var lblSoNgay = new Label
            {
                Text = ngayKham.SoNgay,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = GetTextColor(ngayKham),
                HorizontalOptions = LayoutOptions.Center
            };

            stackLayout.Children.Add(lblTenNgay);
            stackLayout.Children.Add(lblSoNgay);

            // Hiển thị số lượt còn lại nếu có thể chọn
            if (ngayKham.CoTheChon && ngayKham.SoLuotConLai > 0)
            {
                var lblSoLuot = new Label
                {
                    Text = $"{ngayKham.SoLuotConLai}",
                    FontSize = 8,
                    TextColor = GetSoLuotColor(ngayKham),
                    HorizontalOptions = LayoutOptions.Center
                };
                stackLayout.Children.Add(lblSoLuot);
            }

            // Badge "Hôm nay"
            if (ngayKham.LaHomNay && ngayKham.CoTheChon)
            {
                var badge = new Frame
                {
                    BackgroundColor = Color.FromArgb("#10B981"),
                    Padding = new Thickness(4, 2),
                    CornerRadius = 4,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.End,
                    Margin = new Thickness(0, 0, 0, -5)
                };

                var badgeLabel = new Label
                {
                    Text = "Hôm nay",
                    FontSize = 8,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                };

                badge.Content = badgeLabel;
                stackLayout.Children.Add(badge);
            }

            frame.Content = stackLayout;

            if (ngayKham.CoTheChon)
            {
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => OnNgayTapped(ngayKham);
                frame.GestureRecognizers.Add(tapGesture);
            }
            else if (!ngayKham.LaNgayQuaKhu)
            {
                // Icon khóa cho ngày không thể chọn (trừ ngày quá khứ)
                var lockIcon = new Image
                {
                    Source = "khoa.png",
                    WidthRequest = 12,
                    HeightRequest = 12,
                    HorizontalOptions = LayoutOptions.Center,
                    Opacity = 0.5,
                    Margin = new Thickness(0, 2, 0, 0)
                };
                stackLayout.Children.Add(lockIcon);
            }

            return frame;
        }

        private string GetTenNgayVietTat(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "T2",
                DayOfWeek.Tuesday => "T3",
                DayOfWeek.Wednesday => "T4",
                DayOfWeek.Thursday => "T5",
                DayOfWeek.Friday => "T6",
                DayOfWeek.Saturday => "T7",
                DayOfWeek.Sunday => "CN",
                _ => ""
            };
        }

        private Color GetBackgroundColor(NgayKhamModel ngay)
        {
            if (ngay.LaHomNay)
                return Color.FromArgb("#10B981"); // ✅ hôm nay nền xanh đậm

            if (ngay.IsSelected)
                return Color.FromArgb("#E8F0FE");

            if (!ngay.CoTheChon)
                return Color.FromArgb("#F8FAFC");

            return Colors.White;
        }

        private Color GetBorderColor(NgayKhamModel ngay)
        {
            if (ngay.LaHomNay)
                return Color.FromArgb("#10B981"); // xanh đậm hơn

            if (ngay.IsSelected)
                return Color.FromArgb("#1A73E8");

            return Color.FromArgb("#E2E8F0");
        }


        private Color GetTextColor(NgayKhamModel ngay)
        {
            if (ngay.LaHomNay)
                return Colors.White; // ✅ hôm nay chữ trắng

            if (ngay.IsSelected)
                return Color.FromArgb("#1A73E8");

            if (!ngay.CoTheChon)
                return Color.FromArgb("#94A3B8");

            if (ngay.LaCuoiTuan)
                return Color.FromArgb("#EF4444");

            return Color.FromArgb("#1E293B");
        }


        private Color GetSoLuotColor(NgayKhamModel ngay)
        {
            if (!ngay.CoTheChon)
                return Color.FromArgb("#94A3B8");
            if (ngay.SoLuotConLai > 10)
                return Color.FromArgb("#10B981");
            if (ngay.SoLuotConLai > 5)
                return Color.FromArgb("#F59E0B");
            return Color.FromArgb("#EF4444");
        }

        private void UpdateThangLabel()
        {
            ThangLabel.Text = $"THÁNG {_thangHienTai:MM/yyyy}";
        }

        private void UpdateTiepTucButton()
        {
            BtnTiepTuc.IsVisible = SelectedNgay != null;
        }

        private void UpdateSelectedDayUI()
        {
            foreach (var child in NgayGrid.Children)
            {
                if (child is Frame frame && frame.BindingContext is NgayKhamModel ngayKham)
                {
                    // Cập nhật màu nền và viền
                    frame.BackgroundColor = GetBackgroundColor(ngayKham);
                    frame.BorderColor = GetBorderColor(ngayKham);

                    // Cập nhật màu chữ trong các label
                    if (frame.Content is VerticalStackLayout stackLayout)
                    {
                        foreach (var view in stackLayout.Children)
                        {
                            if (view is Label label)
                            {
                                // Kiểm tra xem có phải là label trong badge không
                                bool isBadgeLabel = false;
                                foreach (var parentView in stackLayout.Children)
                                {
                                    if (parentView is Frame badgeFrame && badgeFrame.Content is Label badgeContent)
                                    {
                                        if (badgeContent == label)
                                        {
                                            isBadgeLabel = true;
                                            break;
                                        }
                                    }
                                }

                                if (!isBadgeLabel)
                                {
                                    label.TextColor = GetTextColor(ngayKham);
                                }
                            }
                        }
                    }
                }
            }
        }

        private async void OnNgayTapped(NgayKhamModel ngay)
        {
            if (!ngay.CoTheChon)
            {
                if (ngay.LaNgayQuaKhu)
                    await DisplayAlert("Thông báo", "Không thể chọn ngày trong quá khứ", "OK");
                else if (ngay.LaCuoiTuan)
                    await DisplayAlert("Thông báo", "Không nhận đặt lịch khám vào cuối tuần", "OK");
                else
                    await DisplayAlert("Thông báo", "Ngày này không khả dụng. Vui lòng chọn ngày khác.", "OK");
                return;
            }

            SelectedNgay = ngay;
        }
        #endregion

        #region Event Handlers
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnPreviousMonthClicked(object sender, EventArgs e)
        {
            _thangHienTai = _thangHienTai.AddMonths(-1);
            SelectedNgay = null;
            UpdateTiepTucButton();

            // Reload lại dữ liệu từ API cho tháng mới
            await LoadDataAsync();
        }

        private async void OnNextMonthClicked(object sender, EventArgs e)
        {
            _thangHienTai = _thangHienTai.AddMonths(1);
            SelectedNgay = null;
            UpdateTiepTucButton();

            // Reload lại dữ liệu từ API cho tháng mới
            await LoadDataAsync();
        }

        private async void OnTiepTucClicked(object sender, EventArgs e)
        {
            if (SelectedNgay == null)
            {
                await DisplayAlert("Thông báo", "Vui lòng chọn ngày khám", "OK");
                return;
            }

            // Lưu ngày đã chọn vào SecureStorage
            await SecureStorage.SetAsync("SelectedNgayKham", SelectedNgay.Ngay.ToString("yyyy-MM-dd"));

            // Chuyển sang trang chọn giờ
            await Navigation.PushAsync(new S0306_mKhamTheoGioPage(SelectedNgay.Ngay));
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            _thangHienTai = DateTime.Today;
            SelectedNgay = null;
            UpdateTiepTucButton();
            await LoadDataAsync();
            await DisplayAlert("Thông báo", "Đã làm mới lịch", "OK");
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}