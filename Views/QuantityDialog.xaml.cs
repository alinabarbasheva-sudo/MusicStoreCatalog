using MusicStoreCatalog.Models;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicStoreCatalog.Views
{
    public partial class QuantityDialog : Window
    {
        public int SelectedQuantity { get; private set; }
        public Instrument Instrument { get; set; }
        private int _maxQuantity = 100;

        public QuantityDialog(Instrument instrument)
        {
            InitializeComponent();
            Instrument = instrument;
            InitializeDialog();
        }

        private void InitializeDialog()
        {
            // Устанавливаем информацию об инструменте
            InstrumentInfoText.Text = $"{Instrument.Brand} {Instrument.Model}\nЦена: {Instrument.Price} br";

            // Информация о наличии на складе
            StockInfoText.Text = $"В наличии: {Instrument.StockQuantity} шт.";

            // Максимальное количество для заказа
            _maxQuantity = Math.Max(Instrument.StockQuantity * 2, 10);

            // Фокус на поле количества
            QuantityBox.Focus();
            QuantityBox.SelectAll();

            // Обработчики событий
            QuantityBox.PreviewTextInput += QuantityBox_PreviewTextInput;
            QuantityBox.TextChanged += QuantityBox_TextChanged;
        }

        private void DecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuantityBox.Text, out int quantity) && quantity > 1)
            {
                quantity--;
                QuantityBox.Text = quantity.ToString();
                ClearError();
            }
        }

        private void IncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuantityBox.Text, out int quantity))
            {
                if (quantity < _maxQuantity)
                {
                    quantity++;
                    QuantityBox.Text = quantity.ToString();
                    ClearError();
                }
                else
                {
                    ShowError($"Максимальное количество: {_maxQuantity} шт.");
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                SelectedQuantity = int.Parse(QuantityBox.Text);
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInput()
        {
            ClearError();

            if (!int.TryParse(QuantityBox.Text, out int quantity))
            {
                ShowError("Введите корректное число");
                QuantityBox.Focus();
                QuantityBox.SelectAll();
                return false;
            }

            if (quantity <= 0)
            {
                ShowError("Количество должно быть больше 0");
                QuantityBox.Focus();
                QuantityBox.SelectAll();
                return false;
            }

            if (quantity > _maxQuantity)
            {
                ShowError($"Максимальное количество: {_maxQuantity} шт.");
                QuantityBox.Focus();
                QuantityBox.SelectAll();
                return false;
            }

            return true;
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        private void ClearError()
        {
            ErrorText.Visibility = Visibility.Collapsed;
        }

        // Валидация ввода - только цифры
        private void QuantityBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void QuantityBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearError();
        }
    }
}