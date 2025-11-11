using MusicStoreCatalog.Pages;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MusicStoreCatalog.Pages;

namespace MusicStoreCatalog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string UserLogin { get; set; }
        public string UserRole { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();
            CatalogBtn.Click += (s, e) =>
            {
                var catalogPage = new CatalogPage();
                catalogPage.SetUserRole(this.UserRole);
                MainFrame.Content = catalogPage;
            };
            ProfileBtn.Click += (s, e) => MainFrame.Content = new ProfilePage();
            FunWelcomeText();
        }

        public void FunWelcomeText()
        {
            WelcomeText.Text = $"Добро пожаловать, {UserLogin} ({UserRole})";

            //видимость оперделенных кнопок только для админитратора 
            AdminHeader.Visibility = UserRole == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
            AdminUsersBtn.Visibility = UserRole == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
            AdminAddInstrumentBtn.Visibility = UserRole == "Консультант" ? Visibility.Visible : Visibility.Collapsed;
            AdminReportsBtn.Visibility = UserRole == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
        }



    }
}