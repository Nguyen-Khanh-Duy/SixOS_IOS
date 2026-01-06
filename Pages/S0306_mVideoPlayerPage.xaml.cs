using System;
using Microsoft.Maui.Controls;

namespace SixOSDatKhamAppMobile
{
    public partial class S0306_mVideoPlayerPage : ContentPage
    {
        public S0306_mVideoPlayerPage()
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

                // Create HTML video player
                var videoUrl = "https://nguyen-khanh-duy.github.io/videohuongdandatlich/videohuongdandatlichkham.mp4";
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
                LoadingIndicator.IsVisible = false;
            }
            catch
            {
                LoadingIndicator.IsVisible = false;
                ErrorMessage.IsVisible = true;
            }
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            // You can't stop WebView video programmatically easily
            Navigation.PopAsync();
        }
    }
}