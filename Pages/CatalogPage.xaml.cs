using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using MusicStoreCatalog.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MusicStoreCatalog.Pages
{
    public partial class CatalogPage : UserControl
    {
        public string UserRole { get; set; }
        private int _userId;
        private string _userSpecialization;

        public CatalogPage()
        {
            InitializeComponent();

            // Находим колонки
            var sellColumn = InstrumentsGrid.Columns
                .OfType<DataGridTemplateColumn>()
                .FirstOrDefault(c => c.Header?.ToString() == "Продажа");
            var orderColumn = InstrumentsGrid.Columns
                .OfType<DataGridTemplateColumn>()
                .FirstOrDefault(c => c.Header?.ToString() == "Заказ");

            if (sellColumn != null) SellColumn = sellColumn;
            if (orderColumn != null) OrderColumn = orderColumn;

            RefreshBtn.Click += RefreshBtn_Click;
            Loaded += (s, e) =>
            {
                UpdateButtonVisibility();
                LoadInstruments();
            };
        }

        public void SetUserRole(string role)
        {
            UserRole = role;
            UpdateButtonVisibility();
        }

        public void SetUserId(int userId)
        {
            _userId = userId;
        }

        public void SetUserSpecialization(string specialization)
        {
            _userSpecialization = specialization;

            if (UserRole == "Консультант" && !string.IsNullOrEmpty(specialization))
            {
                TitleText.Text = $"🎸 Каталог инструментов ({specialization})";
            }
        }

        private void UpdateButtonVisibility()
        {
            if (SellColumn != null)
                SellColumn.Visibility = UserRole == "Консультант" ? Visibility.Visible : Visibility.Collapsed;

            if (OrderColumn != null)
                OrderColumn.Visibility = UserRole == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadInstruments()
        {
            try
            {
                using var context = new AppDbContext();

                IQueryable<Instrument> query = context.Instruments;

                if (UserRole == "Консультант" && !string.IsNullOrEmpty(_userSpecialization))
                {
                    var categories = GetCategoriesForSpecialization(_userSpecialization);
                    if (categories.Any())
                    {
                        query = query.Where(i => categories.Contains(i.Category));
                    }
                }

                var instruments = query
                    .OrderBy(i => i.Category)
                    .ThenBy(i => i.Brand)
                    .ThenBy(i => i.Model)
                    .ToList();

                InstrumentsGrid.ItemsSource = instruments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки инструментов: {ex.Message}", "Ошибка");
            }
        }

        private List<string> GetCategoriesForSpecialization(string specialization)
        {
            var categoryMap = new Dictionary<string, List<string>>
            {
                { "Гитары", new List<string> { "Гитара", "Струнные", "Аксессуары" } },
                { "Клавишные", new List<string> { "Клавишные", "Аксессуары" } },
                { "Ударные", new List<string> { "Ударные", "Аксессуары" } },
                { "Духовые", new List<string> { "Духовые", "Аксессуары" } },
                { "Струнные", new List<string> { "Струнные", "Гитара", "Аксессуары" } }
            };

            return categoryMap.ContainsKey(specialization)
                ? categoryMap[specialization]
                : new List<string>();
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadInstruments();
        }

        private void SellButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null && int.TryParse(button.Tag.ToString(), out int instrumentId))
            {
                using var context = new AppDbContext();
                var instrument = context.Instruments.FirstOrDefault(i => i.Id == instrumentId);

                if (instrument != null && instrument.StockQuantity > 0)
                {
                    instrument.StockQuantity -= 1;

                    if (_userId > 0)
                    {
                        var consultant = context.Users.OfType<Consultant>().FirstOrDefault(c => c.ID == _userId);
                        if (consultant != null)
                        {
                            consultant.SalesCount += 1;
                        }
                    }

                    context.SaveChanges();
                    LoadInstruments();

                    MessageBox.Show($"Продажа успешно завершена!\n" +
                                  $"Осталось: {instrument.StockQuantity} шт.",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
            }
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null && int.TryParse(button.Tag.ToString(), out int instrumentId))
            {
                using var context = new AppDbContext();
                var instrument = context.Instruments.FirstOrDefault(i => i.Id == instrumentId);

                if (instrument != null && _userId > 0)
                {
                    var orderWindow = new CreateOrderWindow(instrument, _userId);
                    orderWindow.Owner = Window.GetWindow(this);
                    orderWindow.ShowDialog();

                    if (orderWindow.DialogResult == true)
                    {
                        MessageBox.Show("Заявка создана!", "Успех");
                    }
                }
            }
        }
    }
}