using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using MusicStoreCatalog.Utilities; // Добавляем using
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            // Используем утилитарные методы
            PhoneBox.PreviewTextInput += PhoneFormatter.PhoneBox_PreviewTextInput;
            PhoneBox.TextChanged += PhoneFormatter.PhoneBox_TextChanged;
        }

        private void LoadConsultantData()
        {
            try
            {
                using var context = new AppDbContext();
                _consultant = context.Users.OfType<Consultant>()
                    .FirstOrDefault(c => c.ID == _consultantId);

                if (_consultant != null)
                {
                    LoginBox.Text = _consultant.Login;
                    FirstNameBox.Text = _consultant.FirstName;
                    LastNameBox.Text = _consultant.LastName;
                    PhoneBox.Text = _consultant.PhoneNumber;
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
            if (!ValidateInput()) return;

            try
            {
                using var context = new AppDbContext();
                var consultant = context.Users.OfType<Consultant>()
                    .FirstOrDefault(c => c.ID == _consultantId);

                if (consultant != null)
                {
                    consultant.FirstName = FirstNameBox.Text.Trim();
                    consultant.LastName = LastNameBox.Text.Trim();
                    consultant.PhoneNumber = PhoneBox.Text.Trim();
                    consultant.Specialization = (SpecializationCombo.SelectedItem as ComboBoxItem)?.Content.ToString();

                    context.SaveChanges();
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

        // УДАЛИТЬ старые методы форматирования телефона
    }
}