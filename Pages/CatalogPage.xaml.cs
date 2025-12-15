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

            // Подключаем обработчик кнопки обновления
            RefreshBtn.Click += RefreshBtn_Click;

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
                TitleText.Text = $"?? Каталог инструментов ({specialization})";
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

                    MessageBox.Show($"? Продажа завершена!\n\n" +
                                  $"{instrument.Brand} {instrument.Model}\n" +
                                  $"Осталось в наличии: {instrument.StockQuantity} шт.",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("? Этот инструмент закончился на складе",
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
                        $"Хотите добавить еще 1 шт. к существующей заявке?",
                        "Обновление заявки",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        existingOrder.Quantity += 1;
                        context.SaveChanges();

                        MessageBox.Show($"? Заявка обновлена!\n\n" +
                                      $"Инструмент: {instrument.Brand} {instrument.Model}\n" +
                                      $"Теперь в заявке: {existingOrder.Quantity} шт.",
                                      "Успех",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                    }
                    return;
                }

                // Создаем новую заявку
                var orderRequest = new OrderRequest
                {
                    InstrumentId = instrument.Id,
                    InstrumentName = $"{instrument.Brand} {instrument.Model}",
                    Brand = instrument.Brand,
                    Model = instrument.Model,
                    Category = instrument.Category,
                    Quantity = 1,
                    EstimatedPrice = instrument.Price,
                    Notes = $"Заказ через каталог. Остаток на складе: {instrument.StockQuantity} шт.",
                    RequestedById = _userId,
                    RequestDate = DateTime.Now,
                    Status = "Pending"
                };

                context.OrderRequests.Add(orderRequest);
                context.SaveChanges();

                MessageBox.Show($"? Заявка на заказ создана!\n\n" +
                              $"Инструмент: {instrument.Brand} {instrument.Model}\n" +
                              $"Количество: 1 шт.\n" +
                              $"Цена: {instrument.Price} br\n\n" +
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

        // ===== НОВЫЙ ОБРАБОТЧИК КНОПКИ УДАЛЕНИЯ =====
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