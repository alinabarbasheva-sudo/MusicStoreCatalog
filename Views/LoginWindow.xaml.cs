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

            // Проверяем, первый ли запуск
            if (SeedService.IsFirstRun())
            {
                SeedService.CreateFirstAdmin("admin", "admin");
            }

            // Фокус на поле логина при загрузке
            Loaded += (s, e) =>
            {
                UsernameBox.Focus();

            };

            // Обработка нажатия Enter
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
                // ВАЖНО: Используем PasswordBox.Password, а не .Text
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