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
        public MainWindow()
        {
            InitializeComponent();
            CatalogBtn.Click += (s, e) => MainFrame.Content = new CatalogPage();
            ProfileBtn.Click += (s, e) => MainFrame.Content = new ProfilePage();
            WelcomeText.Text = $"Добро пожаловать, {UserLogin}!";
        }
    }
}