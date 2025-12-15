using Microsoft.EntityFrameworkCore;
using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using MusicStoreCatalog.Views;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MusicStoreCatalog.Pages
{
    public partial class ProfilePage : UserControl
    {
        private string _currentUserLogin;
        private string _currentUserRole;

        public ProfilePage()
        {
            InitializeComponent();
        }

        public void LoadUserData(string login)
        {
            _currentUserLogin = login;

            using var context = new AppDbContext();
            var user = context.Users.FirstOrDefault(u => u.Login == login);

            if (user != null)
            {
                // Основная информация
                LoginText.Text = user.Login;
                FirstNameText.Text = user.FirstName;
                LastNameText.Text = user.LastName;
                PhoneText.Text = user.PhoneNumber;

                // Определяем роль
                if (user is Admin)
                {
                    RoleText.Text = "Администратор";
                    SpecializationText.Text = "Не требуется";
                    _currentUserRole = "Администратор";
                }
                else if (user is Consultant consultant)
                {
                    RoleText.Text = "Консультант";
                    SpecializationText.Text = consultant.Specialization ?? "Не указана";
                    _currentUserRole = "Консультант";
                }



                // Управляем видимостью кнопок в зависимости от роли
                UpdateButtonVisibility();
            }
            else
            {
                MessageBox.Show("Пользователь не найден");
            }
        }

        private void UpdateButtonVisibility()
        {
            if (_currentUserRole == "Консультант")
            {
                // Скрываем кнопки для консультанта
                if (EditButton != null)
                    EditButton.Visibility = Visibility.Collapsed;
                if (ChangePasswordButton != null)
                    ChangePasswordButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Показываем кнопки только для администратора
                if (EditButton != null)
                    EditButton.Visibility = Visibility.Visible;
                if (ChangePasswordButton != null)
                    ChangePasswordButton.Visibility = Visibility.Visible;
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, что это администратор
            if (_currentUserRole != "Администратор")
                return;

            MessageBox.Show("Функция редактирования будет реализована позже",
                           "В разработке",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
        }

        private void ChangePasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, что это администратор
            if (_currentUserRole != "Администратор")
                return;

            var changePassWindow = new ChangePasswordWindow(_currentUserLogin);
            changePassWindow.Owner = Window.GetWindow(this);
            changePassWindow.ShowDialog();
        }
    }
}