using SixOSDatKhamAppMobile.Pages;

namespace SixOSDatKhamAppMobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var startPage = new MainPage();

            // Gọi NavigationPage tùy chỉnh tự tắt NavBar
            MainPage = new NoNavBarNavigationPage(startPage);
        }
    }
}
