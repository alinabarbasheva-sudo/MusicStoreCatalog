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
        private int _currentUserId;

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
                _currentUserId = user.ID;

                // Основная информация
                LoginText.Text = user.Login;
                FirstNameText.Text = user.FirstName;
                LastNameText.Text = user.LastName;
                PhoneText.Text = user.PhoneNumber;

                // Определяем роль и настраиваем отображение
                if (user is Admin)
                {
                    RoleText.Text = "Администратор";
                    _currentUserRole = "Администратор";
                    // У администратора нет специализации
                    ClearSpecializationRow();
                }
                else if (user is Consultant consultant)
                {
                    RoleText.Text = "Консультант";
                    _currentUserRole = "Консультант";
                    // Добавляем строку специализации для консультанта
                    AddSpecializationRow(consultant.Specialization ?? "Не указана");
                }

                // Управляем видимостью кнопок
                UpdateButtonVisibility();
            }
            else
            {
                MessageBox.Show("Пользователь не найден", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод для добавления строки специализации
        private void AddSpecializationRow(string specialization)
        {
            // Очищаем предыдущую строку специализации, если была
            ClearSpecializationRow();

            // Добавляем новую строку в сетку
            WorkInfoGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            // Заголовок "Специализация:"
            var specializationLabel = new TextBlock
            {
                Text = "Специализация:",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 10, 5),
                Name = "SpecializationLabel" // Задаем имя для доступа
            };
            Grid.SetRow(specializationLabel, 1);
            Grid.SetColumn(specializationLabel, 0);
            WorkInfoGrid.Children.Add(specializationLabel);

            // Значение специализации
            var specializationText = new TextBlock
            {
                Text = specialization,
                Margin = new Thickness(0, 0, 0, 5),
                Name = "SpecializationText" // Задаем имя для доступа
            };
            Grid.SetRow(specializationText, 1);
            Grid.SetColumn(specializationText, 1);
            WorkInfoGrid.Children.Add(specializationText);
        }

        // Метод для очистки строки специализации
        private void ClearSpecializationRow()
        {
            // Удаляем все элементы специализации
            var elementsToRemove = WorkInfoGrid.Children
                .OfType<FrameworkElement>()
                .Where(e => e.Name == "SpecializationLabel" || e.Name == "SpecializationText")
                .ToList();

            foreach (var element in elementsToRemove)
            {
                WorkInfoGrid.Children.Remove(element);
            }

            // Убираем лишние строки (оставляем только первую)
            while (WorkInfoGrid.RowDefinitions.Count > 1)
            {
                WorkInfoGrid.RowDefinitions.RemoveAt(1);
            }
        }

        private void UpdateButtonVisibility()
        {
            if (_currentUserRole == "Администратор")
            {
                EditButton.Visibility = Visibility.Visible;
                EditButton.IsEnabled = true;
                ChangePasswordButton.Visibility = Visibility.Visible;
                ChangePasswordButton.IsEnabled = true;
            }
            else
            {
                EditButton.Visibility = Visibility.Collapsed;
                EditButton.IsEnabled = false;
                ChangePasswordButton.Visibility = Visibility.Collapsed;
                ChangePasswordButton.IsEnabled = false;
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUserRole != "Администратор")
            {
                MessageBox.Show("Только администратор может редактировать профиль",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var editWindow = new EditProfileWindow(_currentUserId);
            editWindow.Owner = Window.GetWindow(this);

            editWindow.ProfileUpdated += (s, args) =>
            {
                LoadUserData(_currentUserLogin);

                MessageBox.Show("Профиль успешно обновлен!",
                              "Успех",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            };

            editWindow.ShowDialog();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUserRole != "Администратор")
            {
                MessageBox.Show("Только администратор может менять пароль",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var changePassWindow = new ChangePasswordWindow(_currentUserLogin);
            changePassWindow.Owner = Window.GetWindow(this);
            changePassWindow.ShowDialog();
        }
    }
}