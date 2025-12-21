using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using MusicStoreCatalog.Utilities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MusicStoreCatalog.Views
{
    public partial class AddConsultantWindow : Window
    {
        public event EventHandler UserAdded;

        public AddConsultantWindow()
        {
            InitializeComponent();
            SpecializationCombo.SelectedIndex = 0;
            LoginBox.Focus();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Заполните логин и пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                LoginBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(FirstNameBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameBox.Text))
            {
                MessageBox.Show("Заполните имя и фамилию", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                FirstNameBox.Focus();
                return;
            }

            using var context = new AppDbContext();

            if (context.Users.Any(u => u.Login == LoginBox.Text))
            {
                MessageBox.Show("Логин уже занят", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                LoginBox.Focus();
                LoginBox.SelectAll();
                return;
            }

            try
            {
                var consultant = new Consultant
                {
                    Login = LoginBox.Text.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordBox.Password),
                    FirstName = FirstNameBox.Text.Trim(),
                    LastName = LastNameBox.Text.Trim(),
                    PhoneNumber = PhoneBox.Text,
                    Specialization = (SpecializationCombo.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    SalesCount = 0
                };

                context.Users.Add(consultant);
                context.SaveChanges();

                UserAdded?.Invoke(this, EventArgs.Empty);

                MessageBox.Show($"✅ Консультант успешно добавлен!\n\n" +
                              $"Логин: {consultant.Login}\n" +
                              $"Имя: {consultant.FirstName} {consultant.LastName}\n" +
                              $"Специализация: {consultant.Specialization}",
                              "Успех",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчики событий для форматирования телефона
        private void PhoneBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            PhoneFormatter.PhoneBox_PreviewTextInput(sender, e);
        }

        private void PhoneBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PhoneFormatter.PhoneBox_TextChanged(sender, e);
        }
    }
}