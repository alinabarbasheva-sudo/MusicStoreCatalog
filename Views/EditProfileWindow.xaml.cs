using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicStoreCatalog.Views
{
    public partial class EditProfileWindow : Window
    {
        // Событие для уведомления об обновлении профиля
        public event EventHandler ProfileUpdated;

        private int _userId;
        private User _currentUser;

        public EditProfileWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadUserData();

            // Настройка форматирования телефона
            PhoneBox.PreviewTextInput += PhoneBox_PreviewTextInput;
            PhoneBox.TextChanged += PhoneBox_TextChanged;
        }

        private void LoadUserData()
        {
            try
            {
                using var context = new AppDbContext();
                _currentUser = context.Users.FirstOrDefault(u => u.ID == _userId);

                if (_currentUser != null)
                {
                    FirstNameBox.Text = _currentUser.FirstName;
                    LastNameBox.Text = _currentUser.LastName;
                    PhoneBox.Text = _currentUser.PhoneNumber;
                }
                else
                {
                    MessageBox.Show("Пользователь не найден", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация данных
            if (string.IsNullOrWhiteSpace(FirstNameBox.Text))
            {
                MessageBox.Show("Введите имя", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                FirstNameBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(LastNameBox.Text))
            {
                MessageBox.Show("Введите фамилию", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                LastNameBox.Focus();
                return;
            }

            try
            {
                using var context = new AppDbContext();
                var user = context.Users.FirstOrDefault(u => u.ID == _userId);

                if (user != null)
                {
                    // Обновляем данные
                    user.FirstName = FirstNameBox.Text.Trim();
                    user.LastName = LastNameBox.Text.Trim();
                    user.PhoneNumber = PhoneBox.Text.Trim();

                    context.SaveChanges();

                    // Вызываем событие обновления профиля
                    ProfileUpdated?.Invoke(this, EventArgs.Empty);

                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Форматирование телефона
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