using MusicStoreCatalog.Pages;
using MusicStoreCatalog.Views;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MusicStoreCatalog
{
    public partial class MainWindow : Window
    {
        public string UserLogin { get; set; }
        public string UserRole { get; set; }
        public int UserId { get; set; }
        public string UserSpecialization { get; set; }

        private bool _isLoggingOut = false; // Флаг для предотвращения двойного выхода

        public MainWindow()
        {
            InitializeComponent();

            // Навигация по меню
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
                ordersPage.SetCurrentUserId(this.UserId);
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

            // Кнопка выхода - ОДИН обработчик
            LogoutButton.Click += LogoutButton_Click;

            FunWelcomeText();
        }

        // ===== ОБРАБОТЧИК ВЫХОДА ИЗ АККАУНТА =====
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Отписываемся от события временно
            LogoutButton.Click -= LogoutButton_Click;

            try
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите выйти из аккаунта?\nПользователь: {UserLogin}",
                    "Подтверждение выхода",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Создаем новое окно входа
                    var loginWindow = new LoginWindow();
                    loginWindow.Show();

                    // Закрываем текущее окно
                    this.Close();
                }
            }
            finally
            {
                // Подписываемся обратно
                LogoutButton.Click += LogoutButton_Click;
            }
        }

        public void FunWelcomeText()
        {
            string welcomeText = $"Добро пожаловать, {UserLogin} ({UserRole})";

            if (UserRole == "Консультант" && !string.IsNullOrEmpty(UserSpecialization))
            {
                welcomeText += $"\nСпециализация: {UserSpecialization}";
            }

            WelcomeText.Text = welcomeText;

            AdminHeader.Visibility = UserRole == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
            AdminUsersBtn.Visibility = UserRole == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
            AdminAddInstrumentBtn.Visibility = UserRole == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
            AdminReportsBtn.Visibility = UserRole == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
            AdminOrdersBtn.Visibility = UserRole == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}