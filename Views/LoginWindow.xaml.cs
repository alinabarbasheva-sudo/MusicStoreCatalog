using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using MusicStoreCatalog.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicStoreCatalog.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // ВАЖНО: Сначала создаем БД, потом проверяем первый запуск
            InitializeDatabase();

            // Теперь проверяем первый запуск
            CheckFirstRun();

            // Центрируем окно
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Фокус на поле логина
            Loaded += (s, e) => UsernameBox.Focus();

            // Обработка Enter
            UsernameBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                    PasswordBox.Focus();
            };

            PasswordBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                    LoginButton_Click(null, null);
            };
        }

        private void InitializeDatabase()
        {
            try
            {
                Console.WriteLine("=== Инициализация базы данных ===");

                using var context = new AppDbContext();
                context.EnsureDatabaseCreated();

                Console.WriteLine("База данных инициализирована");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации базы данных: {ex.Message}\n\n" +
                              $"Попробуйте удалить файл MusicStore.db и перезапустить приложение.",
                              "Критическая ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);

                // Можно закрыть приложение или продолжить
                // Application.Current.Shutdown();
            }
        }

        private void CheckFirstRun()
        {
            if (SeedService.IsFirstRun())
            {
                // Создаем только админа
                SeedService.CreateFirstAdmin("admin", "admin");

                MessageBox.Show("Система запущена впервые.\n" +
                              "Создан администратор по умолчанию:\n" +
                              "Логин: admin\n" +
                              "Пароль: admin\n\n" +
                              "Рекомендуется сменить пароль после входа.",
                              "Первоначальная настройка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            User user = null;
            MainWindow mainWindow = null;
            string UserRole = "";
            string UserSpecialization = "";

            using (var context = new AppDbContext())
            {
                user = context.Users.FirstOrDefault(u => u.Login == UsernameBox.Text);
            }

            if (user != null)
            {
                if (string.IsNullOrEmpty(PasswordBox.Password))
                {
                    MessageBox.Show("Введите пароль");
                    PasswordBox.Focus();
                    return;
                }

                if (BCrypt.Net.BCrypt.Verify(PasswordBox.Password, user.PasswordHash))
                {
                    if (user is Admin)
                    {
                        UserRole = "Администратор";
                        UserSpecialization = "";
                    }
                    else if (user is Consultant consultant)
                    {
                        UserRole = "Консультант";
                        UserSpecialization = consultant.Specialization;
                    }

                    mainWindow = new MainWindow();
                    mainWindow.UserLogin = UsernameBox.Text;
                    mainWindow.UserRole = UserRole;
                    mainWindow.UserId = user.ID;
                    mainWindow.UserSpecialization = UserSpecialization;
                    mainWindow.FunWelcomeText();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный пароль");
                    PasswordBox.Focus();
                    PasswordBox.SelectAll();
                }
            }
            else
            {
                MessageBox.Show("Пользователь не найден");
                UsernameBox.Focus();
                UsernameBox.SelectAll();
            }
        }
    }
}