using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using SixOSDatKhamAppMobile.Models;
using SixOSDatKhamAppMobile.Services;
using SixOSDatKhamAppMobile.Services.S0305;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mXacNhan : ContentPage, INotifyPropertyChanged
    {
        #region Properties
        private readonly S0305_DKGoiKhamService _service;
        private readonly S0305_DoiTacService _doiTacService;
        private readonly S0305_HoSoService _hoSoService;
        private System.Timers.Timer _countdownTimer;
        private int _remainingSeconds = 300; // 5 phút = 300 giây
        private bool _isLoading = false;

        // Thông tin bệnh nhân
        private string _hoTen = "";
        private string _ngaySinh = "";
        private string _gioiTinh = "";
        private string _soDienThoai = "";
        private string _diaChi = "";

        // Thông tin đặt lịch
        private string _tenGoiKham = "";
        private string _giaGoiKham = "";
        private string _loaiGoi = "";
        private string _tenChuyenGia = "";
        private bool _coChonChuyenGia = false;
        private string _ngayKham = "";
        private string _gioKham = "";
        private string _ngayGioKhamDayDu = "";
        private string _ngayDangKy = "";

        // Thông tin lịch hẹn từ API
        private string _maHen = "";
        private long _idLichHenBN = 0;

        // Thông tin cơ sở khám
        private string _tenCoSoKham = "Bệnh viện Ung Bướu TP.HCM";
        private string _diaChiCoSo = "Số 3 Nơ Trang Long, Phường Gia Định, Quận Bình Thạnh, TP.HCM";
        private string _khuKham = "Tầng 3 - Khu khám bệnh Chất lượng cao";

        // IDs để gọi API
        private long _selectedGoiKhamId = 0;
        private long _selectedChuyenGiaId = 0;
        private int _selectedKhungGioId = 0;
        private long _idDoiTac = 0;
        private long _idBenhNhan = 0;
        private string _lisIdGoiKemTheo = "";

        // Biến lưu kết quả từ API LuuDatHen
        private LuuDatHenResponseDTO _luuDatHenResult = null;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string MaHen
        {
            get => _maHen;
            set
            {
                _maHen = value;
                OnPropertyChanged();
            }
        }

        public string HoTen
        {
            get => _hoTen;
            set
            {
                _hoTen = value;
                OnPropertyChanged();
            }
        }

        public string NgaySinh
        {
            get => _ngaySinh;
            set
            {
                _ngaySinh = value;
                OnPropertyChanged();
            }
        }

        public string GioiTinh
        {
            get => _gioiTinh;
            set
            {
                _gioiTinh = value;
                OnPropertyChanged();
            }
        }

        public string SoDienThoai
        {
            get => _soDienThoai;
            set
            {
                _soDienThoai = value;
                OnPropertyChanged();
            }
        }

        public string DiaChi
        {
            get => _diaChi;
            set
            {
                _diaChi = value;
                OnPropertyChanged();
            }
        }

        public string TenGoiKham
        {
            get => _tenGoiKham;
            set
            {
                _tenGoiKham = value;
                OnPropertyChanged();
            }
        }

        public string GiaGoiKham
        {
            get => _giaGoiKham;
            set
            {
                _giaGoiKham = value;
                OnPropertyChanged();
            }
        }

        public string LoaiGoi
        {
            get => _loaiGoi;
            set
            {
                _loaiGoi = value;
                OnPropertyChanged();
            }
        }

        public string TenChuyenGia
        {
            get => _tenChuyenGia;
            set
            {
                _tenChuyenGia = value;
                OnPropertyChanged();
            }
        }

        public bool CoChonChuyenGia
        {
            get => _coChonChuyenGia;
            set
            {
                _coChonChuyenGia = value;
                OnPropertyChanged();
            }
        }

        public string NgayGioKhamDayDu
        {
            get => _ngayGioKhamDayDu;
            set
            {
                _ngayGioKhamDayDu = value;
                OnPropertyChanged();
            }
        }

        public string NgayDangKy
        {
            get => _ngayDangKy;
            set
            {
                _ngayDangKy = value;
                OnPropertyChanged();
            }
        }

        public string TenCoSoKham
        {
            get => _tenCoSoKham;
            set
            {
                _tenCoSoKham = value;
                OnPropertyChanged();
            }
        }

        public string DiaChiCoSo
        {
            get => _diaChiCoSo;
            set
            {
                _diaChiCoSo = value;
                OnPropertyChanged();
            }
        }

        public string KhuKham
        {
            get => _khuKham;
            set
            {
                _khuKham = value;
                OnPropertyChanged();
            }
        }
        #endregion
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetUseSafeArea(this))
            {
                // Sử dụng safe area
                On<iOS>().SetUseSafeArea(true);
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
        #region Constructor
        public S0306_mXacNhan()
        {
            InitializeComponent();
            _service = new S0305_DKGoiKhamService();
            _doiTacService = new S0305_DoiTacService();
            _hoSoService = new S0305_HoSoService();
            BindingContext = this;

            // Khởi tạo timer đếm ngược
            InitializeCountdownTimer();
            SetupSafeArea();
            // Ngày đăng ký là hôm nay
            NgayDangKy = DateTime.Now.ToString("dd-MM-yyyy");

            // Load dữ liệu
            Dispatcher.DispatchAsync(async () =>
            {
                await LoadDataAsync();
                await LuuDatHenTruocAsync(); // Gọi API LuuDatHen trước khi hiển thị xác nhận
            });
        }
        #endregion

        #region Private Methods
        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                // 0. Lấy ID đối tác và ID bệnh nhân
                await LoadIdDoiTacVaBenhNhan();

                // 1. Lấy thông tin chi nhánh/đối tác
                await LoadThongTinDoiTac();

                // 2. Lấy thông tin bệnh nhân
                await LoadThongTinBenhNhan();

                // 3. Lấy thông tin gói khám đã chọn
                await LoadThongTinGoiKham();

                // 4. Lấy thông tin chuyên gia (nếu có)
                await LoadThongTinChuyenGia();

                // 5. Lấy thông tin ngày giờ khám
                await LoadThongTinLichKham();

                // 6. Lấy danh sách gói kèm theo
                await LoadThongTinGoiKemTheo();

            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể tải dữ liệu: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadIdDoiTacVaBenhNhan()
        {
            try
            {
                // Lấy ID đối tác
                var idDoiTacStr = await S0305_SecureStorage.GetIdDoiTacAsync();
                if (!string.IsNullOrEmpty(idDoiTacStr))
                {
                    long.TryParse(idDoiTacStr, out _idDoiTac);
                }

                // Lấy ID bệnh nhân
                var idBenhNhanStr = await S0305_SecureStorage.GetUserIdAsync();
                if (!string.IsNullOrEmpty(idBenhNhanStr))
                {
                    long.TryParse(idBenhNhanStr, out _idBenhNhan);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy ID đối tác và bệnh nhân: {ex.Message}");
            }
        }

        private async Task LoadThongTinDoiTac()
        {
            try
            {
                if (_idDoiTac <= 0)
                {
                    Console.WriteLine("Chưa có ID đối tác");
                    return;
                }

                var result = await _doiTacService.GetBenhVienAsync();

                if (result != null && result.Success)
                {
                    // result chính là thông tin đối tác hiện tại
                    TenCoSoKham = result.TenDoiTac ?? "Bệnh viện Ung Bướu TP.HCM";
                    DiaChiCoSo = result.DiaChi ?? "Số 3 Nơ Trang Long, Phường Gia Định, Quận Bình Thạnh, TP.HCM";

                    // Lấy thêm thông tin khu khám nếu có
                    // Nếu BenhVien class có property KhuKham thì dùng, không thì để mặc định
                    KhuKham = "Tầng 3 - Khu khám bệnh Chất lượng cao";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi load thông tin đối tác: {ex.Message}");
            }
        }

        private async Task LoadThongTinBenhNhan()
        {
            try
            {
                if (_idBenhNhan <= 0)
                {
                    Console.WriteLine("Chưa có ID bệnh nhân");
                    return;
                }

                // Gọi API lấy thông tin chi tiết bệnh nhân
                var result = await _hoSoService.LayThongTinBenhNhanAsync(_idBenhNhan);

                if (result != null && result.Success && result.Data != null)
                {
                    HoTen = result.Data.HoTen ?? "";
                    NgaySinh = DateTime.TryParse(result.Data.NgaySinh, out var date)
                        ? date.ToString("dd-MM-yyyy")
                        : "";
                    SoDienThoai = result.Data.DienThoai ?? "";
                    DiaChi = result.Data.DiaChi ?? "";

                    // Xử lý giới tính
                    if (!string.IsNullOrEmpty(result.Data.GioiTinh))
                    {
                        GioiTinh = result.Data.GioiTinh;
                    }
                }
                else
                {
                    // Fallback: lấy từ SecureStorage nếu API thất bại
                    var hoTenStr = await SecureStorage.GetAsync("BenhNhanHoTen");
                    var ngaySinhStr = await SecureStorage.GetAsync("BenhNhanNgaySinh");
                    var soDienThoaiStr = await SecureStorage.GetAsync("BenhNhanSoDienThoai");
                    var diaChiStr = await SecureStorage.GetAsync("BenhNhanDiaChi");

                    if (!string.IsNullOrEmpty(hoTenStr)) HoTen = hoTenStr;
                    if (!string.IsNullOrEmpty(ngaySinhStr)) NgaySinh = ngaySinhStr;
                    if (!string.IsNullOrEmpty(soDienThoaiStr)) SoDienThoai = soDienThoaiStr;
                    if (!string.IsNullOrEmpty(diaChiStr)) DiaChi = diaChiStr;
                }

                // Lấy thông tin giới tính từ API khác nếu chưa có
                if (string.IsNullOrEmpty(GioiTinh))
                {
                    var doTuoiGioiTinhResult = await _service.LayDoTuoiGioiTinhBNAsync();
                    if (doTuoiGioiTinhResult.Success && doTuoiGioiTinhResult.Data != null)
                    {
                        long idGioiTinh = doTuoiGioiTinhResult.Data.GioiTinh ?? 0;
                        GioiTinh = idGioiTinh switch
                        {
                            1 => "Nam",
                            2 => "Nữ",
                            _ => "Khác"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi load thông tin bệnh nhân: {ex.Message}");
            }
        }

        private async Task LoadThongTinGoiKham()
        {
            try
            {
                var goiKhamIdStr = await SecureStorage.GetAsync("SelectedGoiKhamId");
                var tenGoiKhamStr = await SecureStorage.GetAsync("SelectedGoiKhamName");
                var giaGoiKhamStr = await SecureStorage.GetAsync("SelectedGoiKhamPrice");
                var loaiGoiStr = await SecureStorage.GetAsync("SelectedGoiKhamLoai");

                if (!string.IsNullOrEmpty(goiKhamIdStr))
                {
                    long.TryParse(goiKhamIdStr, out _selectedGoiKhamId);
                }

                if (!string.IsNullOrEmpty(tenGoiKhamStr))
                {
                    TenGoiKham = tenGoiKhamStr;
                }

                if (!string.IsNullOrEmpty(giaGoiKhamStr))
                {
                    if (decimal.TryParse(giaGoiKhamStr, out var gia))
                    {
                        GiaGoiKham = $"{gia:N0} VNĐ";
                    }
                    else
                    {
                        GiaGoiKham = giaGoiKhamStr;
                    }
                }

                if (!string.IsNullOrEmpty(loaiGoiStr))
                {
                    LoaiGoi = loaiGoiStr;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi load thông tin gói khám: {ex.Message}");
            }
        }

        private async Task LoadThongTinChuyenGia()
        {
            try
            {
                var chuyenGiaIdStr = await SecureStorage.GetAsync("SelectedChuyenGiaId");
                var tenChuyenGiaStr = await SecureStorage.GetAsync("SelectedChuyenGiaName");

                if (!string.IsNullOrEmpty(chuyenGiaIdStr))
                {
                    if (long.TryParse(chuyenGiaIdStr, out _selectedChuyenGiaId) && _selectedChuyenGiaId > 0)
                    {
                        CoChonChuyenGia = true;

                        if (!string.IsNullOrEmpty(tenChuyenGiaStr))
                        {
                            TenChuyenGia = tenChuyenGiaStr;
                        }
                        else
                        {
                            TenChuyenGia = "Đang tải...";
                        }
                    }
                    else
                    {
                        CoChonChuyenGia = false;
                        TenChuyenGia = "Không chọn chuyên gia";
                    }
                }
                else
                {
                    CoChonChuyenGia = false;
                    TenChuyenGia = "Không chọn chuyên gia";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi load thông tin chuyên gia: {ex.Message}");
                CoChonChuyenGia = false;
                TenChuyenGia = "Không chọn chuyên gia";
            }
        }

        private async Task LoadThongTinLichKham()
        {
            try
            {
                var ngayKhamStr = await SecureStorage.GetAsync("SelectedNgayKham");
                var gioKhamStr = await SecureStorage.GetAsync("SelectedGioKham");
                var khungGioIdStr = await SecureStorage.GetAsync("SelectedKhungGioId");

                if (!string.IsNullOrEmpty(khungGioIdStr))
                {
                    int.TryParse(khungGioIdStr, out _selectedKhungGioId);
                }

                if (!string.IsNullOrEmpty(ngayKhamStr))
                {
                    _ngayKham = ngayKhamStr;
                }

                if (!string.IsNullOrEmpty(gioKhamStr))
                {
                    _gioKham = gioKhamStr;
                }

                // Format thông tin ngày giờ khám
                if (!string.IsNullOrEmpty(_ngayKham) && !string.IsNullOrEmpty(_gioKham))
                {
                    // Parse ngày khám
                    if (DateTime.TryParse(_ngayKham, out var ngayKhamDate))
                    {
                        string dayOfWeek = ngayKhamDate.DayOfWeek switch
                        {
                            DayOfWeek.Monday => "Thứ Hai",
                            DayOfWeek.Tuesday => "Thứ Ba",
                            DayOfWeek.Wednesday => "Thứ Tư",
                            DayOfWeek.Thursday => "Thứ Năm",
                            DayOfWeek.Friday => "Thứ Sáu",
                            DayOfWeek.Saturday => "Thứ Bảy",
                            DayOfWeek.Sunday => "Chủ Nhật",
                            _ => ""
                        };

                        NgayGioKhamDayDu = $"{dayOfWeek}, {ngayKhamDate:dd/MM/yyyy} lúc {_gioKham}";
                    }
                    else
                    {
                        NgayGioKhamDayDu = $"{_ngayKham} lúc {_gioKham}";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi load thông tin lịch khám: {ex.Message}");
            }
        }

        private async Task LoadThongTinGoiKemTheo()
        {
            try
            {
                // Lấy danh sách ID gói kèm theo từ SecureStorage
                var goiKemTheoIds = await SecureStorage.GetAsync("SelectedGoiKemTheoIds");
                if (!string.IsNullOrEmpty(goiKemTheoIds))
                {
                    _lisIdGoiKemTheo = goiKemTheoIds;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi load thông tin gói kèm theo: {ex.Message}");
            }
        }

        private async Task LuuDatHenTruocAsync()
        {
            try
            {
                if (_idDoiTac <= 0 || _idBenhNhan <= 0 || _selectedGoiKhamId <= 0)
                {
                    await DisplayAlert("Lỗi", "Thiếu thông tin cần thiết để đặt lịch", "OK");
                    await Navigation.PopAsync();
                    return;
                }

                IsLoading = true;
                DateTime ngayKhamDate;

                if (!DateTime.TryParse(_ngayKham, out ngayKhamDate))
                {
                    await DisplayAlert("Lỗi", "Ngày khám không hợp lệ", "OK");
                    await Navigation.PopAsync();
                    return;
                }
                string ngayMuonDatHenFormatted = ngayKhamDate.ToString("dd-MM-yyyy");
                string gioHienTaiFormatted;

                if (!string.IsNullOrEmpty(_gioKham))
                {
                    gioHienTaiFormatted = $"{ngayMuonDatHenFormatted} {_gioKham}";
                }
                else
                {
                    gioHienTaiFormatted = $"{ngayMuonDatHenFormatted} 07:30";
                }

                // Tạo request gọi API LuuDatHen
                var request = new LuuDatHenRequestDTO
                {
                    IdBenhNhan = _idBenhNhan,
                    IdDoiTac = _idDoiTac,
                    IdGoi = _selectedGoiKhamId,
                    NgayMuonDatHen = ngayMuonDatHenFormatted,  
                    GioHienTai = gioHienTaiFormatted,         
                    LisIdGoiKemTheo = _lisIdGoiKemTheo ?? "",
                    IdChuyenGia = _selectedChuyenGiaId
                };

                // Gọi API LuuDatHen
                var result = await _service.LuuDatHenAsync(request);

                IsLoading = false;

                if (result.Success)
                {
                    _luuDatHenResult = result.Data;

                    if (_luuDatHenResult != null)
                    {
                        _maHen = _luuDatHenResult.MaHen;
                        _idLichHenBN = _luuDatHenResult.IdHen;

                        if (!string.IsNullOrEmpty(_luuDatHenResult.Thoigian))
                        {
                            NgayGioKhamDayDu = _luuDatHenResult.Thoigian;
                        }

                        if (result.Message?.Contains("Bạn đã đăng ký lịch khám trước đây") == true)
                        {
                            await HienThiModalXacNhanLichMoi();
                        }
                        else if (result.Message?.Contains("Đặt lịch thành công") == true)
                        {
                            StartCountdown();
                        }
                        else if (result.Message?.Contains("Không còn lịch trống") == true)
                        {
                            await DisplayAlert("Thông báo", result.Message, "OK");
                            await Navigation.PopAsync();
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Lỗi", result.Message ?? "Không thể đặt lịch", "OK");
                    await Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                IsLoading = false;
                await DisplayAlert("Lỗi", $"Không thể đặt lịch: {ex.Message}", "OK");
                await Navigation.PopAsync();
            }
        }
        private async Task HienThiModalXacNhanLichMoi()
        {
            // Tự động chấp nhận thay đổi lịch
            StartCountdown();
        }

        private void InitializeCountdownTimer()
        {
            _countdownTimer = new System.Timers.Timer(1000); // Cập nhật mỗi giây
            _countdownTimer.Elapsed += OnCountdownTimerElapsed;
            _countdownTimer.AutoReset = true;
        }

        private void StartCountdown()
        {
            _remainingSeconds = 300; // Reset về 5 phút
            UpdateCountdownDisplay();
            _countdownTimer.Start();
        }

        private void StopCountdown()
        {
            _countdownTimer?.Stop();
        }

        private void OnCountdownTimerElapsed(object sender, ElapsedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _remainingSeconds--;

                if (_remainingSeconds <= 0)
                {
                    StopCountdown();
                    AutoCancelRegistration();
                }
                else
                {
                    UpdateCountdownDisplay();
                }
            });
        }

        private void UpdateCountdownDisplay()
        {
            int minutes = _remainingSeconds / 60;
            int seconds = _remainingSeconds % 60;
            LblCountdown.Text = $"{minutes:00}:{seconds:00}";

            // Đổi màu khi còn dưới 1 phút
            if (_remainingSeconds <= 60)
            {
                LblCountdown.TextColor = Color.FromArgb("#EF4444"); // Đỏ
            }
            else if (_remainingSeconds <= 120)
            {
                LblCountdown.TextColor = Color.FromArgb("#EF4444"); // Cam
            }
            else
            {
                LblCountdown.TextColor = Color.FromArgb("#EF4444"); // Xanh lá
            }
        }

        private async void AutoCancelRegistration()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                LblCountdown.Text = "00:00";

                // Gọi API hủy lịch khi hết thời gian
                await HuyLichHenAuto();

                await DisplayAlert("Hết thời gian", "Thời gian đặt chỗ đã hết. Lịch hẹn đã bị hủy.", "OK");
                await Navigation.PopAsync();
            });
        }

        private async Task HuyLichHenAuto()
        {
            try
            {
                if (_idLichHenBN > 0 && !string.IsNullOrEmpty(_maHen))
                {
                    var request = new ChotLichHenRequestDTO
                    {
                        IdBenhNhan = _idBenhNhan,
                        MaHen = _maHen,
                        IDLichHenBN = _idLichHenBN,
                        TrangThai = false // Hủy lịch
                    };

                    await _service.ChotLichHenAsync(request);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi hủy lịch tự động: {ex.Message}");
            }
        }
        #endregion

        #region Lifecycle Methods
       
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopCountdown();
        }
        #endregion

        #region Event Handlers
        private async void OnBackClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "Xác nhận",
                "Bạn có chắc chắn muốn quay lại? Lịch hẹn tạm thời sẽ bị hủy.",
                "Có",
                "Không");

            if (confirm)
            {
                StopCountdown();

                // Hủy lịch tạm thời khi quay lại
                if (_idLichHenBN > 0 && !string.IsNullOrEmpty(_maHen))
                {
                    try
                    {
                        var request = new ChotLichHenRequestDTO
                        {
                            IdBenhNhan = _idBenhNhan,
                            MaHen = _maHen,
                            IDLichHenBN = _idLichHenBN,
                            TrangThai = false // Hủy lịch
                        };

                        await _service.ChotLichHenAsync(request);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi khi hủy lịch: {ex.Message}");
                    }
                }

                await Navigation.PopAsync();
            }
        }

        private async void OnHuyDangKyClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "Xác nhận hủy",
                "Bạn có chắc chắn muốn hủy đăng ký khám này?",
                "Có, hủy đăng ký",
                "Không");

            if (confirm)
            {
                try
                {
                    IsLoading = true;

                    if (_idLichHenBN > 0 && !string.IsNullOrEmpty(_maHen))
                    {
                        // Gọi API ChotLichHen với TrangThai = false (hủy lịch)
                        var request = new ChotLichHenRequestDTO
                        {
                            IdBenhNhan = _idBenhNhan,
                            MaHen = _maHen,
                            IDLichHenBN = _idLichHenBN,
                            TrangThai = false
                        };

                        var result = await _service.ChotLichHenAsync(request);

                        IsLoading = false;

                        if (result.Success)
                        {
                            // Dừng timer
                            StopCountdown();

                            await DisplayAlert("Thành công", "Đã hủy đăng ký khám thành công.", "OK");

                            await Navigation.PopAsync();
                        }
                        else
                        {
                            await DisplayAlert("Lỗi", result.Message ?? "Không thể hủy đăng ký", "OK");
                        }
                    }
                    else
                    {
                        IsLoading = false;
                        await DisplayAlert("Lỗi", "Không tìm thấy thông tin lịch hẹn", "OK");
                    }
                }
                catch (Exception ex)
                {
                    IsLoading = false;
                    await DisplayAlert("Lỗi", $"Không thể hủy đăng ký: {ex.Message}", "OK");
                }
            }
        }

        private async void OnXacNhanDangKyClicked(object sender, EventArgs e)
        {
            // Dừng timer khi xác nhận
            StopCountdown();

            try
            {
                IsLoading = true;

                    // Gọi API ChotLichHen với TrangThai = true (xác nhận đặt hẹn)
                    var request = new ChotLichHenRequestDTO
                    {
                        IdBenhNhan = _idBenhNhan,
                        MaHen = _maHen,
                        IDLichHenBN = _idLichHenBN,
                        TrangThai = true
                    };

                var result = await _service.ChotLichHenAsync(request);

                IsLoading = false;

                if (result.Success)
                {
                    // Xóa dữ liệu tạm
                    await ClearTemporaryData();

                    // Chuyển về trang chủ
                    await Navigation.PushAsync(new S0306_mTrangChu());
                }
                else
                {
                    // Nếu lỗi → tiếp tục đếm ngược
                    StartCountdown();
                }
            }
            catch
            {
                IsLoading = false;

                StartCountdown();
            }
        }


        private async Task ClearTemporaryData()
        {
            try
            {
                await SecureStorage.SetAsync("SelectedGoiKhamId", "");
                await SecureStorage.SetAsync("SelectedGoiKhamName", "");
                await SecureStorage.SetAsync("SelectedGoiKhamPrice", "");
                await SecureStorage.SetAsync("SelectedGoiKhamLoai", "");
                await SecureStorage.SetAsync("SelectedChuyenGiaId", "");
                await SecureStorage.SetAsync("SelectedChuyenGiaName", "");
                await SecureStorage.SetAsync("SelectedNgayKham", "");
                await SecureStorage.SetAsync("SelectedGioKham", "");
                await SecureStorage.SetAsync("SelectedKhungGioId", "");
                await SecureStorage.SetAsync("SelectedGoiKemTheoIds", "");

                // Lưu mã hẹn để hiển thị ở trang chủ
                if (!string.IsNullOrEmpty(_maHen))
                {
                    await SecureStorage.SetAsync("LastMaHen", _maHen);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa dữ liệu tạm: {ex.Message}");
            }
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