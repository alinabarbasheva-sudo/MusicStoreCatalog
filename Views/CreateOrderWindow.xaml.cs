using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicStoreCatalog.Views
{
    public partial class CreateOrderWindow : Window
    {
        private readonly Instrument _instrument;
        private readonly int _userId;

        public CreateOrderWindow(Instrument instrument, int userId)
        {
            InitializeComponent();
            _instrument = instrument;
            _userId = userId;

            // Заполняем информацию об инструменте
            InstrumentInfoText.Text = $"{instrument.Brand} {instrument.Model}\n" +
                                     $"Категория: {instrument.Category}\n" +
                                     $"Текущее количество: {instrument.StockQuantity}";

            // Устанавливаем примерную цену
            PriceBox.Text = instrument.Price.ToString();

            // Фокус на поле количества
            QuantityBox.Focus();
            QuantityBox.SelectAll();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void CreateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(QuantityBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(PriceBox.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using var context = new AppDbContext();

            // Используем переданный userId без повторного поиска по логину
            var user = context.Users.FirstOrDefault(u => u.ID == _userId);
            if (user == null)
            {
                MessageBox.Show($"Пользователь с ID {_userId} не найден", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Создаем заявку
            var orderRequest = new OrderRequest
            {
                InstrumentId = _instrument.Id,
                InstrumentName = $"{_instrument.Brand} {_instrument.Model}",
                Brand = _instrument.Brand,
                Model = _instrument.Model,
                Category = _instrument.Category,
                Quantity = quantity,
                EstimatedPrice = price,
                Notes = NotesBox.Text,
                RequestedById = _userId,
                RequestDate = DateTime.Now,
                Status = "Pending"
            };

            context.OrderRequests.Add(orderRequest);
            context.SaveChanges();

            this.DialogResult = true;
            this.Close();
        }

        // Валидация для чисел
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        // Валидация для десятичных чисел
        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string newText = textBox.Text + e.Text;

            e.Handled = !decimal.TryParse(newText, out _);
        }
    }
}