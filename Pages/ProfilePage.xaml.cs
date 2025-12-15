using Microsoft.EntityFrameworkCore;
using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using MusicStoreCatalog.Views;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MusicStoreCatalog.Pages
{
    public partial class ProfilePage : UserControl
    {
        private string _currentUserLogin;
        private string _currentUserRole;
        private int _currentUserId;
        private Consultant _currentConsultant;

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

                // Определяем роль
                if (user is Admin)
                {
                    RoleText.Text = "Администратор";
                    _currentUserRole = "Администратор";

                    // Скрываем статистику для администратора
                    SalesStatsBorder.Visibility = Visibility.Collapsed;
                }
                else if (user is Consultant consultant)
                {
                    _currentConsultant = consultant;
                    RoleText.Text = "Консультант";
                    _currentUserRole = "Консультант";

                    // Добавляем специализацию
                    AddSpecializationRow(consultant.Specialization ?? "Не указана");

                    // Показываем и загружаем статистику
                    SalesStatsBorder.Visibility = Visibility.Visible;
                    LoadSalesStatistics(context, consultant);
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
                Margin = new Thickness(0, 0, 10, 5)
            };
            Grid.SetRow(specializationLabel, 1);
            Grid.SetColumn(specializationLabel, 0);
            WorkInfoGrid.Children.Add(specializationLabel);

            // Значение специализации
            var specializationText = new TextBlock
            {
                Text = specialization,
                Margin = new Thickness(0, 0, 0, 5)
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

        // ===== НОВЫЙ МЕТОД: Загрузка статистики продаж =====
        private void LoadSalesStatistics(AppDbContext context, Consultant consultant)
        {
            try
            {
                // 1. Всего продаж
                TotalSalesText.Text = consultant.SalesCount.ToString();

                // 2. Расчет рейтинга (звезды)
                int rating = CalculateRating(consultant.SalesCount);
                UpdateRatingStars(rating);

                // 3. Место среди всех консультантов
                var allConsultants = context.Users
                    .OfType<Consultant>()
                    .OrderByDescending(c => c.SalesCount)
                    .ToList();

                int rankAll = allConsultants.FindIndex(c => c.ID == consultant.ID) + 1;
                int totalConsultants = allConsultants.Count;

                RankAllText.Text = $"{rankAll} из {totalConsultants}";

                // 4. Место среди консультантов своей специализации
                if (!string.IsNullOrEmpty(consultant.Specialization))
                {
                    var sameSpecializationConsultants = allConsultants
                        .Where(c => c.Specialization == consultant.Specialization)
                        .OrderByDescending(c => c.SalesCount)
                        .ToList();

                    if (sameSpecializationConsultants.Any())
                    {
                        int rankSpecialization = sameSpecializationConsultants.FindIndex(c => c.ID == consultant.ID) + 1;
                        int totalInSpecialization = sameSpecializationConsultants.Count;

                        RankSpecializationText.Text = $"{rankSpecialization} из {totalInSpecialization}";
                    }
                    else
                    {
                        RankSpecializationText.Text = "1 из 1";
                    }
                }
                else
                {
                    RankSpecializationText.Text = "Специализация не указана";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка");
            }
        }

        // Расчет рейтинга по количеству продаж
        private int CalculateRating(int salesCount)
        {
            return salesCount switch
            {
                >= 50 => 5,
                >= 40 => 4,
                >= 30 => 3,
                >= 20 => 2,
                >= 10 => 1,
                _ => 0
            };
        }

        // Обновление звезд рейтинга
        private void UpdateRatingStars(int rating)
        {
            // Очищаем панель
            RatingStarsPanel.Children.Clear();

            // Добавляем золотые звезды за рейтинг
            for (int i = 0; i < rating; i++)
            {
                var star = new TextBlock
                {
                    Text = "★",
                    Foreground = Brushes.Gold,
                    FontSize = 16,
                    Margin = new Thickness(0, 0, 2, 0)
                };
                RatingStarsPanel.Children.Add(star);
            }

            // Добавляем серые звезды для недостающих
            for (int i = rating; i < 5; i++)
            {
                var star = new TextBlock
                {
                    Text = "☆",
                    Foreground = Brushes.LightGray,
                    FontSize = 16,
                    Margin = new Thickness(0, 0, 2, 0)
                };
                RatingStarsPanel.Children.Add(star);
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