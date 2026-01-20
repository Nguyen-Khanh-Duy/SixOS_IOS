using Camera.MAUI;
using Camera.MAUI.ZXingHelper;
using System.Linq;
using System.Threading.Tasks;
//using ZXing.Net.Maui.Controls;
using ZXing.Net.Maui;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mQRScannerPage : ContentPage
    {
        private TaskCompletionSource<string> _scanCompletionSource;
        private bool _isScanning = true;
        private bool _isProcessing = false;
        private bool _isFlashOn = false;

        public S0306_mQRScannerPage()
        {
            InitializeComponent();
        }

        public Task<string> WaitForScanResultAsync()
        {
            _scanCompletionSource = new TaskCompletionSource<string>();
            return _scanCompletionSource.Task;
        }
      
        protected override async void OnAppearing()
        {
            base.OnAppearing();

#if ANDROID
            var statusBarHeight = DeviceDisplay.MainDisplayInfo.Density > 0
                ? DeviceDisplay.MainDisplayInfo.Density * 14
                : 24;

            Padding = new Thickness(0, statusBarHeight + 2, 0, 42);
#endif
            _isScanning = true;
            _isProcessing = false;
            scanLine.BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#FF4081");
            scanLine.HeightRequest = 3;
            loadingFrame.IsVisible = false;

            try
            {
                // Yêu cầu quyền truy cập camera
                var hasPermission = await CheckAndRequestCameraPermission();
                if (!hasPermission)
                {
                    await DisplayAlert("Cần quyền truy cập",
                        "Ứng dụng cần quyền truy cập camera để quét mã QR.", "OK");
                    await Navigation.PopAsync();
                    return;
                }

                // Cấu hình camera cho quét QR
                await ConfigureCamera();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Camera error: {ex.Message}");
                await DisplayAlert("Lỗi", "Không thể khởi động camera.", "OK");
            }
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            _isScanning = false;

            try
            {
                await cameraView.StopCameraAsync();
            }
            catch { }
        }

        private async Task<bool> CheckAndRequestCameraPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (status != PermissionStatus.Granted)
            {
                // Hiển thị dialog thân thiện
                var result = await DisplayAlert("Cần quyền truy cập camera",
                    "Ứng dụng cần quyền truy cập camera để quét mã QR trên CCCD.\n\nBạn có cho phép sử dụng camera không?",
                    "Cho phép", "Từ chối");

                if (result)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                }
                else
                {
                    return false;
                }
            }

            return status == PermissionStatus.Granted;
        }

        private async Task ConfigureCamera()
        {
            try
            {
                // Đợi camera load
                if (cameraView.Cameras == null || cameraView.Cameras.Count == 0)
                {
                    await Task.Delay(500);
                }

                if (cameraView.Cameras.Count > 0)
                {
                    // Tìm camera sau
                    CameraInfo rearCamera = null;
                    foreach (var cam in cameraView.Cameras)
                    {
                        // Kiểm tra theo Position property (nếu có)
                        if (cam.ToString().ToLower().Contains("back") ||
                            cam.ToString().ToLower().Contains("rear") ||
                            (cam.GetType().GetProperty("Position")?.GetValue(cam)?.ToString()?.ToLower().Contains("back") == true))
                        {
                            rearCamera = cam;
                            break;
                        }
                    }

                    if (rearCamera != null)
                    {
                        cameraView.Camera = rearCamera;
                    }
                    else
                    {
                        cameraView.Camera = cameraView.Cameras.FirstOrDefault();
                    }

                    // Cấu hình quét QR
                    cameraView.BarCodeDetectionEnabled = true;

                    cameraView.BarCodeOptions = new BarcodeDecodeOptions
                    {
                        AutoRotate = true,
                        TryHarder = true,
                        TryInverted = true
                    };

                    // Khởi động camera
                    await Task.Delay(300);
                    var result = await cameraView.StartCameraAsync();

                    if (result != CameraResult.Success)
                    {
                        await DisplayAlert("Lỗi", $"Không thể khởi động camera: {result}", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Lỗi", "Không tìm thấy camera trên thiết bị", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConfigureCamera error: {ex.Message}");
                throw;
            }
        }

        private async void cameraView_CamerasLoaded(object sender, EventArgs e)
        {
            try
            {
                if (_isScanning && !_isProcessing)
                {
                    await Task.Delay(300);
                    await ConfigureCamera();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CamerasLoaded error: {ex.Message}");
            }
        }

        private async void cameraView_BarcodeDetected(object sender, BarcodeEventArgs args)
        {
            try
            {
                // Kiểm tra điều kiện quét
                if (!_isScanning || _isProcessing || args?.Result == null || !args.Result.Any())
                    return;

                var qrData = args.Result[0].Text;

                // Validate dữ liệu QR
                if (string.IsNullOrWhiteSpace(qrData))
                    return;

                // Kiểm tra định dạng CCCD (ít nhất 6 phần)
                var parts = qrData.Split('|');
                if (parts.Length < 6)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        // Tạm dừng quét
                        cameraView.BarCodeDetectionEnabled = false;

                        await DisplayAlert("Mã QR không hợp lệ",
                            "Mã QR không đúng định dạng CCCD. Vui lòng quét mã QR trên thẻ căn cước công dân.\n\nĐịnh dạng đúng: Số CCCD||Họ tên|Ngày sinh|Giới tính|Địa chỉ|Ngày cấp",
                            "OK");

                        // Tiếp tục quét
                        cameraView.BarCodeDetectionEnabled = true;
                    });
                    return;
                }

                // Bắt đầu xử lý
                _isProcessing = true;
                _isScanning = false;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        // Hiển thị loading
                        loadingFrame.IsVisible = true;

                        // Hiệu ứng quét thành công
                        scanLine.BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#4CAF50");
                        scanLine.HeightRequest = 5;

                        await Task.Delay(500);

                        // Dừng camera
                        await cameraView.StopCameraAsync();

                        // Trả kết quả về page trước
                        _scanCompletionSource?.TrySetResult(qrData);

                        // Đóng trang quét
                        await Navigation.PopAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Processing error: {ex.Message}");

                        // Reset trạng thái
                        _isScanning = true;
                        _isProcessing = false;
                        loadingFrame.IsVisible = false;
                        scanLine.BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#FF4081");
                        scanLine.HeightRequest = 3;

                        // Khởi động lại camera
                        await cameraView.StartCameraAsync();

                        await DisplayAlert("Lỗi", "Có lỗi khi xử lý mã QR. Vui lòng thử lại.", "OK");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BarcodeDetected error: {ex.Message}");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    _isScanning = true;
                    _isProcessing = false;
                    loadingFrame.IsVisible = false;

                    // Khởi động lại camera nếu cần
                    try
                    {
                        await cameraView.StartCameraAsync();
                    }
                    catch { }
                });
            }
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            _isScanning = false;

            try
            {
                await cameraView.StopCameraAsync();
            }
            catch { }

            _scanCompletionSource?.TrySetResult(null);
            await Navigation.PopAsync();
        }

        private async void OnFlashClicked(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra camera đã được khởi động chưa
                if (cameraView.Camera == null)
                {
                    await DisplayAlert("Thông báo", "Camera chưa sẵn sàng", "OK");
                    return;
                }

                _isFlashOn = !_isFlashOn;

                // Thử cách trực tiếp với Camera.MAUI
                try
                {
                    // Camera.MAUI có thể có property TorchEnabled hoặc FlashMode
                    var torchProp = cameraView.GetType().GetProperty("TorchEnabled");
                    if (torchProp != null && torchProp.CanWrite)
                    {
                        torchProp.SetValue(cameraView, _isFlashOn);
                    }
                    else
                    {
                        // Thử với FlashMode
                        var flashProperty = cameraView.GetType().GetProperty("FlashMode");
                        if (flashProperty != null && flashProperty.CanWrite)
                        {
                            var flashModeType = flashProperty.PropertyType;
                            if (flashModeType.IsEnum)
                            {
                                // Thử tìm giá trị Torch hoặc On
                                object flashValue = null;
                                try
                                {
                                    flashValue = Enum.Parse(flashModeType, _isFlashOn ? "Torch" : "Off");
                                }
                                catch
                                {
                                    try
                                    {
                                        flashValue = Enum.Parse(flashModeType, _isFlashOn ? "On" : "Off");
                                    }
                                    catch { }
                                }

                                if (flashValue != null)
                                {
                                    flashProperty.SetValue(cameraView, flashValue);
                                }
                            }
                        }
                    }

                    // Cập nhật UI
                    btnFlash.TextColor = _isFlashOn ? Colors.Yellow : Colors.White;
                    btnFlash.BackgroundColor = _isFlashOn ?
                        Microsoft.Maui.Graphics.Color.FromArgb("#80FFD700") :
                        Microsoft.Maui.Graphics.Color.FromArgb("#40000000");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Flash toggle error: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Flash error: {ex.Message}");

                // Reset trạng thái
                _isFlashOn = !_isFlashOn;

                await DisplayAlert("Thông báo",
                    "Thiết bị của bạn không hỗ trợ đèn flash hoặc tính năng này tạm thời không khả dụng.",
                    "OK");
            }
        }

    }

}
