using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using MusicStoreCatalog.Views;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MusicStoreCatalog.Pages
{
    public partial class UsersPage : UserControl
    {
        public UsersPage()
        {
            InitializeComponent();
            LoadUsers();
            AddUserBtn.Click += AddUserBtn_Click;
        }

        private void LoadUsers()
        {
            try
            {
                using var context = new AppDbContext();
                var consultants = context.Users
                    .OfType<Consultant>()
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToList();

                UsersGrid.ItemsSource = consultants;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка");
            }
        }

        private void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddConsultantWindow();
            addWindow.Owner = Window.GetWindow(this);
            addWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            addWindow.UserAdded += (s, args) =>
            {
                LoadUsers();
            };

            addWindow.ShowDialog();
        }

        // ===== НОВЫЙ МЕТОД: Редактирование консультанта =====
        private void EditUserBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null && int.TryParse(button.Tag.ToString(), out int userId))
            {
                EditConsultant(userId);
            }
        }

        private void EditConsultant(int userId)
        {
            var editWindow = new EditConsultantWindow(userId);
            editWindow.Owner = Window.GetWindow(this);
            editWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            editWindow.ConsultantUpdated += (s, args) =>
            {
                LoadUsers();
            };

            editWindow.ShowDialog();
        }

        public void DeleteUserBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null && int.TryParse(button.Tag.ToString(), out int userId))
            {
                DeleteConsultant(userId);
            }
        }

        private void DeleteConsultant(int userId)
        {
            try
            {
                var result = MessageBox.Show(
                    "Вы уверены, что хотите удалить этого консультанта?\n" +
                    "Все связанные заявки также будут удалены.",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                using var context = new AppDbContext();
                var consultant = context.Users.OfType<Consultant>().FirstOrDefault(c => c.ID == userId);

                if (consultant == null)
                {
                    MessageBox.Show("Консультант не найден", "Ошибка");
                    return;
                }

                string consultantInfo = $"{consultant.FirstName} {consultant.LastName} ({consultant.Login})";

                // Удаляем связанные заявки
                var orders = context.OrderRequests.Where(o => o.RequestedById == userId).ToList();
                context.OrderRequests.RemoveRange(orders);


                // Удаляем консультанта
                context.Users.Remove(consultant);
                context.SaveChanges();

                MessageBox.Show($"Консультант {consultantInfo} успешно удален",
                              "Успех",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка");
            }
        }
    }
}