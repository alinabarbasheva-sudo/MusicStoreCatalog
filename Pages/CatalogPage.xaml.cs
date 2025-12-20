using Microsoft.VisualBasic;
using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using MusicStoreCatalog.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MusicStoreCatalog.Pages
{
    public partial class CatalogPage : UserControl
    {
        public string UserRole { get; set; }
        private int _userId;
        private string _userSpecialization;
        private List<Instrument> _allInstruments = new List<Instrument>();
        private string _currentSearchText = string.Empty;
        private SearchFilters _currentFilters = new SearchFilters();

        public CatalogPage()
        {
            InitializeComponent();

            // Подключаем обработчики
            RefreshBtn.Click += RefreshBtn_Click;
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;

            // При загрузке страницы
            Loaded += CatalogPage_Loaded;
        }

        // Класс для хранения фильтров поиска
        private class SearchFilters
        {
            public string Brand { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public decimal? MinPrice { get; set; }
            public decimal? MaxPrice { get; set; }
            public int? MinStock { get; set; }
            public int? MaxStock { get; set; }
            public bool InStockOnly { get; set; } = false;
        }

        private void CatalogPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateButtonVisibility();
            LoadInstruments();
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
                TitleText.Text = $"📋 Каталог инструментов ({specialization})";
            }
        }

        private void UpdateButtonVisibility()
        {
            if (UserRole == "Консультант")
            {
                // Консультант видит кнопки продажи и заказа
                SellColumn.Visibility = Visibility.Visible;
                OrderColumn.Visibility = Visibility.Visible;
                DeleteColumn.Visibility = Visibility.Collapsed;
            }
            else if (UserRole == "Администратор")
            {
                // Админ видит кнопку заказа и кнопку удаления
                SellColumn.Visibility = Visibility.Collapsed;
                OrderColumn.Visibility = Visibility.Visible;
                DeleteColumn.Visibility = Visibility.Visible;
            }
            else
            {
                // Остальные не видят ничего
                SellColumn.Visibility = Visibility.Collapsed;
                OrderColumn.Visibility = Visibility.Collapsed;
                DeleteColumn.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadInstruments()
        {
            try
            {
                using var context = new AppDbContext();

                IQueryable<Instrument> query = context.Instruments;

                // Если пользователь - консультант, фильтруем по специализации
                if (UserRole == "Консультант" && !string.IsNullOrEmpty(_userSpecialization))
                {
                    var categories = GetCategoriesForSpecialization(_userSpecialization);
                    if (categories.Any())
                    {
                        query = query.Where(i => categories.Contains(i.Category));
                    }
                }

                _allInstruments = query
                    .OrderBy(i => i.Category)
                    .ThenBy(i => i.Brand)
                    .ThenBy(i => i.Model)
                    .ToList();

                // Применяем текущий поиск и фильтры
                ApplySearchAndFilters();
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

        // ===== ПОИСК И ФИЛЬТРАЦИЯ =====
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearchText = SearchTextBox.Text.Trim();
            ApplySearchAndFilters();
        }

        private void ClearSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _currentFilters = new SearchFilters();
            ApplySearchAndFilters();
        }

        private void AdvancedSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowAdvancedSearchDialog();
        }

        private void ApplySearchAndFilters()
        {
            if (!_allInstruments.Any())
            {
                InstrumentsGrid.ItemsSource = null;
                return;
            }

            var filteredInstruments = _allInstruments.AsEnumerable();

            // Применяем текстовый поиск
            if (!string.IsNullOrEmpty(_currentSearchText))
            {
                filteredInstruments = filteredInstruments.Where(i =>
                    (i.Brand != null && i.Brand.Contains(_currentSearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (i.Model != null && i.Model.Contains(_currentSearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (i.Category != null && i.Category.Contains(_currentSearchText, StringComparison.OrdinalIgnoreCase)));
            }

            // Применяем фильтры из расширенного поиска
            if (!string.IsNullOrEmpty(_currentFilters.Brand))
            {
                filteredInstruments = filteredInstruments.Where(i =>
                    i.Brand != null && i.Brand.Equals(_currentFilters.Brand, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(_currentFilters.Model))
            {
                filteredInstruments = filteredInstruments.Where(i =>
                    i.Model != null && i.Model.Contains(_currentFilters.Model, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(_currentFilters.Category))
            {
                filteredInstruments = filteredInstruments.Where(i =>
                    i.Category != null && i.Category.Equals(_currentFilters.Category, StringComparison.OrdinalIgnoreCase));
            }

            if (_currentFilters.MinPrice.HasValue)
            {
                filteredInstruments = filteredInstruments.Where(i => i.Price >= _currentFilters.MinPrice.Value);
            }

            if (_currentFilters.MaxPrice.HasValue)
            {
                filteredInstruments = filteredInstruments.Where(i => i.Price <= _currentFilters.MaxPrice.Value);
            }

            if (_currentFilters.MinStock.HasValue)
            {
                filteredInstruments = filteredInstruments.Where(i => i.StockQuantity >= _currentFilters.MinStock.Value);
            }

            if (_currentFilters.MaxStock.HasValue)
            {
                filteredInstruments = filteredInstruments.Where(i => i.StockQuantity <= _currentFilters.MaxStock.Value);
            }

            if (_currentFilters.InStockOnly)
            {
                filteredInstruments = filteredInstruments.Where(i => i.StockQuantity > 0);
            }

            // Обновляем DataGrid
            InstrumentsGrid.ItemsSource = filteredInstruments
                .OrderBy(i => i.Category)
                .ThenBy(i => i.Brand)
                .ThenBy(i => i.Model)
                .ToList();

            // Показываем количество найденных инструментов
            UpdateResultsCount(filteredInstruments.Count());
        }

        private void UpdateResultsCount(int count)
        {
            var totalCount = _allInstruments.Count;
            var title = TitleText.Text;

            // Убираем предыдущий счетчик, если он есть
            var idx = title.IndexOf("(");
            if (idx > 0)
            {
                title = title.Substring(0, idx).Trim();
            }

            if (count != totalCount || !string.IsNullOrEmpty(_currentSearchText))
            {
                TitleText.Text = $"{title} (Найдено: {count} из {totalCount})";
            }
            else
            {
                TitleText.Text = title;
            }
        }

        private void ShowAdvancedSearchDialog()
        {
            try
            {
                using var context = new AppDbContext();

                // Получаем уникальные бренды и категории для выпадающих списков
                var brands = context.Instruments
                    .Select(i => i.Brand)
                    .Distinct()
                    .OrderBy(b => b)
                    .ToList();

                var categories = context.Instruments
                    .Select(i => i.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                // Получаем минимальную и максимальную цену
                var minPrice = context.Instruments.Min(i => i.Price);
                var maxPrice = context.Instruments.Max(i => i.Price);

                var minStock = context.Instruments.Min(i => i.StockQuantity);
                var maxStock = context.Instruments.Max(i => i.StockQuantity);

                // Создаем окно расширенного поиска
                var dialog = new Window
                {
                    Title = "⚙️ Расширенный поиск",
                    Width = 400,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize
                };

                var stackPanel = new StackPanel { Margin = new Thickness(20) };

                // Бренд
                var brandLabel = new Label { Content = "Бренд:", FontWeight = FontWeights.Bold };
                var brandComboBox = new ComboBox
                {
                    ItemsSource = new List<string> { "" }.Concat(brands),
                    SelectedItem = _currentFilters.Brand
                };

                // Модель
                var modelLabel = new Label { Content = "Модель:", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 10, 0, 0) };
                var modelTextBox = new TextBox { Text = _currentFilters.Model };

                // Категория
                var categoryLabel = new Label { Content = "Категория:", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 10, 0, 0) };
                var categoryComboBox = new ComboBox
                {
                    ItemsSource = new List<string> { "" }.Concat(categories),
                    SelectedItem = _currentFilters.Category
                };

                // Цена
                var priceLabel = new Label { Content = $"Цена (от {minPrice} до {maxPrice} br):", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 10, 0, 0) };
                var priceStack = new StackPanel { Orientation = Orientation.Horizontal };
                var minPriceTextBox = new TextBox { Width = 100, Text = _currentFilters.MinPrice?.ToString() ?? "", ToolTip = "Минимальная цена" };
                var priceSeparator = new Label { Content = " - ", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 5, 0) };
                var maxPriceTextBox = new TextBox { Width = 100, Text = _currentFilters.MaxPrice?.ToString() ?? "", ToolTip = "Максимальная цена" };

                priceStack.Children.Add(minPriceTextBox);
                priceStack.Children.Add(priceSeparator);
                priceStack.Children.Add(maxPriceTextBox);

                // Количество на складе
                var stockLabel = new Label { Content = $"Количество (от {minStock} до {maxStock}):", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 10, 0, 0) };
                var stockStack = new StackPanel { Orientation = Orientation.Horizontal };
                var minStockTextBox = new TextBox { Width = 100, Text = _currentFilters.MinStock?.ToString() ?? "", ToolTip = "Минимальное количество" };
                var stockSeparator = new Label { Content = " - ", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 5, 0) };
                var maxStockTextBox = new TextBox { Width = 100, Text = _currentFilters.MaxStock?.ToString() ?? "", ToolTip = "Максимальное количество" };

                stockStack.Children.Add(minStockTextBox);
                stockStack.Children.Add(stockSeparator);
                stockStack.Children.Add(maxStockTextBox);

                // Только в наличии
                var inStockCheckBox = new CheckBox
                {
                    Content = "Только инструменты в наличии",
                    IsChecked = _currentFilters.InStockOnly,
                    Margin = new Thickness(0, 10, 0, 0)
                };

                // Кнопки
                var buttonStack = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 20, 0, 0) };

                var searchButton = new Button
                {
                    Content = "🔍 Поиск",
                    Width = 100,
                    Height = 30,
                    Margin = new Thickness(0, 0, 10, 0),
                    Background = Brushes.DodgerBlue,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold
                };

                var clearButton = new Button
                {
                    Content = "❌ Очистить",
                    Width = 100,
                    Height = 30,
                    Margin = new Thickness(10, 0, 0, 0),
                    Background = Brushes.LightGray
                };

                buttonStack.Children.Add(searchButton);
                buttonStack.Children.Add(clearButton);

                // Добавляем элементы на панель
                stackPanel.Children.Add(brandLabel);
                stackPanel.Children.Add(brandComboBox);
                stackPanel.Children.Add(modelLabel);
                stackPanel.Children.Add(modelTextBox);
                stackPanel.Children.Add(categoryLabel);
                stackPanel.Children.Add(categoryComboBox);
                stackPanel.Children.Add(priceLabel);
                stackPanel.Children.Add(priceStack);
                stackPanel.Children.Add(stockLabel);
                stackPanel.Children.Add(stockStack);
                stackPanel.Children.Add(inStockCheckBox);
                stackPanel.Children.Add(buttonStack);

                dialog.Content = stackPanel;

                // Обработчики событий
                searchButton.Click += (s, e) =>
                {
                    // Сохраняем фильтры
                    _currentFilters.Brand = brandComboBox.SelectedItem?.ToString() ?? string.Empty;
                    _currentFilters.Model = modelTextBox.Text.Trim();
                    _currentFilters.Category = categoryComboBox.SelectedItem?.ToString() ?? string.Empty;
                    _currentFilters.InStockOnly = inStockCheckBox.IsChecked ?? false;

                    // Парсим цены
                    if (decimal.TryParse(minPriceTextBox.Text, out decimal minPriceValue) && minPriceValue >= 0)
                        _currentFilters.MinPrice = minPriceValue;
                    else
                        _currentFilters.MinPrice = null;

                    if (decimal.TryParse(maxPriceTextBox.Text, out decimal maxPriceValue) && maxPriceValue >= 0)
                        _currentFilters.MaxPrice = maxPriceValue;
                    else
                        _currentFilters.MaxPrice = null;

                    // Парсим количество
                    if (int.TryParse(minStockTextBox.Text, out int minStockValue) && minStockValue >= 0)
                        _currentFilters.MinStock = minStockValue;
                    else
                        _currentFilters.MinStock = null;

                    if (int.TryParse(maxStockTextBox.Text, out int maxStockValue) && maxStockValue >= 0)
                        _currentFilters.MaxStock = maxStockValue;
                    else
                        _currentFilters.MaxStock = null;

                    // Применяем фильтры
                    ApplySearchAndFilters();
                    dialog.Close();
                };

                clearButton.Click += (s, e) =>
                {
                    brandComboBox.SelectedItem = "";
                    modelTextBox.Text = "";
                    categoryComboBox.SelectedItem = "";
                    minPriceTextBox.Text = "";
                    maxPriceTextBox.Text = "";
                    minStockTextBox.Text = "";
                    maxStockTextBox.Text = "";
                    inStockCheckBox.IsChecked = false;
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии диалога поиска: {ex.Message}", "Ошибка");
            }
        }

        // ===== ОБРАБОТЧИК КНОПКИ ОБНОВЛЕНИЯ =====
        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadInstruments();
        }


        // ===== ОБРАБОТЧИК КНОПКИ ПРОДАЖИ =====
        private void SellButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null && int.TryParse(button.Tag.ToString(), out int instrumentId))
            {
                SellInstrument(instrumentId);
            }
        }

        private void SellInstrument(int instrumentId)
        {
            try
            {
                using var context = new AppDbContext();
                var instrument = context.Instruments.FirstOrDefault(i => i.Id == instrumentId);

                if (instrument == null)
                {
                    MessageBox.Show("Инструмент не найден", "Ошибка");
                    return;
                }

                if (instrument.StockQuantity > 0)
                {
                    // Уменьшаем количество
                    instrument.StockQuantity -= 1;

                    // Увеличиваем счетчик продаж консультанта
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

                    MessageBox.Show($"✅ Продажа завершена!\n\n" +
                                  $"{instrument.Brand} {instrument.Model}\n" +
                                  $"Осталось в наличии: {instrument.StockQuantity} шт.",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("⚠️ Этот инструмент закончился на складе",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при продаже: {ex.Message}", "Ошибка");
            }
        }

        // ===== ОБРАБОТЧИК КНОПКИ ЗАКАЗА =====
        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null && int.TryParse(button.Tag.ToString(), out int instrumentId))
            {
                OrderInstrument(instrumentId);
            }
        }

        private void OrderInstrument(int instrumentId)
        {
            try
            {
                // Проверяем, что ID пользователя установлен
                if (_userId == 0)
                {
                    MessageBox.Show("Ошибка: ID пользователя не установлен",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                    return;
                }

                using var context = new AppDbContext();
                var instrument = context.Instruments.FirstOrDefault(i => i.Id == instrumentId);

                if (instrument == null)
                {
                    MessageBox.Show("Инструмент не найден", "Ошибка");
                    return;
                }

                // Используем InputBox для выбора количества
                string input = Interaction.InputBox(
                    $"Введите количество для заказа:\n\n" +
                    $"Инструмент: {instrument.Brand} {instrument.Model}\n" +
                    $"Цена: {instrument.Price} br\n" +
                    $"В наличии: {instrument.StockQuantity} шт.\n\n" +
                    $"Максимальное количество: {Math.Max(instrument.StockQuantity * 2, 10)} шт.",
                    "Выбор количества",
                    "1");

                // Если пользователь нажал Отмена или закрыл окно
                if (string.IsNullOrEmpty(input)) return;

                // Проверяем введенное количество
                if (!int.TryParse(input, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Введите корректное количество (больше 0)", "Ошибка");
                    return;
                }

                // Проверяем максимальное количество
                int maxQuantity = Math.Max(instrument.StockQuantity * 2, 10);
                if (quantity > maxQuantity)
                {
                    MessageBox.Show($"Максимальное количество для заказа: {maxQuantity} шт.", "Ошибка");
                    return;
                }

                // Получаем текущего пользователя
                var currentUser = context.Users.FirstOrDefault(u => u.ID == _userId);
                if (currentUser == null)
                {
                    MessageBox.Show("Пользователь не найден", "Ошибка");
                    return;
                }

                // Проверяем, есть ли уже активная заявка на этот инструмент от этого пользователя
                var existingOrder = context.OrderRequests
                    .FirstOrDefault(o => o.InstrumentId == instrumentId &&
                                       o.RequestedById == _userId &&
                                       o.Status == "Pending");

                if (existingOrder != null)
                {
                    // Если заявка уже есть - предлагаем увеличить количество
                    var result = MessageBox.Show(
                        $"У вас уже есть активная заявка на этот инструмент.\n" +
                        $"Текущее количество в заявке: {existingOrder.Quantity} шт.\n\n" +
                        $"Хотите добавить {quantity} шт. к существующей заявке?\n" +
                        $"(Новое количество: {existingOrder.Quantity + quantity} шт.)",
                        "Обновление заявки",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        existingOrder.Quantity += quantity;
                        existingOrder.EstimatedPrice = instrument.Price * existingOrder.Quantity;
                        context.SaveChanges();

                        MessageBox.Show($"✅ Заявка обновлена!\n\n" +
                                      $"Инструмент: {instrument.Brand} {instrument.Model}\n" +
                                      $"Добавлено: {quantity} шт.\n" +
                                      $"Теперь в заявке: {existingOrder.Quantity} шт.",
                                      "Успех",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        return;
                    }
                }

                // Создаем новую заявку
                var orderRequest = new OrderRequest
                {
                    InstrumentId = instrument.Id,
                    InstrumentName = $"{instrument.Brand} {instrument.Model}",
                    Brand = instrument.Brand,
                    Model = instrument.Model,
                    Category = instrument.Category,
                    Quantity = quantity,
                    EstimatedPrice = instrument.Price * quantity,
                    Notes = $"Заказ через каталог. Остаток на складе: {instrument.StockQuantity} шт.",
                    RequestedById = _userId,
                    RequestDate = DateTime.Now,
                    Status = "Pending"
                };

                context.OrderRequests.Add(orderRequest);
                context.SaveChanges();

                MessageBox.Show($"✅ Заявка на заказ создана!\n\n" +
                              $"Инструмент: {instrument.Brand} {instrument.Model}\n" +
                              $"Количество: {quantity} шт.\n" +
                              $"Общая стоимость: {instrument.Price * quantity} br\n" +
                              $"Цена за единицу: {instrument.Price} br\n\n" +
                              $"Заявка будет рассмотрена администратором.",
                              "Успех",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании заявки: {ex.Message}", "Ошибка");
            }
        }

        // ===== ОБРАБОТЧИК КНОПКИ УДАЛЕНИЯ =====
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null && int.TryParse(button.Tag.ToString(), out int instrumentId))
            {
                DeleteInstrument(instrumentId);
            }
        }

        private void DeleteInstrument(int instrumentId)
        {
            // Проверяем, что пользователь - администратор
            if (UserRole != "Администратор")
            {
                MessageBox.Show("Только администратор может удалять инструменты",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new AppDbContext();
                var instrument = context.Instruments.FirstOrDefault(i => i.Id == instrumentId);

                if (instrument == null)
                {
                    MessageBox.Show("Инструмент не найден", "Ошибка");
                    return;
                }

                // Проверяем, есть ли активные заявки на этот инструмент
                var activeOrders = context.OrderRequests
                    .Where(o => o.InstrumentId == instrumentId && o.Status == "Pending")
                    .ToList();

                if (activeOrders.Any())
                {
                    var result = MessageBox.Show(
                        $"На этот инструмент есть активные заявки в ожидании ({activeOrders.Count} шт.).\n" +
                        "Если вы удалите инструмент, эти заявки будут отклонены.\n\n" +
                        "Продолжить удаление?",
                        "Предупреждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes)
                        return;
                }

                // Подтверждение удаления
                var confirmResult = MessageBox.Show(
                    $"Вы уверены, что хотите удалить инструмент?\n\n" +
                    $"{instrument.Brand} {instrument.Model}\n" +
                    $"Категория: {instrument.Category}\n" +
                    $"Цена: {instrument.Price} br\n" +
                    $"В наличии: {instrument.StockQuantity} шт.",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmResult != MessageBoxResult.Yes)
                    return;

                // Удаляем связанные заявки (Pending)
                var ordersToDelete = context.OrderRequests
                    .Where(o => o.InstrumentId == instrumentId)
                    .ToList();

                context.OrderRequests.RemoveRange(ordersToDelete);

                // Удаляем сам инструмент
                context.Instruments.Remove(instrument);
                context.SaveChanges();

                MessageBox.Show($"✅ Инструмент успешно удален!\n\n" +
                              $"{instrument.Brand} {instrument.Model}\n" +
                              $"Удалено заявок: {ordersToDelete.Count} шт.",
                              "Успех",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                // Обновляем список инструментов
                LoadInstruments();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка при удалении: {ex.Message}", "Ошибка");
            }
        }
    }
}