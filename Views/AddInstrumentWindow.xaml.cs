using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using System;
using System.Globalization;
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
            CategoryCombo.SelectedIndex = 0;
            BrandBox.Focus();
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

                // Получаем выбранную категорию правильно
                string selectedCategory = GetSelectedCategory();
                if (string.IsNullOrEmpty(selectedCategory))
                {
                    MessageBox.Show("Выберите категорию инструмента", "Ошибка");
                    CategoryCombo.Focus();
                    return;
                }

                // Проверяем, нет ли уже такого инструмента
                bool exists = context.Instruments.Any(i =>
                    i.Brand == BrandBox.Text.Trim() &&
                    i.Model == ModelBox.Text.Trim() &&
                    i.Category == selectedCategory);

                if (exists)
                {
                    MessageBox.Show("Такой инструмент уже есть в каталоге",
                                  "Внимание",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

                // Парсим цену с учетом культуры (запятая/точка)
                decimal price;
                if (!decimal.TryParse(PriceBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out price))
                {
                    MessageBox.Show("Введите корректную цену", "Ошибка");
                    PriceBox.Focus();
                    PriceBox.SelectAll();
                    return;
                }

                // Создаем новый инструмент
                var instrument = new Instrument
                {
                    Brand = BrandBox.Text.Trim(),
                    Model = ModelBox.Text.Trim(),
                    Category = selectedCategory,
                    Price = price,
                    StockQuantity = int.Parse(QuantityBox.Text)
                };

                context.Instruments.Add(instrument);
                context.SaveChanges();

                MessageBox.Show($"✅ Инструмент успешно добавлен!\n\n" +
                              $"{instrument.Brand} {instrument.Model}\n" +
                              $"Категория: {instrument.Category}\n" +
                              $"Количество: {instrument.StockQuantity} шт.\n" +
                              $"Цена: {instrument.Price} br",
                              "Успех",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка при сохранении: {ex.Message}\n\n" +
                              $"Подробности: {ex.InnerException?.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        // Метод для получения выбранной категории
        private string GetSelectedCategory()
        {
            if (CategoryCombo.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content?.ToString();
            }
            else if (CategoryCombo.SelectedValue != null)
            {
                return CategoryCombo.SelectedValue.ToString();
            }
            else if (!string.IsNullOrEmpty(CategoryCombo.Text))
            {
                return CategoryCombo.Text;
            }

            return null;
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

            // Проверка категории
            string category = GetSelectedCategory();
            if (string.IsNullOrEmpty(category))
            {
                MessageBox.Show("Выберите категорию инструмента", "Ошибка");
                CategoryCombo.Focus();
                return false;
            }

            // Проверка цены
            if (string.IsNullOrWhiteSpace(PriceBox.Text))
            {
                MessageBox.Show("Введите цену инструмента", "Ошибка");
                PriceBox.Focus();
                return false;
            }

            // Пробуем распарсить цену
            if (!decimal.TryParse(PriceBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену (больше 0)\nПример: 15000 или 15000.50", "Ошибка");
                PriceBox.Focus();
                PriceBox.SelectAll();
                return false;
            }

            // Проверка количества
            if (string.IsNullOrWhiteSpace(QuantityBox.Text))
            {
                MessageBox.Show("Введите количество инструментов", "Ошибка");
                QuantityBox.Focus();
                return false;
            }

            if (!int.TryParse(QuantityBox.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Введите корректное количество (0 или больше)", "Ошибка");
                QuantityBox.Focus();
                QuantityBox.SelectAll();
                return false;
            }

            return true;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string newText = textBox.Text + e.Text;

            // Разрешаем цифры, одну точку или запятую
            Regex regex = new Regex(@"^[0-9]*[,.]?[0-9]*$");
            e.Handled = !regex.IsMatch(newText);
        }

        // Обработчик изменения текста в поле цены
        private void PriceBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Автоматически заменяем запятую на точку
            if (PriceBox.Text.Contains(","))
            {
                PriceBox.Text = PriceBox.Text.Replace(",", ".");
                PriceBox.CaretIndex = PriceBox.Text.Length;
            }
        }
    }
}