

namespace SixOSDatKhamAppMobile
{
    public partial class S0306_mVideoTraCuu : ContentPage
    {
        public S0306_mVideoTraCuu()
        {
            InitializeComponent();
            LoadVideo();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

#if ANDROID
            var statusBarHeight = DeviceDisplay.MainDisplayInfo.Density > 0
                ? DeviceDisplay.MainDisplayInfo.Density * 14
                : 24;

            Padding = new Thickness(0, statusBarHeight + 2, 0, 42);
#endif
        }
        private void LoadVideo()
        {
            try
            {
                LoadingIndicator.IsVisible = true;

                // URL video tra c?u l?ch h?n (c?n thay ð?i URL th?c t?)
                var videoUrl = "https://nguyen-khanh-duy.github.io/tracuu/videotracuu.mp4";


                var htmlSource = new HtmlWebViewSource
                {
                    Html = @$"
                    <!DOCTYPE html>
                    <html>
                    <body style='margin:0;padding:0;background-color:black;'>
                        <video width='100%' height='100%' controls autoplay>
                            <source src='{videoUrl}' type='video/mp4'>
                            Your browser does not support the video tag.
                        </video>
                    </body>
                    </html>"
                };

                VideoPlayer.Source = htmlSource;

                // ?n indicator sau 2 giây
                Device.StartTimer(TimeSpan.FromSeconds(2), () =>
                {
                    LoadingIndicator.IsVisible = false;
                    return false;
                });
            }
            catch (Exception ex)
            {
                LoadingIndicator.IsVisible = false;
                ErrorMessage.Text = $"Không th? t?i video: {ex.Message}";
                ErrorMessage.IsVisible = true;
            }
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            // D?ng video trý?c khi ðóng trang
            try
            {
                VideoPlayer.Source = null; // Xóa ngu?n video ð? d?ng phát
            }
            catch { }

            Navigation.PopAsync();
        }

        // X? l? khi trang ðóng
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Ð?m b?o d?ng video khi r?i trang
            VideoPlayer.Source = null;
        }
    }
}
