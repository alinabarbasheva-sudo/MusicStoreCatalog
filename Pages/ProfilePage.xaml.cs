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
                }
                else if (user is Consultant consultant)
                {
                    RoleText.Text = "Консультант";
                    SpecializationText.Text = consultant.Specialization ?? "Не указана";
                }

                // ВРЕМЕННО - не загружаем график
                ScheduleText.Text = "График работы";

                // ЗАКОММЕНТИРУЙТЕ эту строку:
                // LoadSchedule(user.ID);
            }
            else
            {
                MessageBox.Show("Пользователь не найден");
            }
        }

        private void LoadSchedule(int userId)
        {
            try
            {
                using var context = new AppDbContext();

                // Прямой SQL-запрос для проверки таблицы
                var tableExists = context.Database.ExecuteSqlRaw(
                    "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='Schedules'");

                if (tableExists == 0)
                {
                    ScheduleText.Text = "График не настроен";
                    return;
                }

                // Ищем расписание для пользователя
                var schedule = context.Schedules
                    .FirstOrDefault(s => s.UserId == userId && s.IsWorking);

                if (schedule != null)
                {
                    string day = GetDayOfWeekRussian(schedule.DayOfWeek);
                    ScheduleText.Text = $"{day}, {schedule.StartTime:hh\\:mm} - {schedule.EndTime:hh\\:mm}";
                }
                else
                {
                    ScheduleText.Text = "График не установлен";
                }
            }
            catch (Exception ex)
            {
                // Если ошибка - просто показываем стандартный текст
                ScheduleText.Text = "График работы";
                Console.WriteLine($"Информация: таблица Schedules не создана или пуста: {ex.Message}");
            }
        }

        private string GetDayOfWeekRussian(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => "Понедельник",
                DayOfWeek.Tuesday => "Вторник",
                DayOfWeek.Wednesday => "Среда",
                DayOfWeek.Thursday => "Четверг",
                DayOfWeek.Friday => "Пятница",
                DayOfWeek.Saturday => "Суббота",
                DayOfWeek.Sunday => "Воскресенье",
                _ => "Неизвестно"
            };
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция редактирования будет реализована позже",
                           "В разработке",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
        }

        private void ChangePasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            var changePassWindow = new ChangePasswordWindow(_currentUserLogin);
            changePassWindow.Owner = Window.GetWindow(this);
            changePassWindow.ShowDialog();
        }
    }
}