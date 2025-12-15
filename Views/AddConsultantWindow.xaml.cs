using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicStoreCatalog.Views
{
    public partial class AddConsultantWindow : Window
    {
        // Событие для уведомления о добавлении пользователя
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

                // Вызываем событие
                UserAdded?.Invoke(this, EventArgs.Empty);

                // Показываем сообщение
                MessageBox.Show($"✅ Консультант успешно добавлен!\n\n" +
                              $"Логин: {consultant.Login}\n" +
                              $"Имя: {consultant.FirstName} {consultant.LastName}\n" +
                              $"Специализация: {consultant.Specialization}",
                              "Успех",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                // ЗАКРЫВАЕМ окно (как было раньше)
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка при сохранении: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PhoneBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void PhoneBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text.Replace(" ", "").Replace("-", "").Replace(")", "").Replace("(", "");

            if (text.Length >= 4)
            {
                textBox.Text = FormatPhoneNumber(text);
                textBox.CaretIndex = textBox.Text.Length;
            }
        }

        private string FormatPhoneNumber(string phone)
        {
            if (phone.Length <= 4) return phone;

            string result = "+375 (";

            if (phone.Length > 4) result += phone.Substring(4, Math.Min(2, phone.Length - 4));
            if (phone.Length > 6) result += ")-" + phone.Substring(6, Math.Min(3, phone.Length - 6));
            if (phone.Length > 9) result += "-" + phone.Substring(9, Math.Min(2, phone.Length - 9));
            if (phone.Length > 11) result += "-" + phone.Substring(11, Math.Min(2, phone.Length - 11));

            return result;
        }
    }
}