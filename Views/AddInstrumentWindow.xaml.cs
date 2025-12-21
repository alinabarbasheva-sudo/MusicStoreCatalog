using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using MusicStoreCatalog.Utilities;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
                return;

            try
            {
                using var context = new AppDbContext();

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

                // Парсим цену
                if (!decimal.TryParse(PriceBox.Text.Replace(",", "."),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
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

        private string GetSelectedCategory()
        {
            if (CategoryCombo.SelectedItem is ComboBoxItem selectedItem)
                return selectedItem.Content?.ToString();
            else if (CategoryCombo.SelectedValue != null)
                return CategoryCombo.SelectedValue.ToString();
            else if (!string.IsNullOrEmpty(CategoryCombo.Text))
                return CategoryCombo.Text;

            return null;
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(BrandBox.Text))
            {
                MessageBox.Show("Введите бренд инструмента", "Ошибка");
                BrandBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(ModelBox.Text))
            {
                MessageBox.Show("Введите модель инструмента", "Ошибка");
                ModelBox.Focus();
                return false;
            }

            string category = GetSelectedCategory();
            if (string.IsNullOrEmpty(category))
            {
                MessageBox.Show("Выберите категорию инструмента", "Ошибка");
                CategoryCombo.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(PriceBox.Text))
            {
                MessageBox.Show("Введите цену инструмента", "Ошибка");
                PriceBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(QuantityBox.Text))
            {
                MessageBox.Show("Введите количество инструментов", "Ошибка");
                QuantityBox.Focus();
                return false;
            }

            return true;
        }

        // Обработчики валидации
        private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            ValidationHelper.NumberValidationTextBox(sender, e);
        }

        private void DecimalValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            ValidationHelper.DecimalValidationTextBox(sender, e);
        }
    }
}