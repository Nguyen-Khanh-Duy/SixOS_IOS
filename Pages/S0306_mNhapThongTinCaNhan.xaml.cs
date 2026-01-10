using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Layouts;
using SixOSDatKhamAppMobile.Controls;
using SixOSDatKhamAppMobile.Models;
using SixOSDatKhamAppMobile.Services.S0305;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Picker = Microsoft.Maui.Controls.Picker;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mNhapThongTinCaNhan : ContentPage
    {
        private DateTime? selectedDate = null;
        private bool isDatePickerVisible = false;
        private bool _isInternalUpdate = false;
        private readonly object _parentPage;
        private readonly long _userId;
        private readonly string _cccd;
        private readonly string _flag = "";
        private readonly S0305_PatientInfoService _patientInfoService;
        private readonly S0305_PickerService _pickerService;

        private long _selectedPhuongXaId = 0;


        // Collections cho các picker
        private ObservableCollection<PickerItem> _gioiTinhItems = new ObservableCollection<PickerItem>();
        private ObservableCollection<PickerItem> _danTocItems = new ObservableCollection<PickerItem>();
        private ObservableCollection<PickerItem> _tinhThanhItems = new ObservableCollection<PickerItem>();
        private ObservableCollection<PickerItem> _quocGiaItems = new ObservableCollection<PickerItem>();
        private ObservableCollection<PickerItem> _ngheNghiepItems = new ObservableCollection<PickerItem>();
        private ObservableCollection<PickerItem> _quanHuyenItems = new ObservableCollection<PickerItem>();
        private ObservableCollection<PickerItem> _xaPhuongItems = new ObservableCollection<PickerItem>();
        private object? parentPage;

        public S0306_mNhapThongTinCaNhan(long userId, string cccd, string soDienThoai)
        {
            InitializeComponent();
            _parentPage = parentPage;
            _userId = userId;
            _cccd = cccd;
            _patientInfoService = new S0305_PatientInfoService();
            _pickerService = new S0305_PickerService();
            _flag = "LuuHoSo";

            // Binding dữ liệu cho pickers
            pickerGioiTinh.ItemsSource = _gioiTinhItems;
            pickerDanToc.ItemsSource = _danTocItems;
            pickerTinhThanh.ItemsSource = _tinhThanhItems;
            pickerQuocTich.ItemsSource = _quocGiaItems;
            pickerNgheNghiep.ItemsSource = _ngheNghiepItems;
            pickerPhuongXa.ItemsSource = _xaPhuongItems;

            // Ẩn tất cả các thông báo lỗi
            HideAllErrorMessages();

            // Thiết lập sự kiện khi mất focus
            SetupEntryFocusEvents();
            SetupSafeArea();
            // Tự động điền số CCCD
            entryCCCD.Text = cccd;
            entryCCCD.IsEnabled = false;

            entrySoDienThoai.Text = soDienThoai;

            // Load dữ liệu picker với loading
            _ = InitializePageDataAsync();
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
        public S0306_mNhapThongTinCaNhan(HoSoDetailResponse hoSoDetailResponse, string flag)
        {
            InitializeComponent();
            _parentPage = parentPage;
            _userId = hoSoDetailResponse.Data.Id;
            _cccd = hoSoDetailResponse.Data.Cccd ?? "";
            _patientInfoService = new S0305_PatientInfoService();
            _pickerService = new S0305_PickerService();
            _flag = flag;

            // Binding dữ liệu cho pickers
            pickerGioiTinh.ItemsSource = _gioiTinhItems;
            pickerDanToc.ItemsSource = _danTocItems;
            pickerTinhThanh.ItemsSource = _tinhThanhItems;
            pickerQuocTich.ItemsSource = _quocGiaItems;
            pickerNgheNghiep.ItemsSource = _ngheNghiepItems;
            pickerPhuongXa.ItemsSource = _xaPhuongItems;

            // Ẩn tất cả các thông báo lỗi
            HideAllErrorMessages();

            // Thiết lập sự kiện khi mất focus
            SetupEntryFocusEvents();

            // Load dữ liệu picker và điền form với loading
            LoadPickerDataAndFillFormAsync(hoSoDetailResponse);
        }

        #region Loading Methods

        /// <summary>
        /// Hiển thị/Ẩn loading overlay với text tùy chỉnh
        /// </summary>
        private void ShowLoadingOverlay(bool show, string message = "Đang tải...", string subtext = "Vui lòng đợi")
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                loadingOverlay.IsVisible = show;
                loadingSpinner.IsRunning = show;

                if (show)
                {
                    loadingText.Text = message;
                    loadingSubtext.Text = subtext;
                }
            });
        }

        /// <summary>
        /// Khởi tạo dữ liệu trang lần đầu (cho constructor thứ nhất)
        /// </summary>
        private async Task InitializePageDataAsync()
        {
            try
            {
                ShowLoadingOverlay(true, "Đang tải dữ liệu...", "Vui lòng đợi trong giây lát");

                await LoadPickerDataAsync();

                // Đợi thêm 500ms để đảm bảo UI đã render xong
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể tải dữ liệu: {ex.Message}", "OK");
            }
            finally
            {
                ShowLoadingOverlay(false);
            }
        }

        #endregion

        /// Load dữ liệu picker và điền form
        private async void LoadPickerDataAndFillFormAsync(HoSoDetailResponse hoSoDetailResponse)
        {
            try
            {
                ShowLoadingOverlay(true, "Đang tải thông tin...", "Đang xử lý dữ liệu của bạn");

                // Load dữ liệu picker trước
                await LoadPickerDataAsync();

                // Sau khi load xong, điền dữ liệu vào form
                await Device.InvokeOnMainThreadAsync(async () =>
                {
                    // Điền các trường text đơn giản
                    entryHoTen.Text = hoSoDetailResponse.Data.HoTen;
                    entryNgaySinh.Text = hoSoDetailResponse.Data.NgaySinh;
                    entryCCCD.Text = hoSoDetailResponse.Data.Cccd;
                    entrySoDienThoai.Text = hoSoDetailResponse.Data.DienThoai;
                    entryDiaChi.Text = hoSoDetailResponse.Data.DiaChi;
                    entryEmail.Text = hoSoDetailResponse.Data.Email;

                    // Parse ngày sinh
                    if (!string.IsNullOrWhiteSpace(hoSoDetailResponse.Data.NgaySinh))
                    {
                        if (DateTime.TryParseExact(hoSoDetailResponse.Data.NgaySinh,
                            new[] { "dd/MM/yyyy", "yyyy-MM-dd" },
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateTime parsedDate))
                        {
                            selectedDate = parsedDate;
                        }
                    }

                    // Đợi một chút để đảm bảo UI đã render
                    await Task.Delay(100);

                    // Chọn các giá trị picker từ string
                    SelectGioiTinhByName(hoSoDetailResponse.Data.GioiTinh);
                    SelectQuocTichByName(hoSoDetailResponse.Data.QuocTich);
                    SelectDanTocByName(hoSoDetailResponse.Data.DanToc);
                    SelectNgheNghiepByName(hoSoDetailResponse.Data.NgheNghiep);
                    SelectTinhThanhByName(hoSoDetailResponse.Data.TinhThanh);

                    // Đợi UI cập nhật xong chọn tỉnh
                    await Task.Delay(200);

                    // Nếu có tỉnh thành, load dữ liệu địa chỉ cấp dưới
                    if (!string.IsNullOrWhiteSpace(hoSoDetailResponse.Data.TinhThanh) &&
                        pickerTinhThanh.SelectedItem != null)
                    {
                        // Load quận/huyện và xã/phường
                        await LoadAddressDataForSelectedProvince();

                        // Đợi load xong
                        await Task.Delay(300);

                        // Chọn phường/xã theo tên
                        if (!string.IsNullOrWhiteSpace(hoSoDetailResponse.Data.PhuongXa))
                        {
                            SelectPhuongXaByName(hoSoDetailResponse.Data.PhuongXa);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể tải dữ liệu: {ex.Message}", "OK");
            }
            finally
            {
                // Đợi thêm 300ms để người dùng thấy dữ liệu đã được điền
                await Task.Delay(300);
                ShowLoadingOverlay(false);
            }
        }

        private async Task LoadAddressDataForSelectedProvince()
        {
            try
            {
                var selectedItem = pickerTinhThanh.SelectedItem as PickerItem;
                if (selectedItem == null) return;

                // Lấy quận/huyện theo tỉnh
                var quanHuyenList = await _pickerService.GetQuanHuyenByTinhIdAsync(selectedItem.Id);

                // Lấy xã/phường theo tỉnh
                var xaPhuongList = await _pickerService.GetXaPhuongByTinhIdAsync(selectedItem.Id);

                await Device.InvokeOnMainThreadAsync(() =>
                {
                    // Đổ dữ liệu quận/huyện
                    _quanHuyenItems.Clear();
                    foreach (var item in quanHuyenList)
                    {
                        _quanHuyenItems.Add(new PickerItem
                        {
                            Id = item.Id,
                            Name = item.Ten,
                            Value = item.Id.ToString()
                        });
                    }

                    // Đổ dữ liệu xã/phường
                    _xaPhuongItems.Clear();
                    foreach (var item in xaPhuongList)
                    {
                        _xaPhuongItems.Add(new PickerItem
                        {
                            Id = item.Id,
                            Name = item.Ten,
                            Value = item.Id.ToString()
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading address data: {ex.Message}");
            }
        }

        private async Task LoadPickerDataAsync()
        {
            try
            {
                // Lấy tất cả dữ liệu picker
                var pickerData = await _pickerService.GetAllPickerDataAsync();

                if (pickerData != null)
                {
                    // Đổ dữ liệu vào các collection
                    await Device.InvokeOnMainThreadAsync(() =>
                    {
                        // Giới tính
                        _gioiTinhItems.Clear();
                        foreach (var item in pickerData.GioiTinh)
                        {
                            _gioiTinhItems.Add(new PickerItem
                            {
                                Id = item.Id,
                                Name = item.Ten,
                                Value = item.Id.ToString()
                            });
                        }

                        // Dân tộc
                        _danTocItems.Clear();
                        foreach (var item in pickerData.DanToc)
                        {
                            _danTocItems.Add(new PickerItem
                            {
                                Id = item.Id,
                                Name = item.Ten,
                                Value = item.Id.ToString()
                            });
                        }

                        // Tỉnh thành
                        _tinhThanhItems.Clear();
                        foreach (var item in pickerData.TinhThanh)
                        {
                            _tinhThanhItems.Add(new PickerItem
                            {
                                Id = item.Id,
                                Name = item.Ten,
                                Value = item.Id.ToString()
                            });
                        }

                        // Quốc gia
                        _quocGiaItems.Clear();
                        foreach (var item in pickerData.QuocGia)
                        {
                            _quocGiaItems.Add(new PickerItem
                            {
                                Id = item.Id,
                                Name = item.Ten,
                                Value = item.Id.ToString()
                            });
                        }

                        // Nghề nghiệp
                        _ngheNghiepItems.Clear();
                        foreach (var item in pickerData.NgheNghiep)
                        {
                            _ngheNghiepItems.Add(new PickerItem
                            {
                                Id = item.Id,
                                Name = item.Ten,
                                Value = item.Id.ToString()
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể tải dữ liệu picker: {ex.Message}", ex);
            }
        }

        private async void OnTinhThanhSelected(object sender, EventArgs e)
        {
            try
            {
                if (pickerTinhThanh.SelectedIndex == -1)
                {
                    // Xóa dữ liệu quận/huyện và xã/phường khi chưa chọn tỉnh
                    _quanHuyenItems.Clear();
                    _xaPhuongItems.Clear();
                    pickerPhuongXa.SelectedIndex = -1;
                    return;
                }

                var selectedItem = pickerTinhThanh.SelectedItem as PickerItem;
                if (selectedItem == null) return;

                // Hiển thị loading
                ShowLoading(true);

                // Lấy quận/huyện theo tỉnh
                var quanHuyenList = await _pickerService.GetQuanHuyenByTinhIdAsync(selectedItem.Id);

                // Lấy xã/phường theo tỉnh
                var xaPhuongList = await _pickerService.GetXaPhuongByTinhIdAsync(selectedItem.Id);

                await Device.InvokeOnMainThreadAsync(() =>
                {
                    // Đổ dữ liệu quận/huyện
                    _quanHuyenItems.Clear();
                    foreach (var item in quanHuyenList)
                    {
                        _quanHuyenItems.Add(new PickerItem
                        {
                            Id = item.Id,
                            Name = item.Ten,
                            Value = item.Id.ToString()
                        });
                    }

                    // Đổ dữ liệu xã/phường
                    _xaPhuongItems.Clear();
                    foreach (var item in xaPhuongList)
                    {
                        _xaPhuongItems.Add(new PickerItem
                        {
                            Id = item.Id,
                            Name = item.Ten,
                            Value = item.Id.ToString()
                        });
                    }

                    // Reset selection
                    pickerPhuongXa.SelectedIndex = -1;
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể tải dữ liệu địa chỉ: {ex.Message}", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void SetupEntryFocusEvents()
        {
            var entries = new[] { entryHoTen, entryCCCD, entrySoDienThoai, entryDiaChi, entryEmail };
            foreach (var entry in entries)
            {
                if (entry != null)
                    entry.Unfocused += OnEntryUnfocused;
            }
        }

        private void ShowLoading(bool show)
        {
            var controls = new Microsoft.Maui.Controls.VisualElement[]
            {
                entryHoTen, entryNgaySinh, pickerGioiTinh, entryCCCD,
                pickerQuocTich, pickerNgheNghiep, pickerDanToc,
                entrySoDienThoai, entryDiaChi, pickerTinhThanh,
                pickerPhuongXa, entryEmail
            };

            foreach (var control in controls)
            {
                if (control != null)
                    control.IsEnabled = !show;
            }
        }

        #region Picker Selection Helpers

        /// Chọn item trong picker dựa trên tên (string)
        private void SelectPickerItemByName(Picker picker, ObservableCollection<PickerItem> items, string name)
        {
            if (string.IsNullOrWhiteSpace(name) || items == null || items.Count == 0)
                return;

            try
            {
                var normalizedName = name.Trim().ToLower();

                // Tìm item khớp chính xác
                var item = items.FirstOrDefault(x =>
                    x.Name?.Trim().ToLower() == normalizedName);

                // Nếu không tìm thấy khớp chính xác, tìm item chứa tên
                if (item == null)
                {
                    item = items.FirstOrDefault(x =>
                        x.Name?.ToLower().Contains(normalizedName) == true ||
                        normalizedName.Contains(x.Name?.ToLower() ?? ""));
                }

                if (item != null)
                {
                    picker.SelectedItem = item;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Không tìm thấy item cho: {name}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error selecting picker item by name: {ex.Message}");
            }
        }

        /// Chọn item trong picker dựa trên ID
        private void SelectPickerItemById(Picker picker, ObservableCollection<PickerItem> items, long id)
        {
            if (id <= 0 || items == null || items.Count == 0)
                return;

            try
            {
                var item = items.FirstOrDefault(x => x.Id == id);

                if (item != null)
                {
                    picker.SelectedItem = item;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Không tìm thấy item cho ID: {id}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error selecting picker item by ID: {ex.Message}");
            }
        }

        /// Chọn Dân tộc theo tên
        private void SelectDanTocByName(string danTocName)
        {
            SelectPickerItemByName(pickerDanToc, _danTocItems, danTocName);
        }

        /// Chọn Quốc tịch theo tên
        private void SelectQuocTichByName(string quocTichName)
        {
            SelectPickerItemByName(pickerQuocTich, _quocGiaItems, quocTichName);
        }

        /// Chọn Nghề nghiệp theo tên
        private void SelectNgheNghiepByName(string ngheNghiepName)
        {
            SelectPickerItemByName(pickerNgheNghiep, _ngheNghiepItems, ngheNghiepName);
        }

        /// Chọn Tỉnh/Thành phố theo tên
        private async void SelectTinhThanhByName(string tinhThanhName)
        {
            SelectPickerItemByName(pickerTinhThanh, _tinhThanhItems, tinhThanhName);

            // Nếu đã chọn tỉnh và có tên, load dữ liệu địa chỉ
            if (!string.IsNullOrWhiteSpace(tinhThanhName) && pickerTinhThanh.SelectedItem != null)
            {
                // Đợi UI cập nhật
                await Task.Delay(100);

                // Load quận/huyện và xã/phường
                await LoadAddressDataForSelectedProvince();
            }
        }

        /// Chọn Phường/Xã theo tên
        private void SelectPhuongXaByName(string phuongXaName)
        {
            SelectPickerItemByName(pickerPhuongXa, _xaPhuongItems, phuongXaName);
        }

        /// Chọn Phường/Xã theo ID
        private void SelectPhuongXaById(long phuongXaId)
        {
            SelectPickerItemById(pickerPhuongXa, _xaPhuongItems, phuongXaId);
        }

        /// Chọn Giới tính theo tên
        private void SelectGioiTinhByName(string gioiTinhName)
        {
            SelectPickerItemByName(pickerGioiTinh, _gioiTinhItems, gioiTinhName);
        }

        #endregion

        // Xử lý nút quét mã QR
 private async void OnQuetQRClicked(object sender, EventArgs e) { try { // Kiểm tra quyền camera var status = await Permissions.CheckStatusAsync<Permissions.Camera>();  if (status != PermissionStatus.Granted) { // Hiển thị dialog yêu cầu quyền với UX tốt var result = await DisplayAlert("Cần quyền truy cập camera", "Ứng dụng cần quyền truy cập camera để quét mã QR trên CCCD.\n\nBạn có cho phép sử dụng camera không?", "Cho phép", "Từ chối");  if (result) { status = await Permissions.RequestAsync<Permissions.Camera>();  if (status != PermissionStatus.Granted) { await DisplayAlert("Thông báo", "Bạn cần cấp quyền camera trong cài đặt để sử dụng tính năng quét QR.", "OK"); return; } } else { return; } }  // Mở trang quét QR var scannerPage = new S0306_mQRScannerPage();  // Hiển thị loading nhẹ trước khi mở scanner ShowLoading(true); await Task.Delay(100);  // Mở trang quét QR await Navigation.PushAsync(scannerPage); ShowLoading(false);  // Đợi kết quả quét var qrData = await scannerPage.WaitForScanResultAsync();  if (!string.IsNullOrEmpty(qrData)) { await ProcessQRResult(qrData); } } catch (Exception ex) { ShowLoading(false); await DisplayAlert("Lỗi", $"Không thể quét mã QR: {ex.Message}", "OK"); } }
        private async Task ProcessQRResult(string qrData)
        {
            try
            {
                ShowLoadingOverlay(true, "Đang xử lý QR...", "Đang đọc thông tin từ CCCD");

                var cccdInfo = S0305_QRCCCDParserService.ParseQRData(qrData);

                if (cccdInfo == null || !S0305_QRCCCDParserService.ValidateCCCDInfo(cccdInfo))
                {
                    ShowLoadingOverlay(false);
                    await DisplayAlert("Lỗi",
                        "Không thể đọc thông tin từ mã QR. Vui lòng kiểm tra lại hoặc nhập thủ công.",
                        "OK");
                    return;
                }

                // Điền thông tin vào form
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    // 1. Số CCCD (nếu chưa có)
                    entryCCCD.Text = cccdInfo.SoCCCD;

                    // 2. Họ tên
                    if (string.IsNullOrWhiteSpace(entryHoTen.Text) && !string.IsNullOrWhiteSpace(cccdInfo.HoTen))
                    {
                        entryHoTen.Text = cccdInfo.HoTen;
                    }

                    // 3. Số SDT 
                    //entrySoDienThoai.Text = "";

                    // 4. Ngày sinh
                    if (!selectedDate.HasValue && cccdInfo.NgaySinh.HasValue)
                    {
                        selectedDate = cccdInfo.NgaySinh.Value;
                        entryNgaySinh.Text = cccdInfo.NgaySinh.Value.ToString("dd/MM/yyyy");
                        ValidateNgaySinh();
                    }

                    // 5. Giới tính
                    if (pickerGioiTinh.SelectedIndex == -1 && !string.IsNullOrWhiteSpace(cccdInfo.GioiTinh))
                    {
                        SelectGioiTinhByName(cccdInfo.GioiTinh);
                    }

                    // 6. Quốc tịch mặc định Việt Nam (ID = 190)
                    if (pickerQuocTich.SelectedIndex == -1)
                    {
                        SelectQuocTich(190);
                    }

                    // 7. Địa chỉ (chỉ lưu phần chung, không phân cấp)
                    if (string.IsNullOrWhiteSpace(entryDiaChi.Text) && !string.IsNullOrWhiteSpace(cccdInfo.DiaChi))
                    {
                        entryDiaChi.Text = cccdInfo.DiaChi;
                        ValidateDiaChi();
                    }
                });

                // 8. Tỉnh/Thành phố - tìm và chọn từ QR (chỉ lấy Tỉnh)
                if (!string.IsNullOrWhiteSpace(cccdInfo.TinhThanh))
                {
                    await SelectTinhThanhFromQR(cccdInfo.TinhThanh);
                }

                // Đợi thêm để người dùng thấy dữ liệu đã được điền
                await Task.Delay(500);
                ShowLoadingOverlay(false);

                // Hiển thị thông báo thành công với animation
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Thành công",
                        "✅ Đã quét mã QR và điền thông tin tự động.\n\nVui lòng kiểm tra và bổ sung các thông tin còn thiếu.",
                        "OK");
                });
            }
            catch (Exception ex)
            {
                ShowLoadingOverlay(false);
                await DisplayAlert("Lỗi", $"Lỗi xử lý dữ liệu QR: {ex.Message}", "OK");
            }
        }

        /// Chọn quốc tịch dựa trên ID
        private void SelectQuocTich(long quocTichId)
        {
            try
            {
                var quocTich = _quocGiaItems.FirstOrDefault(q => q.Id == quocTichId);

                if (quocTich != null)
                {
                    pickerQuocTich.SelectedItem = quocTich;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error selecting quoc tich: {ex.Message}");
            }
        }

        /// Tìm và chọn tỉnh/thành phố từ tên trong QR
        private async Task SelectTinhThanhFromQR(string tinhName)
        {
            try
            {
                // Chuẩn hóa tên tỉnh
                var normalizedName = S0305_QRCCCDParserService.NormalizeTinhName(tinhName).ToLower();

                // Tìm tỉnh trong danh sách
                var tinh = _tinhThanhItems.FirstOrDefault(t =>
                {
                    var normalizedTinh = S0305_QRCCCDParserService.NormalizeTinhName(t.Name).ToLower();

                    // So sánh chính xác hoặc chứa
                    return normalizedTinh == normalizedName ||
                           normalizedTinh.Contains(normalizedName) ||
                           normalizedName.Contains(normalizedTinh);
                });

                if (tinh != null)
                {
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
                        pickerTinhThanh.SelectedItem = tinh;

                        // Trigger event để load quận/huyện và xã/phường
                        await Task.Delay(100);
                        OnTinhThanhSelected(pickerTinhThanh, EventArgs.Empty);
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Không tìm thấy tỉnh: {tinhName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error selecting tinh thanh: {ex.Message}");
            }
        }

        #region Validation Methods
        private bool ValidateNgaySinh()
        {
            if (!selectedDate.HasValue)
            {
                ShowFieldError(lblErrorNgaySinh, "Vui lòng chọn ngày sinh");
                return false;
            }

            var age = CalculateAge(selectedDate.Value);
            if (age < 0)
            {
                ShowFieldError(lblErrorNgaySinh, "Ngày sinh không thể ở tương lai");
                return false;
            }
            else if (age > 120)
            {
                ShowFieldError(lblErrorNgaySinh, "Ngày sinh không hợp lệ");
                return false;
            }
            else if (age < 16)
            {
                // Chỉ cảnh báo nhưng vẫn cho phép
                lblErrorNgaySinh.Text = $"Bạn mới {age} tuổi. Vui lòng đảm bảo thông tin chính xác.";
                lblErrorNgaySinh.IsVisible = true;
                lblErrorNgaySinh.TextColor = Color.FromArgb("#F59E0B");
                return true;
            }

            HideFieldError(lblErrorNgaySinh);
            return true;
        }

        private int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }

        private bool ValidateHoTen()
        {
            var hoTen = entryHoTen.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(hoTen))
            {
                ShowFieldError(lblErrorHoTen, "Vui lòng nhập họ và tên");
                return false;
            }

            if (hoTen.Length < 2 || hoTen.Length > 50)
            {
                ShowFieldError(lblErrorHoTen, "Họ và tên phải từ 2 đến 50 ký tự");
                return false;
            }

            HideFieldError(lblErrorHoTen);
            return true;
        }

        private bool ValidateGioiTinh()
        {
            if (pickerGioiTinh.SelectedIndex == -1)
            {
                ShowFieldError(lblErrorGioiTinh, "Vui lòng chọn giới tính");
                return false;
            }

            HideFieldError(lblErrorGioiTinh);
            return true;
        }

        private bool ValidateCCCD()
        {
            var cccd = entryCCCD.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(cccd))
            {
                ShowFieldError(lblErrorCCCD, "Vui lòng nhập số CCCD");
                return false;
            }

            if (!Regex.IsMatch(cccd, @"^\d{9}$|^\d{12}$"))
            {
                ShowFieldError(lblErrorCCCD, "Số CCCD phải có 9 hoặc 12 chữ số");
                return false;
            }

            HideFieldError(lblErrorCCCD);
            return true;
        }

        private bool ValidateQuocTich()
        {
            if (pickerQuocTich.SelectedIndex == -1)
            {
                ShowFieldError(lblErrorQuocTich, "Vui lòng chọn quốc tịch");
                return false;
            }

            HideFieldError(lblErrorQuocTich);
            return true;
        }

        private bool ValidateNgheNghiep()
        {
            if (pickerNgheNghiep.SelectedIndex == -1)
            {
                ShowFieldError(lblErrorNgheNghiep, "Vui lòng chọn nghề nghiệp");
                return false;
            }

            HideFieldError(lblErrorNgheNghiep);
            return true;
        }

        private bool ValidateDanToc()
        {
            if (pickerDanToc.SelectedIndex == -1)
            {
                ShowFieldError(lblErrorDanToc, "Vui lòng chọn dân tộc");
                return false;
            }

            HideFieldError(lblErrorDanToc);
            return true;
        }

        private bool ValidateSoDienThoai()
        {
            var sdt = entrySoDienThoai.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(sdt))
            {
                ShowFieldError(lblErrorSoDienThoai, "Vui lòng nhập số điện thoại");
                return false;
            }

            if (!Regex.IsMatch(sdt, @"^(03|05|07|08|09)\d{8}$"))
            {
                ShowFieldError(lblErrorSoDienThoai, "Số điện thoại không hợp lệ");
                return false;
            }

            HideFieldError(lblErrorSoDienThoai);
            return true;
        }

        private bool ValidateDiaChi()
        {
            var diaChi = entryDiaChi.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(diaChi))
            {
                ShowFieldError(lblErrorDiaChi, "Vui lòng nhập địa chỉ");
                return false;
            }

            if (diaChi.Length < 5)
            {
                ShowFieldError(lblErrorDiaChi, "Địa chỉ quá ngắn");
                return false;
            }

            HideFieldError(lblErrorDiaChi);
            return true;
        }

        private bool ValidateTinhThanh()
        {
            if (pickerTinhThanh.SelectedIndex == -1)
            {
                ShowFieldError(lblErrorTinhThanh, "Vui lòng chọn tỉnh/thành phố");
                return false;
            }

            HideFieldError(lblErrorTinhThanh);
            return true;
        }

        private bool ValidatePhuongXa()
        {
            if (pickerPhuongXa.SelectedIndex == -1)
            {
                ShowFieldError(lblErrorPhuongXa, "Vui lòng chọn phường/xã");
                return false;
            }

            HideFieldError(lblErrorPhuongXa);
            return true;
        }

        private bool ValidateEmail()
        {
            var email = entryEmail.Text?.Trim() ?? "";

            // Nếu không nhập email thì coi như hợp lệ (không bắt buộc)
            if (string.IsNullOrWhiteSpace(email))
            {
                HideFieldError(lblErrorEmail);
                return true;
            }

            // Nếu có nhập thì validate định dạng
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ShowFieldError(lblErrorEmail, "Email không hợp lệ");
                return false;
            }

            HideFieldError(lblErrorEmail);
            return true;
        }

        private bool ValidateAllFields()
        {
            bool isValid = true;

            isValid &= ValidateHoTen();
            isValid &= ValidateNgaySinh();
            isValid &= ValidateGioiTinh();
            isValid &= ValidateCCCD();
            isValid &= ValidateQuocTich();
            isValid &= ValidateNgheNghiep();
            isValid &= ValidateDanToc();
            isValid &= ValidateSoDienThoai();
            isValid &= ValidateDiaChi();
            isValid &= ValidateTinhThanh();
            isValid &= ValidatePhuongXa();
            isValid &= ValidateEmail();

            return isValid;
        }
        #endregion

        #region Helper Methods
        private void ShowFieldError(Label errorLabel, string message)
        {
            errorLabel.Text = message;
            errorLabel.IsVisible = true;
            errorLabel.TextColor = Color.FromArgb("#DC2626");
        }

        private void HideFieldError(Label errorLabel)
        {
            errorLabel.Text = "";
            errorLabel.IsVisible = false;
        }

        private void HideAllErrorMessages()
        {
            var errorLabels = new[]
            {
                lblErrorHoTen, lblErrorNgaySinh, lblErrorGioiTinh, lblErrorCCCD,
                lblErrorQuocTich, lblErrorNgheNghiep, lblErrorDanToc,
                lblErrorSoDienThoai, lblErrorDiaChi, lblErrorTinhThanh,
                lblErrorPhuongXa, lblErrorEmail
            };

            foreach (var label in errorLabels)
            {
                HideFieldError(label);
            }
        }

        private void ShowErrorMessage(string message)
        {
            frameError.IsVisible = true;
            lblErrorMessage.Text = message;

            // Tự động ẩn sau 5 giây
            Device.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                frameError.IsVisible = false;
                return false;
            });
        }
        #endregion

        #region Event Handlers
        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Microsoft.Maui.Controls.Entry;
            if (entry == null) return;

            // Validate real-time cho các trường
            if (entry == entryHoTen) ValidateHoTen();
            else if (entry == entryCCCD) ValidateCCCD();
            else if (entry == entrySoDienThoai) ValidateSoDienThoai();
            else if (entry == entryDiaChi) ValidateDiaChi();
            else if (entry == entryEmail) ValidateEmail();
        }

        private void OnEntryUnfocused(object sender, FocusEventArgs e)
        {
            var entry = sender as Microsoft.Maui.Controls.Entry;
            if (entry == null) return;

            // Validate khi mất focus
            if (entry == entryHoTen) ValidateHoTen();
            else if (entry == entryCCCD) ValidateCCCD();
            else if (entry == entrySoDienThoai) ValidateSoDienThoai();
            else if (entry == entryDiaChi) ValidateDiaChi();
            else if (entry == entryEmail) ValidateEmail();
        }

        private void OnPickerSelectionChanged(object sender, EventArgs e)
        {
            var picker = sender as Picker;
            if (picker == null) return;

            // Validate khi chọn giá trị
            if (picker == pickerGioiTinh) ValidateGioiTinh();
            else if (picker == pickerQuocTich) ValidateQuocTich();
            else if (picker == pickerTinhThanh)
            {
                ValidateTinhThanh();
                OnTinhThanhSelected(sender, e); // Load quận/huyện và xã/phường
            }
            else if (picker == pickerPhuongXa) ValidatePhuongXa();
            else if (picker == pickerDanToc) ValidateDanToc();
            else if (picker == pickerNgheNghiep) ValidateNgheNghiep();
        }

        private async void OnTiepTucClicked(object sender, EventArgs e)
        {
            try
            {
                // Ẩn bàn phím nếu đang mở
                entryHoTen.Unfocus();

                // Validate tất cả các trường
                if (!ValidateAllFields())
                {
                    await DisplayAlert("Thông báo", "Vui lòng điền đầy đủ thông tin bắt buộc", "OK");
                    return;
                }

                var gioiTinhItem = pickerGioiTinh.SelectedItem as PickerItem;
                var quocTichItem = pickerQuocTich.SelectedItem as PickerItem;
                var danTocItem = pickerDanToc.SelectedItem as PickerItem;
                var tinhThanhItem = pickerTinhThanh.SelectedItem as PickerItem;
                var phuongXaItem = pickerPhuongXa.SelectedItem as PickerItem;
                var ngheNghiepItem = pickerNgheNghiep.SelectedItem as PickerItem;

                var patientInfo = new PatientInfoRequest
                {
                    UserId = _userId,
                    HoTen = entryHoTen.Text?.Trim(),
                    NgaySinh = selectedDate.Value.ToString("yyyy-MM-dd"),
                    GioiTinhId = gioiTinhItem?.Id ?? 0,
                    CCCD = entryCCCD.Text?.Trim(),
                    QuocTichId = quocTichItem?.Id ?? 0,
                    NgheNghiepId = ngheNghiepItem?.Id ?? 0,
                    DanTocId = danTocItem?.Id ?? 0,
                    SoDienThoai = entrySoDienThoai.Text?.Trim(),
                    DiaChi = entryDiaChi.Text?.Trim(),
                    TinhThanhId = tinhThanhItem?.Id ?? 0,
                    PhuongXaId = phuongXaItem?.Id ?? 0,
                    Email = entryEmail.Text?.Trim() ?? ""
                };

                // Validate các ID
                if (patientInfo.GioiTinhId == 0 ||
                    patientInfo.QuocTichId == 0 ||
                    patientInfo.DanTocId == 0 ||
                    patientInfo.TinhThanhId == 0 ||
                    patientInfo.PhuongXaId == 0 ||
                    patientInfo.NgheNghiepId == 0)
                {
                    await DisplayAlert("Lỗi", "Vui lòng chọn đầy đủ các thông tin từ danh sách", "OK");
                    return;
                }

                // Hiển thị loading overlay
                ShowLoadingOverlay(true, "Đang lưu thông tin...", "Vui lòng đợi trong giây lát");

                // Gửi dữ liệu lên server
                var result = await _patientInfoService.SavePatientInfoAsync(patientInfo, _flag);

                if (result.Success)
                {
                    // Đợi thêm 500ms để người dùng thấy loading
                    await Task.Delay(500);

                    ShowLoadingOverlay(false);

                    await DisplayAlert("Thành công", "Thông tin đã được lưu thành công!", "OK");

                    if (_flag == "LuuHoSo")
                        await Navigation.PushAsync(new S0306_mChonCoSoKham());
                    else
                        await Navigation.PushAsync(new S0306_mHoSoBenhNhan(parentPage: this));

                    // xóa các trang trước đó khỏi navigation stack
                    var existingPages = Navigation.NavigationStack.ToList();
                    for (int i = existingPages.Count - 2; i >= 0; i--)
                    {
                        Navigation.RemovePage(existingPages[i]);
                    }
                }
                else
                {
                    ShowLoadingOverlay(false);
                    await DisplayAlert("Lỗi", result.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                ShowLoadingOverlay(false);
                await DisplayAlert("Lỗi", $"Đã xảy ra lỗi: {ex.Message}", "OK");
            }
        }

        private async void OnDieuKhoanTapped(object sender, EventArgs e)
        {
            // Hiển thị điều khoản sử dụng
            await DisplayAlert("Điều khoản sử dụng",
                "Nội dung điều khoản sử dụng sẽ được hiển thị tại đây...", "Đã hiểu");
        }
        #endregion

        #region Date Picker Methods
        private async void OnChonNgaySinhClicked(object sender, EventArgs e)
        {
            try
            {
                // Ẩn bàn phím nếu đang mở
                entryNgaySinh.Unfocus();

                // Hiển thị dialog chọn ngày
                await ShowDatePickerPopup();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Lỗi khi mở lịch: {ex.Message}");
            }
        }

        private async Task ShowDatePickerPopup()
        {
            // Ngày hiện tại hoặc ngày đã chọn trước đó
            DateTime initialDate = selectedDate ?? DateTime.Today.AddYears(-18);

            // Tạo DatePicker với các thiết lập
            var datePicker = new Microsoft.Maui.Controls.DatePicker
            {
                Date = initialDate,
                MaximumDate = DateTime.Today,
                MinimumDate = new DateTime(1900, 1, 1),
                FontSize = 16,
                TextColor = Color.FromArgb("#1E293B"),
                HorizontalOptions = LayoutOptions.Center
            };

            // Tạo button OK
            var okButton = new Button
            {
                Text = "CHỌN",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                BackgroundColor = Color.FromArgb("#007bff"),
                CornerRadius = 10,
                HeightRequest = 45,
                WidthRequest = 120,
                Margin = new Thickness(0, 10, 5, 0)
            };

            // Tạo button Hủy
            var cancelButton = new Button
            {
                Text = "HỦY",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                BackgroundColor = Color.FromArgb("#6c757d"),
                CornerRadius = 10,
                HeightRequest = 45,
                WidthRequest = 120,
                Margin = new Thickness(5, 10, 0, 0)
            };

            // Tạo layout cho popup
            var stackLayout = new VerticalStackLayout
            {
                Spacing = 0,
                Padding = 20,
                BackgroundColor = Colors.White,
                MinimumWidthRequest = 300
            };

            // Tiêu đề
            var titleLabel = new Label
            {
                Text = "CHỌN NGÀY SINH",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1E293B"),
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 15)
            };

            // Layout cho các nút
            var buttonStack = new HorizontalStackLayout
            {
                Spacing = 10,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            // Thêm các control vào layout
            stackLayout.Children.Add(titleLabel);
            stackLayout.Children.Add(datePicker);
            stackLayout.Children.Add(buttonStack);

            buttonStack.Children.Add(cancelButton);
            buttonStack.Children.Add(okButton);

            // Tạo Frame để có border và shadow
            var frame = new Frame
            {
                Content = stackLayout,
                CornerRadius = 15,
                BorderColor = Color.FromArgb("#007bff"),
                BackgroundColor = Colors.White,
                HasShadow = true,
                Padding = 0,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            // Tạo AbsoluteLayout để làm nền mờ
            var absoluteLayout = new AbsoluteLayout();

            // Tạo nền mờ
            var backgroundBox = new BoxView
            {
                BackgroundColor = Color.FromArgb("#80000000"),
                Opacity = 0.7
            };

            AbsoluteLayout.SetLayoutFlags(backgroundBox, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(backgroundBox, new Rect(0, 0, 1, 1));

            // Thêm Frame vào giữa
            AbsoluteLayout.SetLayoutFlags(frame, AbsoluteLayoutFlags.PositionProportional);
            AbsoluteLayout.SetLayoutBounds(frame, new Rect(0.5, 0.5, 320, AbsoluteLayout.AutoSize));

            absoluteLayout.Children.Add(backgroundBox);
            absoluteLayout.Children.Add(frame);

            // Tạo trang modal
            var modalPage = new ContentPage
            {
                Content = absoluteLayout,
                BackgroundColor = Colors.Transparent
            };

            // Task completion source để chờ kết quả
            var tcs = new TaskCompletionSource<DateTime?>();

            // Xử lý sự kiện nút OK
            okButton.Clicked += async (s, e) =>
            {
                selectedDate = datePicker.Date;
                await Navigation.PopModalAsync();
                tcs.SetResult(selectedDate);
            };

            // Xử lý sự kiện nút Hủy
            cancelButton.Clicked += async (s, e) =>
            {
                await Navigation.PopModalAsync();
                tcs.SetResult(null);
            };

            // Xử lý tap trên nền mờ để đóng popup
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) =>
            {
                await Navigation.PopModalAsync();
                tcs.SetResult(null);
            };
            backgroundBox.GestureRecognizers.Add(tapGesture);

            try
            {
                // Hiển thị popup
                await Navigation.PushModalAsync(modalPage, true);

                // Chờ kết quả
                var result = await tcs.Task;

                if (result.HasValue)
                {
                    // Cập nhật Entry với ngày đã chọn
                    entryNgaySinh.Text = result.Value.ToString("dd/MM/yyyy");
                    selectedDate = result.Value;
                    ValidateNgaySinh();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi hiển thị date picker: {ex.Message}");
                await Navigation.PopModalAsync();
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
        #endregion
    }
}