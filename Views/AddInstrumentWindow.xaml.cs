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
    public partial class AddInstrumentWindow : Window
    {
        public AddInstrumentWindow()
        {
            InitializeComponent();
            CategoryCombo.SelectedIndex = 0; // Выбираем первую категорию по умолчанию
            BrandBox.Focus(); // Фокус на первое поле
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            try
            {
                using var context = new AppDbContext();

                // Проверяем, нет ли уже такого инструмента
                bool exists = context.Instruments.Any(i =>
                    i.Brand == BrandBox.Text &&
                    i.Model == ModelBox.Text &&
                    i.Category == CategoryCombo.Text);

                if (exists)
                {
                    MessageBox.Show("Такой инструмент уже есть в каталоге",
                                  "Внимание",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

                // Создаем новый инструмент
                var instrument = new Instrument
                {
                    Brand = BrandBox.Text.Trim(),
                    Model = ModelBox.Text.Trim(),
                    Category = (CategoryCombo.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    Price = decimal.Parse(PriceBox.Text),
                    StockQuantity = int.Parse(QuantityBox.Text),
                    Description = DescriptionBox.Text.Trim(),
                    SerialNumber = SerialBox.Text.Trim()
                };

                context.Instruments.Add(instrument);
                context.SaveChanges();

                MessageBox.Show($"Инструмент успешно добавлен!\n\n" +
                              $"{instrument.Brand} {instrument.Model}\n" +
                              $"Категория: {instrument.Category}\n" +
                              $"Количество: {instrument.StockQuantity} шт.\n" +
                              $"Цена: {instrument.Price:C}",
                              "Успех",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            // Проверка бренда
            if (string.IsNullOrWhiteSpace(BrandBox.Text))
            {
                MessageBox.Show("Введите бренд инструмента", "Ошибка");
                BrandBox.Focus();
                return false;
            }

            // Проверка модели
            if (string.IsNullOrWhiteSpace(ModelBox.Text))
            {
                MessageBox.Show("Введите модель инструмента", "Ошибка");
                ModelBox.Focus();
                return false;
            }

            // Проверка цены
            if (!decimal.TryParse(PriceBox.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену (больше 0)", "Ошибка");
                PriceBox.Focus();
                PriceBox.SelectAll();
                return false;
            }

            // Проверка количества
            if (!int.TryParse(QuantityBox.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Введите корректное количество (0 или больше)", "Ошибка");
                QuantityBox.Focus();
                QuantityBox.SelectAll();
                return false;
            }

            return true;
        }

        // Валидация для чисел
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Валидация для десятичных чисел
        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string newText = textBox.Text + e.Text;

            // Разрешаем цифры и одну точку/запятую
            Regex regex = new Regex(@"^[0-9]*[.,]?[0-9]*$");
            e.Handled = !regex.IsMatch(newText);
        }
    }
}