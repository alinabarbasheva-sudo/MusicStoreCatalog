using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using MusicStoreCatalog.Utilities;
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

        // Упрощенные фильтры вместо SearchFilters класса
        private string _filterBrand = string.Empty;
        private string _filterModel = string.Empty;
        private string _filterCategory = string.Empty;
        private decimal? _filterMinPrice = null;
        private decimal? _filterMaxPrice = null;
        private bool _filterInStockOnly = false;

        public CatalogPage()
        {
            InitializeComponent();

            // Подключаем обработчики
            RefreshBtn.Click += RefreshBtn_Click;
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;

            // При загрузке страницы
            Loaded += CatalogPage_Loaded;
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
                SellColumn.Visibility = Visibility.Visible;
                OrderColumn.Visibility = Visibility.Visible;
                DeleteColumn.Visibility = Visibility.Collapsed;
            }
            else if (UserRole == "Администратор")
            {
                SellColumn.Visibility = Visibility.Collapsed;
                OrderColumn.Visibility = Visibility.Visible;
                DeleteColumn.Visibility = Visibility.Visible;
            }
            else
            {
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

                // Фильтр по специализации для консультантов
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

                ApplySearchAndFilters();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Ошибка загрузки инструментов: {ex.Message}");
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
            ClearAllFilters();
            ApplySearchAndFilters();
        }

        private void AdvancedSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowAdvancedSearchDialog();
        }

        private void ClearAllFilters()
        {
            _filterBrand = string.Empty;
            _filterModel = string.Empty;
            _filterCategory = string.Empty;
            _filterMinPrice = null;
            _filterMaxPrice = null;
            _filterInStockOnly = false;
        }

        private void ApplySearchAndFilters()
        {
            if (!_allInstruments.Any())
            {
                InstrumentsGrid.ItemsSource = null;
                return;
            }

            var filteredInstruments = _allInstruments.AsEnumerable();

            // Текстовый поиск
            if (!string.IsNullOrEmpty(_currentSearchText))
            {
                filteredInstruments = filteredInstruments.Where(i =>
                    (i.Brand?.Contains(_currentSearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (i.Model?.Contains(_currentSearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (i.Category?.Contains(_currentSearchText, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            // Фильтры
            if (!string.IsNullOrEmpty(_filterBrand))
                filteredInstruments = filteredInstruments.Where(i => i.Brand == _filterBrand);

            if (!string.IsNullOrEmpty(_filterModel))
                filteredInstruments = filteredInstruments.Where(i =>
                    i.Model?.Contains(_filterModel, StringComparison.OrdinalIgnoreCase) ?? false);

            if (!string.IsNullOrEmpty(_filterCategory))
                filteredInstruments = filteredInstruments.Where(i => i.Category == _filterCategory);

            if (_filterMinPrice.HasValue)
                filteredInstruments = filteredInstruments.Where(i => i.Price >= _filterMinPrice.Value);

            if (_filterMaxPrice.HasValue)
                filteredInstruments = filteredInstruments.Where(i => i.Price <= _filterMaxPrice.Value);

            if (_filterInStockOnly)
                filteredInstruments = filteredInstruments.Where(i => i.StockQuantity > 0);

            // Обновление DataGrid
            UpdateInstrumentsGrid(filteredInstruments);
        }

        private void UpdateInstrumentsGrid(IEnumerable<Instrument> instruments)
        {
            InstrumentsGrid.ItemsSource = instruments
                .OrderBy(i => i.Category)
                .ThenBy(i => i.Brand)
                .ThenBy(i => i.Model)
                .ToList();

            UpdateResultsCount(instruments.Count());
        }

        private void UpdateResultsCount(int count)
        {
            var totalCount = _allInstruments.Count;
            var title = TitleText.Text;

            // Убираем предыдущий счетчик
            var idx = title.IndexOf("(");
            if (idx > 0)
                title = title.Substring(0, idx).Trim();

            if (count != totalCount || !string.IsNullOrEmpty(_currentSearchText))
                TitleText.Text = $"{title} (Найдено: {count} из {totalCount})";
            else
                TitleText.Text = title;
        }

        private void ShowAdvancedSearchDialog()
        {
            try
            {
                using var context = new AppDbContext();

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

                var minPrice = context.Instruments.Min(i => i.Price);
                var maxPrice = context.Instruments.Max(i => i.Price);

                var dialog = CreateSearchDialog(brands, categories, minPrice, maxPrice);
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Ошибка при открытии диалога поиска: {ex.Message}");
            }
        }

        private Window CreateSearchDialog(List<string> brands, List<string> categories,
                                         decimal minPrice, decimal maxPrice)
        {
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
            var brandComboBox = new ComboBox
            {
                ItemsSource = new List<string> { "" }.Concat(brands),
                SelectedItem = _filterBrand
            };

            // Модель
            var modelTextBox = new TextBox { Text = _filterModel };

            // Категория
            var categoryComboBox = new ComboBox
            {
                ItemsSource = new List<string> { "" }.Concat(categories),
                SelectedItem = _filterCategory
            };

            // Цена
            var priceStack = new StackPanel { Orientation = Orientation.Horizontal };
            var minPriceTextBox = new TextBox
            {
                Width = 100,
                Text = _filterMinPrice?.ToString() ?? "",
                ToolTip = "Минимальная цена"
            };
            var maxPriceTextBox = new TextBox
            {
                Width = 100,
                Text = _filterMaxPrice?.ToString() ?? "",
                ToolTip = "Максимальная цена"
            };
            priceStack.Children.Add(minPriceTextBox);
            priceStack.Children.Add(new Label { Content = " - ", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 5, 0) });
            priceStack.Children.Add(maxPriceTextBox);

            // Только в наличии
            var inStockCheckBox = new CheckBox
            {
                Content = "Только инструменты в наличии",
                IsChecked = _filterInStockOnly,
                Margin = new Thickness(0, 10, 0, 0)
            };

            // Кнопки
            var buttonStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

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

            // Добавляем элементы с метками
            AddControlWithLabel(stackPanel, "Бренд:", brandComboBox);
            AddControlWithLabel(stackPanel, "Модель:", modelTextBox, new Thickness(0, 10, 0, 0));
            AddControlWithLabel(stackPanel, "Категория:", categoryComboBox, new Thickness(0, 10, 0, 0));

            // Цена с меткой
            var priceLabel = new Label
            {
                Content = $"Цена (от {minPrice} до {maxPrice} br):",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 0)
            };
            stackPanel.Children.Add(priceLabel);
            stackPanel.Children.Add(priceStack);

            stackPanel.Children.Add(inStockCheckBox);
            stackPanel.Children.Add(buttonStack);

            dialog.Content = stackPanel;

            // Обработчики событий
            searchButton.Click += (s, e) =>
            {
                UpdateFiltersFromDialog(brandComboBox, modelTextBox, categoryComboBox,
                                      minPriceTextBox, maxPriceTextBox, inStockCheckBox);
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
                inStockCheckBox.IsChecked = false;
            };

            return dialog;
        }

        private void AddControlWithLabel(StackPanel panel, string labelText, Control control, Thickness? margin = null)
        {
            var label = new Label
            {
                Content = labelText,
                FontWeight = FontWeights.Bold,
                Margin = margin ?? new Thickness(0, 10, 0, 0)
            };
            panel.Children.Add(label);
            panel.Children.Add(control);
        }

        private void UpdateFiltersFromDialog(ComboBox brandComboBox, TextBox modelTextBox,
                                           ComboBox categoryComboBox, TextBox minPriceTextBox,
                                           TextBox maxPriceTextBox, CheckBox inStockCheckBox)
        {
            _filterBrand = brandComboBox.SelectedItem?.ToString() ?? string.Empty;
            _filterModel = modelTextBox.Text.Trim();
            _filterCategory = categoryComboBox.SelectedItem?.ToString() ?? string.Empty;
            _filterInStockOnly = inStockCheckBox.IsChecked ?? false;

            // Парсим цены
            _filterMinPrice = ParseDecimal(minPriceTextBox.Text);
            _filterMaxPrice = ParseDecimal(maxPriceTextBox.Text);
        }

        private decimal? ParseDecimal(string text)
        {
            if (decimal.TryParse(text, out decimal value) && value >= 0)
                return value;
            return null;
        }

        // ===== ОБРАБОТЧИК КНОПКИ ОБНОВЛЕНИЯ =====

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadInstruments();
        }

        // ===== ОБРАБОТЧИК КНОПКИ ПРОДАЖИ =====

        private void SellButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null &&
                int.TryParse(button.Tag.ToString(), out int instrumentId))
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
                    MessageHelper.ShowError("Инструмент не найден");
                    return;
                }

                if (instrument.StockQuantity > 0)
                {
                    instrument.StockQuantity -= 1;

                    // Увеличиваем счетчик продаж консультанта
                    if (_userId > 0)
                    {
                        var consultant = context.Users.OfType<Consultant>()
                            .FirstOrDefault(c => c.ID == _userId);
                        if (consultant != null)
                            consultant.SalesCount += 1;
                    }

                    context.SaveChanges();
                    LoadInstruments();

                    MessageHelper.ShowSuccess($"✅ Продажа завершена!\n\n" +
                        $"{instrument.Brand} {instrument.Model}\n" +
                        $"Осталось в наличии: {instrument.StockQuantity} шт.");
                }
                else
                {
                    MessageHelper.ShowWarning("⚠️ Этот инструмент закончился на складе");
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Ошибка при продаже: {ex.Message}");
            }
        }

        // ===== ОБРАБОТЧИК КНОПКИ ЗАКАЗА =====

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null &&
                int.TryParse(button.Tag.ToString(), out int instrumentId))
            {
                OrderInstrument(instrumentId);
            }
        }

        private void OrderInstrument(int instrumentId)
        {
            try
            {
                if (_userId == 0)
                {
                    MessageHelper.ShowError("Ошибка: ID пользователя не установлен");
                    return;
                }

                using var context = new AppDbContext();
                var instrument = context.Instruments.FirstOrDefault(i => i.Id == instrumentId);

                if (instrument == null)
                {
                    MessageHelper.ShowError("Инструмент не найден");
                    return;
                }

                // Используем диалог для выбора количества
                var quantityDialog = new QuantityDialog(instrument);
                if (quantityDialog.ShowDialog() != true) return;

                int quantity = quantityDialog.SelectedQuantity;

                // Проверка максимального количества
                int maxQuantity = Math.Max(instrument.StockQuantity * 2, 10);
                if (quantity > maxQuantity)
                {
                    MessageHelper.ShowError($"Максимальное количество для заказа: {maxQuantity} шт.");
                    return;
                }

                var currentUser = context.Users.FirstOrDefault(u => u.ID == _userId);
                if (currentUser == null)
                {
                    MessageHelper.ShowError("Пользователь не найден");
                    return;
                }

                // Проверка существующей заявки
                var existingOrder = context.OrderRequests
                    .FirstOrDefault(o => o.InstrumentId == instrumentId &&
                                       o.RequestedById == _userId &&
                                       o.Status == "Pending");

                if (existingOrder != null)
                {
                    var result = MessageHelper.ShowQuestion(
                        $"У вас уже есть активная заявка на этот инструмент.\n" +
                        $"Текущее количество в заявке: {existingOrder.Quantity} шт.\n\n" +
                        $"Хотите добавить {quantity} шт. к существующей заявке?\n" +
                        $"(Новое количество: {existingOrder.Quantity + quantity} шт.)",
                        "Обновление заявки");

                    if (result == MessageBoxResult.Yes)
                    {
                        existingOrder.Quantity += quantity;
                        existingOrder.EstimatedPrice = instrument.Price * existingOrder.Quantity;
                        context.SaveChanges();

                        MessageHelper.ShowSuccess($"✅ Заявка обновлена!\n\n" +
                            $"Инструмент: {instrument.Brand} {instrument.Model}\n" +
                            $"Добавлено: {quantity} шт.\n" +
                            $"Теперь в заявке: {existingOrder.Quantity} шт.");
                        return;
                    }
                }

                // Создание новой заявки
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

                MessageHelper.ShowSuccess($"✅ Заявка на заказ создана!\n\n" +
                    $"Инструмент: {instrument.Brand} {instrument.Model}\n" +
                    $"Количество: {quantity} шт.\n" +
                    $"Общая стоимость: {instrument.Price * quantity} br\n" +
                    $"Цена за единицу: {instrument.Price} br\n\n" +
                    $"Заявка будет рассмотрена администратором.");
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Ошибка при создании заявки: {ex.Message}");
            }
        }

        // ===== ОБРАБОТЧИК КНОПКИ УДАЛЕНИЯ =====

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null &&
                int.TryParse(button.Tag.ToString(), out int instrumentId))
            {
                DeleteInstrument(instrumentId);
            }
        }

        private void DeleteInstrument(int instrumentId)
        {
            if (UserRole != "Администратор")
            {
                MessageHelper.ShowWarning("Только администратор может удалять инструменты");
                return;
            }

            try
            {
                using var context = new AppDbContext();
                var instrument = context.Instruments.FirstOrDefault(i => i.Id == instrumentId);

                if (instrument == null)
                {
                    MessageHelper.ShowError("Инструмент не найден");
                    return;
                }

                // Проверка активных заявок
                var activeOrders = context.OrderRequests
                    .Where(o => o.InstrumentId == instrumentId && o.Status == "Pending")
                    .ToList();

                if (activeOrders.Any())
                {
                    var result = MessageHelper.ShowQuestion(
                        $"На этот инструмент есть активные заявки в ожидании ({activeOrders.Count} шт.).\n" +
                        "Если вы удалите инструмент, эти заявки будут отклонены.\n\n" +
                        "Продолжить удаление?",
                        "Предупреждение");

                    if (result != MessageBoxResult.Yes) return;
                }

                // Подтверждение удаления
                var confirmResult = MessageHelper.ShowQuestion(
                    $"Вы уверены, что хотите удалить инструмент?\n\n" +
                    $"{instrument.Brand} {instrument.Model}\n" +
                    $"Категория: {instrument.Category}\n" +
                    $"Цена: {instrument.Price} br\n" +
                    $"В наличии: {instrument.StockQuantity} шт.",
                    "Подтверждение удаления");

                if (confirmResult != MessageBoxResult.Yes) return;

                // Удаление связанных заявок
                var ordersToDelete = context.OrderRequests
                    .Where(o => o.InstrumentId == instrumentId)
                    .ToList();

                context.OrderRequests.RemoveRange(ordersToDelete);
                context.Instruments.Remove(instrument);
                context.SaveChanges();

                MessageHelper.ShowSuccess($"✅ Инструмент успешно удален!\n\n" +
                    $"{instrument.Brand} {instrument.Model}\n" +
                    $"Удалено заявок: {ordersToDelete.Count} шт.");

                LoadInstruments();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"❌ Ошибка при удалении: {ex.Message}");
            }
        }
    }
}