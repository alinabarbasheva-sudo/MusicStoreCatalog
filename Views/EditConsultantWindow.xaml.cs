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
    public partial class EditConsultantWindow : Window
    {
        public event EventHandler ConsultantUpdated;
        private int _consultantId;
        private Consultant _consultant;

        public EditConsultantWindow(int consultantId)
        {
            InitializeComponent();
            _consultantId = consultantId;
            LoadConsultantData();

            // Настройка форматирования телефона
            PhoneBox.PreviewTextInput += PhoneBox_PreviewTextInput;
            PhoneBox.TextChanged += PhoneBox_TextChanged;
        }

        private void LoadConsultantData()
        {
            try
            {
                using var context = new AppDbContext();
                _consultant = context.Users.OfType<Consultant>().FirstOrDefault(c => c.ID == _consultantId);

                if (_consultant != null)
                {
                    LoginBox.Text = _consultant.Login;
                    FirstNameBox.Text = _consultant.FirstName;
                    LastNameBox.Text = _consultant.LastName;
                    PhoneBox.Text = _consultant.PhoneNumber;

                    // Устанавливаем специализацию
                    SetSpecialization(_consultant.Specialization);
                }
                else
                {
                    MessageBox.Show("Консультант не найден", "Ошибка");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
                this.Close();
            }
        }

        private void SetSpecialization(string specialization)
        {
            for (int i = 0; i < SpecializationCombo.Items.Count; i++)
            {
                if (SpecializationCombo.Items[i] is ComboBoxItem item &&
                    item.Content?.ToString() == specialization)
                {
                    SpecializationCombo.SelectedIndex = i;
                    return;
                }
            }
            SpecializationCombo.SelectedIndex = 0;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            try
            {
                using var context = new AppDbContext();
                var consultant = context.Users.OfType<Consultant>().FirstOrDefault(c => c.ID == _consultantId);

                if (consultant != null)
                {
                    // Обновляем данные
                    consultant.FirstName = FirstNameBox.Text.Trim();
                    consultant.LastName = LastNameBox.Text.Trim();
                    consultant.PhoneNumber = PhoneBox.Text.Trim();
                    consultant.Specialization = (SpecializationCombo.SelectedItem as ComboBoxItem)?.Content.ToString();

                    context.SaveChanges();

                    // Вызываем событие обновления
                    ConsultantUpdated?.Invoke(this, EventArgs.Empty);

                    MessageBox.Show($"✅ Данные консультанта обновлены!\n\n" +
                                  $"Логин: {consultant.Login}\n" +
                                  $"Имя: {consultant.FirstName} {consultant.LastName}\n" +
                                  $"Специализация: {consultant.Specialization}",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);

                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка сохранения: {ex.Message}", "Ошибка");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(FirstNameBox.Text))
            {
                MessageBox.Show("Введите имя", "Ошибка");
                FirstNameBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(LastNameBox.Text))
            {
                MessageBox.Show("Введите фамилию", "Ошибка");
                LastNameBox.Focus();
                return false;
            }

            return true;
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