using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using SixOSDatKhamAppMobile.Services;
using SixOSDatKhamAppMobile.Services.S0305;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mHoSoBenhNhan : ContentPage
    {
        private ObservableCollection<HoSoBenhNhan> _danhSachHoSo;
        private ObservableCollection<HoSoBenhNhan> _danhSachHienThi;
        private readonly S0305_HoSoService _hoSoService;
        private bool _isLoading;

        private string hoSoDuocChonId = "";

        #region Converter
        public class BoolToButtonColorConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
                => value is bool b && b ? Color.FromArgb("#1A73E8") : Colors.White;

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                => throw new NotImplementedException();
        }

        public class BoolToButtonTextColorConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
                => value is bool b && b ? Colors.White : Color.FromArgb("#1A73E8");

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                => throw new NotImplementedException();
        }
        public class BoolToBorderColorConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
                => value is bool b && b ? Color.FromArgb("#1A73E8") : Color.FromArgb("#E2E8F0");

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                => throw new NotImplementedException();
        }

        public class BoolToBorderThicknessConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
                => value is bool b && b ? 2 : 1;

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                => throw new NotImplementedException();
        }
        #endregion

        public S0306_mHoSoBenhNhan()
        {
            try
            {
                InitializeComponent();

                Resources.Add("BoolToColorConverter", new BoolToButtonColorConverter());
                Resources.Add("BoolToTextColorConverter", new BoolToButtonTextColorConverter());
                Resources.Add("BorderColorConverter", new BoolToBorderColorConverter());
                Resources.Add("BorderThicknessConverter", new BoolToBorderThicknessConverter());

                _hoSoService = new S0305_HoSoService();
                _danhSachHoSo = new ObservableCollection<HoSoBenhNhan>();
                _danhSachHienThi = new ObservableCollection<HoSoBenhNhan>();

                CollectionViewHoSo.ItemsSource = _danhSachHienThi;

                _ = InitializeAsync();
            }
            catch (Exception ex)
            {
                DisplayAlert("Lỗi", ex.Message, "OK");
            }
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
        }

        private async Task InitializeAsync()
        {
            var daQuaTrangChu = await S0305_SecureStorage.GetDaQuaTrangChuAsync();

            if (string.IsNullOrEmpty(daQuaTrangChu) || !bool.Parse(daQuaTrangChu))
            {
                menuBottomInfo.IsVisible = false;
            }
            await TaiDanhSachHoSoAsync();
        }

        public S0306_mHoSoBenhNhan(S0306_mNhapThongTinCaNhan parentPage) : this()
        {

        }

        #region Model
        public class HoSoBenhNhan
        {
            public string Id { get; set; }
            public string HoTen { get; set; }
            public DateTime NgaySinh { get; set; }
            public string SoDienThoai { get; set; }
            public string CCCD { get; set; }
            public string Email { get; set; }
            public string GioiTinh { get; set; }
            public string DiaChi { get; set; }
            public bool IsSelected { get; set; }
            public Color BackgroundColor { get; set; }

            public Command XemThongTinCommand { get; set; }
            public Command TapToSelectCommand { get; set; }
            public Command ChonHoSoCommand { get; set; }
        }
        #endregion

        #region Load danh sách
        private async Task TaiDanhSachHoSoAsync(string? keyTimKiem = null)
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                var result = await _hoSoService.LayDanhSachHoSoAsync(keyTimKiem, false);

                if (result.Success && result.Data != null)
                {
                    _danhSachHoSo.Clear();
                    _danhSachHienThi.Clear();

                    foreach (var item in result.Data)
                    {
                        var id = item.IdBenhNhan.ToString();

                        var hoSo = new HoSoBenhNhan
                        {
                            Id = id,
                            HoTen = item.TenBenhNhan ?? "",
                            NgaySinh = item.NgaySinh ?? DateTime.Now,
                            SoDienThoai = item.DienThoai ?? "",
                            CCCD = item.SoCccd ?? "",
                            Email = item.Email ?? "",
                            GioiTinh = item.GioiTinh ?? "",
                            DiaChi = item.DiaChi ?? "",
                            IsSelected = false,
                            BackgroundColor = Colors.White,

                            XemThongTinCommand = new Command(() => _ = XemThongTinHoSo(id)),
                            TapToSelectCommand = new Command(() => _ = TapToSelectHoSo(id)),
                            ChonHoSoCommand = new Command(() => _ = XemThongTinHoSo(id))
                        };

                        _danhSachHoSo.Add(hoSo);
                        _danhSachHienThi.Add(hoSo);
                    }

                    hoSoDuocChonId = "";
                    LblKhongCoHoSo.IsVisible = !_danhSachHienThi.Any();
                }
                else
                {
                    await DisplayAlert("Thông báo", result.Message ?? "Không thể tải danh sách hồ sơ", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", ex.Message, "OK");
            }
            finally
            {
                _isLoading = false;
            }
        }
        #endregion

        #region Search
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
            => FilterHoSo(e.NewTextValue);

        private void FilterHoSo(string searchText)
        {
            _danhSachHienThi.Clear();

            var list = string.IsNullOrWhiteSpace(searchText)
                ? _danhSachHoSo
                : _danhSachHoSo.Where(h =>
                    h.HoTen.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    h.SoDienThoai.Contains(searchText) ||
                    h.CCCD.Contains(searchText));

            foreach (var hoSo in list)
                _danhSachHienThi.Add(hoSo);

            LblKhongCoHoSo.IsVisible = !_danhSachHienThi.Any();
        }
        #endregion

        #region Select hồ sơ (KHÔNG RESET ItemsSource)
        private Task TapToSelectHoSo(string hoSoId)
            => ChonHoSo(hoSoId);

        private Task ChonHoSo(string hoSoId)
        {
            if (hoSoDuocChonId == hoSoId) return Task.CompletedTask;

            foreach (var h in _danhSachHoSo)
            {
                h.IsSelected = false;
                h.BackgroundColor = Colors.White;
            }

            var hoSo = _danhSachHoSo.FirstOrDefault(h => h.Id == hoSoId);
            if (hoSo != null)
            {
                hoSo.IsSelected = true;
                hoSo.BackgroundColor = Color.FromArgb("#E8F0FE");
                hoSoDuocChonId = hoSoId;
            }

            return Task.CompletedTask;
        }
        #endregion

        #region Xử lý chọn & điều hướng
        //private async Task ChonHoSoVaChuyenTrang(string hoSoId)
        //{
        //    await ChonHoSo(hoSoId);
        //    await LuuHoSoVaChuyenTrang();
        //}

        //private async Task LuuHoSoVaChuyenTrang()
        //{
        //    if (!long.TryParse(hoSoDuocChonId, out var id)) return;

        //    var result = await _hoSoService.ChonBenhNhanAsync(id);
        //    if (result.Success)
        //    {
        //        await S0305_SecureStorage.SaveUserIdAsync(hoSoDuocChonId);
        //        await Navigation.PushAsync(new S0306_mChonCoSoKham());
        //    }
        //}
        #endregion

        #region Xem chi tiết
        private async Task XemThongTinHoSo(string hoSoId)
        {
            if (!long.TryParse(hoSoId, out var id)) return;

            var result = await _hoSoService.LayThongTinBenhNhanAsync(id);
            if (result.Success && result.Data != null)
            {
                var i = result.Data;
                //await DisplayAlert("Thông tin hồ sơ",
                //    $"Họ tên: {i.HoTen}\n" +
                //    $"Ngày sinh: {i.NgaySinh:dd-MM-yyyy}\n" +
                //    $"Giới tính: {i.GioiTinh}\n" +
                //    $"SĐT: {i.DienThoai}\n" +
                //    $"CCCD: {i.Cccd}\n" +
                //    $"Email: {i.Email}\n" +
                //    $"Địa chỉ: {i.DiaChi}",
                //    "Đóng");
                await Navigation.PushAsync(new S0306_mNhapThongTinCaNhan(result, "SuaHoSo"));
            }
        }
        #endregion

        #region Navigation
        private async void OnThemHoSoClicked(object sender, EventArgs e)
        {
            var userId = await S0305_SecureStorage.GetUserIdAsync();
            var cccd = await S0305_SecureStorage.GetUserCCCDAsync();
            var phone = await S0305_SecureStorage.GetUserPhoneAsync();

            if (!string.IsNullOrEmpty(userId))
                await Navigation.PushAsync(new S0306_mNhapThongTinCaNhan(long.Parse(userId), cccd, phone));
        }

        private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
        private async void OnRefresh(object sender, EventArgs e)
        {
            await TaiDanhSachHoSoAsync();
            RefreshViewHoSo.IsRefreshing = false;
        }

        private async void OnTrangChuClicked(object sender, EventArgs e) => await Navigation.PushAsync(new S0306_mTrangChu());
        private async void OnHoSoClicked(object sender, EventArgs e) => await Navigation.PushAsync(new S0306_mHoSoBenhNhan());
        private async void OnLichHenClicked(object sender, EventArgs e) => await Navigation.PushAsync(new S0306_mLichSuDatHen());
        private async void OnPhieuKhamClicked(object sender, EventArgs e) => await Navigation.PushAsync(new S0306_mLichSuKhamBenh());
        private async void OnTaiKhoanClicked(object sender, EventArgs e) => await Navigation.PushAsync(new S0306_mTaiKhoan());
        #endregion
    }
}
