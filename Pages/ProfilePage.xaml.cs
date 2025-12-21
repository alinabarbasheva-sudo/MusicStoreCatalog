using Microsoft.EntityFrameworkCore;
using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using MusicStoreCatalog.Views;
using MusicStoreCatalog.Utilities;
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
                LoadBasicInfo(user);
                DetermineUserRole(context, user);
                UpdateButtonVisibility();
            }
            else
            {
                MessageHelper.ShowError("Пользователь не найден");
            }
        }

        private void LoadBasicInfo(User user)
        {
            LoginText.Text = user.Login;
            FirstNameText.Text = user.FirstName;
            LastNameText.Text = user.LastName;
            PhoneText.Text = user.PhoneNumber;
        }

        private void DetermineUserRole(AppDbContext context, User user)
        {
            if (user is Admin admin)
            {
                _currentUserRole = "Администратор";
                RoleText.Text = "Администратор";
                SalesStatsBorder.Visibility = Visibility.Collapsed;
            }
            else if (user is Consultant consultant)
            {
                _currentConsultant = consultant;
                _currentUserRole = "Консультант";
                RoleText.Text = "Консультант";
                LoadConsultantInfo(consultant);
                LoadSalesStatistics(context, consultant);
                SalesStatsBorder.Visibility = Visibility.Visible;
            }
        }

        private void LoadConsultantInfo(Consultant consultant)
        {
            AddSpecializationRow(consultant.Specialization ?? "Не указана");
        }

        private void AddSpecializationRow(string specialization)
        {
            ClearSpecializationRow();

            WorkInfoGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            var specializationLabel = new TextBlock
            {
                Text = "Специализация:",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 10, 5)
            };

            var specializationText = new TextBlock
            {
                Text = specialization,
                Margin = new Thickness(0, 0, 0, 5)
            };

            Grid.SetRow(specializationLabel, 1);
            Grid.SetColumn(specializationLabel, 0);
            Grid.SetRow(specializationText, 1);
            Grid.SetColumn(specializationText, 1);

            WorkInfoGrid.Children.Add(specializationLabel);
            WorkInfoGrid.Children.Add(specializationText);
        }

        private void ClearSpecializationRow()
        {
            var elementsToRemove = WorkInfoGrid.Children
                .OfType<FrameworkElement>()
                .Where(e => Grid.GetRow(e) == 1)
                .ToList();

            foreach (var element in elementsToRemove)
            {
                WorkInfoGrid.Children.Remove(element);
            }

            while (WorkInfoGrid.RowDefinitions.Count > 1)
            {
                WorkInfoGrid.RowDefinitions.RemoveAt(1);
            }
        }

        private void LoadSalesStatistics(AppDbContext context, Consultant consultant)
        {
            try
            {
                // Всего продаж
                TotalSalesText.Text = consultant.SalesCount.ToString();

                // Рейтинг
                UpdateRatingStars(consultant.Rating);

                // Место среди всех консультантов
                var allConsultants = context.Users
                    .OfType<Consultant>()
                    .OrderByDescending(c => c.SalesCount)
                    .ToList();

                int rankAll = allConsultants.FindIndex(c => c.ID == consultant.ID) + 1;
                RankAllText.Text = $"{rankAll} из {allConsultants.Count}";

                // Место по специализации
                UpdateSpecializationRank(consultant, allConsultants);
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Ошибка загрузки статистики: {ex.Message}");
            }
        }

        private void UpdateSpecializationRank(Consultant consultant, System.Collections.Generic.List<Consultant> allConsultants)
        {
            if (!string.IsNullOrEmpty(consultant.Specialization))
            {
                var sameSpecializationConsultants = allConsultants
                    .Where(c => c.Specialization == consultant.Specialization)
                    .OrderByDescending(c => c.SalesCount)
                    .ToList();

                if (sameSpecializationConsultants.Any())
                {
                    int rankSpecialization = sameSpecializationConsultants.FindIndex(c => c.ID == consultant.ID) + 1;
                    RankSpecializationText.Text = $"{rankSpecialization} из {sameSpecializationConsultants.Count}";
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

        private void UpdateRatingStars(int rating)
        {
            RatingStarsPanel.Children.Clear();

            // Золотые звезды
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

            // Серые звезды
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
            bool isAdmin = _currentUserRole == "Администратор";

            EditButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            EditButton.IsEnabled = isAdmin;

            ChangePasswordButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            ChangePasswordButton.IsEnabled = isAdmin;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUserRole != "Администратор")
            {
                MessageHelper.ShowWarning("Только администратор может редактировать профиль");
                return;
            }

            var editWindow = new EditProfileWindow(_currentUserId);
            editWindow.Owner = Window.GetWindow(this);
            editWindow.ProfileUpdated += (s, args) =>
            {
                LoadUserData(_currentUserLogin);
                MessageHelper.ShowSuccess("Профиль успешно обновлен!");
            };

            editWindow.ShowDialog();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUserRole != "Администратор")
            {
                MessageHelper.ShowWarning("Только администратор может менять пароль");
                return;
            }

            var changePassWindow = new ChangePasswordWindow(_currentUserLogin);
            changePassWindow.Owner = Window.GetWindow(this);
            changePassWindow.ShowDialog();
        }
    }
}