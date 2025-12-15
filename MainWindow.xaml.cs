using MusicStoreCatalog.Pages;
using MusicStoreCatalog.Pages;
using MusicStoreCatalog.Views;
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

namespace MusicStoreCatalog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string UserLogin { get; set; }
        public string UserRole { get; set; }
        public int UserId { get; set; }
        public string UserSpecialization { get; set; } // Добавляем свойство

        public MainWindow()
        {
            InitializeComponent();

            CatalogBtn.Click += (s, e) =>
            {
                var catalogPage = new CatalogPage();
                catalogPage.SetUserRole(this.UserRole);
                catalogPage.SetUserId(this.UserId);
                catalogPage.SetUserSpecialization(this.UserSpecialization);
                MainFrame.Content = catalogPage;
            };

            ProfileBtn.Click += (s, e) =>
            {
                var profilePage = new ProfilePage();
                profilePage.LoadUserData(this.UserLogin);
                MainFrame.Content = profilePage;
            };

            AdminUsersBtn.Click += (s, e) => MainFrame.Content = new UsersPage();
            AdminOrdersBtn.Click += (s, e) =>
            {
                var ordersPage = new OrdersPage();
                ordersPage.SetCurrentUserId(this.UserId); // Передаем ID текущего пользователя
                MainFrame.Content = ordersPage;
            };
            AdminReportsBtn.Click += (s, e) => MainFrame.Content = new ReportsPage();
            AdminAddInstrumentBtn.Click += (s, e) =>
            {
                var addWindow = new AddInstrumentWindow();
                addWindow.Owner = this;
                if (addWindow.ShowDialog() == true)
                {
                    MessageBox.Show("Инструмент успешно добавлен!", "Успех");
                }
            };
            FunWelcomeText();
        }

        public void FunWelcomeText()
        {
            // Добавляем специализацию в приветствие для консультанта
            string welcomeText = $"Добро пожаловать, {UserLogin} ({UserRole})";

            if (UserRole == "Консультант" && !string.IsNullOrEmpty(UserSpecialization))
            {
                welcomeText += $"\nСпециализация: {UserSpecialization}";
            }

            WelcomeText.Text = welcomeText;

            //видимость определенных кнопок только для админитратора 
            AdminHeader.Visibility = UserRole == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
            AdminUsersBtn.Visibility = UserRole == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
            AdminAddInstrumentBtn.Visibility = UserRole == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
            AdminReportsBtn.Visibility = UserRole == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
            AdminOrdersBtn.Visibility = UserRole == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}