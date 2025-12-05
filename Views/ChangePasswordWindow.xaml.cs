using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using System.Linq;
using System.Windows;

namespace MusicStoreCatalog.Views
{
    public partial class ChangePasswordWindow : Window
    {
        private string _userLogin;

        public ChangePasswordWindow(string userLogin)
        {
            InitializeComponent();
            _userLogin = userLogin;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewPasswordBox.Password))
            {
                MessageBox.Show("Введите новый пароль");
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают");
                return;
            }

            if (NewPasswordBox.Password.Length < 4)
            {
                MessageBox.Show("Пароль должен содержать минимум 4 символа");
                return;
            }

            using var context = new AppDbContext();
            var user = context.Users.FirstOrDefault(u => u.Login == _userLogin);

            if (user != null)
            {
                // Проверяем текущий пароль (если введен)
                if (!string.IsNullOrWhiteSpace(CurrentPasswordBox.Password))
                {
                    if (!BCrypt.Net.BCrypt.Verify(CurrentPasswordBox.Password, user.PasswordHash))
                    {
                        MessageBox.Show("Неверный текущий пароль");
                        return;
                    }
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPasswordBox.Password);
                context.SaveChanges();

                MessageBox.Show("Пароль успешно изменен", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Пользователь не найден");
            }
        }
    }
}