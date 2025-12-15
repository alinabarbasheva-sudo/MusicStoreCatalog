using Microsoft.EntityFrameworkCore;
using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MusicStoreCatalog.Pages
{
    public partial class OrdersPage : UserControl
    {
        private int _currentUserId; // Добавьте это поле

        public OrdersPage()
        {
            InitializeComponent();

            // Назначаем обработчики
            RefreshPendingBtn.Click += RefreshPendingBtn_Click;
            RefreshProcessedBtn.Click += RefreshProcessedBtn_Click;
            ProcessedFilterCombo.SelectionChanged += ProcessedFilterCombo_SelectionChanged;

            // Устанавливаем начальный выбор
            ProcessedFilterCombo.SelectedIndex = 0;

            // Загружаем данные при открытии
            Loaded += OrdersPage_Loaded;
        }

        private void OrdersPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Загружаем данные
            LoadPendingOrders();
            LoadProcessedOrders("Все заявки"); // Явно указываем начальный фильтр
            UpdateTabHeaders();
        }

        // Добавьте этот метод для установки UserId
        public void SetCurrentUserId(int userId)
        {
            _currentUserId = userId;
        }

        // Обновите GetCurrentUserId:
        private int? GetCurrentUserId()
        {
            return _currentUserId > 0 ? _currentUserId : (int?)null;
        }

        // === ЗАЯВКИ В ОЖИДАНИИ ===

        private void LoadPendingOrders()
        {
            try
            {
                using var context = new AppDbContext();

                var pendingOrders = context.OrderRequests
                    .Where(or => or.Status == "Pending")
                    .OrderByDescending(or => or.RequestDate)
                    .ToList();

                PendingOrdersGrid.ItemsSource = pendingOrders;

                // Обновляем заголовок вкладки
                UpdatePendingTabHeader(pendingOrders.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка");
            }
        }

        private void RefreshPendingBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadPendingOrders();
        }

        // === ОБРАБОТАННЫЕ ЗАЯВКИ ===

        private void LoadProcessedOrders(string statusFilter = null)
        {
            try
            {
                Console.WriteLine($"=== Начало загрузки обработанных заявок ===");

                // Если фильтр не передан, берем текущий выбранный
                if (statusFilter == null)
                {
                    statusFilter = GetSelectedProcessedFilter();
                }

                Console.WriteLine($"Фильтр: {statusFilter}");

                using var context = new AppDbContext();

                // Выводим статистику по заявкам
                var allOrders = context.OrderRequests.ToList();
                Console.WriteLine($"Всего заявок в базе: {allOrders.Count}");
                Console.WriteLine($"Pending: {allOrders.Count(o => o.Status == "Pending")}");
                Console.WriteLine($"Approved: {allOrders.Count(o => o.Status == "Approved")}");
                Console.WriteLine($"Rejected: {allOrders.Count(o => o.Status == "Rejected")}");

                // Начинаем запрос с базовой фильтрации
                var query = context.OrderRequests
                    .Include(o => o.Instrument)
                    .Include(o => o.RequestedBy)
                    .Where(or => or.Status == "Approved" || or.Status == "Rejected");

                Console.WriteLine($"Базовый запрос (Approved или Rejected): {query.Count()}");

                // Применяем дополнительную фильтрацию, если выбрано не "Все"
                if (statusFilter != "Все заявки" && statusFilter != "Все")
                {
                    string status = statusFilter switch
                    {
                        "Только подтвержденные" => "Approved",
                        "Только отклоненные" => "Rejected",
                        _ => statusFilter
                    };
                    Console.WriteLine($"Применяем фильтр статуса: {status}");
                    query = query.Where(or => or.Status == status);
                    Console.WriteLine($"После фильтрации: {query.Count()}");
                }

                var processedOrders = query
                    .OrderByDescending(or => or.ApprovalDate)
                    .ToList();

                Console.WriteLine($"Загружено заявок для отображения: {processedOrders.Count}");

                ProcessedOrdersGrid.ItemsSource = processedOrders;

                // Обновляем заголовок вкладки
                UpdateProcessedTabHeader(processedOrders.Count, statusFilter);

                Console.WriteLine($"=== Конец загрузки обработанных заявок ===\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки обработанных заявок: {ex.Message}", "Ошибка");
                Console.WriteLine($"Ошибка: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void RefreshProcessedBtn_Click(object sender, RoutedEventArgs e)
        {
            string filter = GetSelectedProcessedFilter();
            LoadProcessedOrders(filter);
        }

        private void ProcessedFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Загружаем заявки с текущим фильтром
            string filter = GetSelectedProcessedFilter();
            LoadProcessedOrders(filter);
        }

        private string GetSelectedProcessedFilter()
        {
            // Получаем выбранный элемент
            if (ProcessedFilterCombo.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content.ToString();
            }
            else if (ProcessedFilterCombo.SelectedValue != null)
            {
                return ProcessedFilterCombo.SelectedValue.ToString();
            }

            // Возвращаем значение по умолчанию
            return "Все заявки";
        }

        // === ОБНОВЛЕНИЕ ЗАГОЛОВКОВ ВКЛАДОК ===

        private void UpdateTabHeaders()
        {
            // Этот метод вызывается после загрузки данных
            using var context = new AppDbContext();

            // Считаем заявки в ожидании
            var pendingCount = context.OrderRequests.Count(or => or.Status == "Pending");
            UpdatePendingTabHeader(pendingCount);

            // Считаем обработанные заявки
            var processedCount = context.OrderRequests.Count(or => or.Status == "Approved" || or.Status == "Rejected");
            UpdateProcessedTabHeader(processedCount, "Все");
        }

        private void UpdatePendingTabHeader(int count)
        {
            // Находим вкладку "В ожидании" и обновляем заголовок
            foreach (TabItem tabItem in ((TabControl)((Grid)Content).Children[1]).Items)
            {
                if (tabItem.Header.ToString().Contains("🟡"))
                {
                    string emoji = count > 0 ? "🟡" : "⚪";
                    tabItem.Header = $"{emoji} В ожидании ({count})";

                    // Меняем цвет иконки если есть заявки
                    if (count > 0)
                    {
                        tabItem.Foreground = System.Windows.Media.Brushes.DarkOrange;
                    }
                    else
                    {
                        tabItem.Foreground = System.Windows.Media.Brushes.Gray;
                    }
                    break;
                }
            }
        }

        private void UpdateProcessedTabHeader(int count, string filter)
        {
            // Находим вкладку "Обработанные" и обновляем заголовок
            foreach (TabItem tabItem in ((TabControl)((Grid)Content).Children[1]).Items)
            {
                if (tabItem.Name == "ProcessedTab" || tabItem.Header.ToString().Contains("Обработанные"))
                {
                    string filterText = filter == "Все заявки" || filter == "Все" ? "" : $" - {filter}";
                    tabItem.Header = $"📋 Обработанные ({count}){filterText}";
                    break;
                }
            }
        }

        // === ОБРАБОТКА ЗАЯВОК ===

        public void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null && int.TryParse(button.Tag.ToString(), out int orderId))
            {
                ProcessOrder(orderId, "Approved");
            }
        }

        public void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null && int.TryParse(button.Tag.ToString(), out int orderId))
            {
                ProcessOrder(orderId, "Rejected");
            }
        }

        private void ProcessOrder(int orderId, string newStatus)
        {
            try
            {
                string actionText = newStatus == "Approved" ? "подтвердить" : "отклонить";
                string statusText = newStatus == "Approved" ? "подтверждена" : "отклонена";

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите {actionText} заявку #{orderId}?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    newStatus == "Approved" ? MessageBoxImage.Question : MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;

                using (var context = new AppDbContext())
                {
                    var orderToProcess = context.OrderRequests
                        .Include(o => o.Instrument) // ВАЖНО: включаем связанные данные
                        .FirstOrDefault(o => o.Id == orderId);

                    if (orderToProcess == null)
                    {
                        MessageBox.Show("Заявка не найдена", "Ошибка");
                        return;
                    }

                    // Проверяем, что заявка еще в статусе Pending
                    if (orderToProcess.Status != "Pending")
                    {
                        string currentStatusText = GetStatusText(orderToProcess.Status);
                        MessageBox.Show($"Эта заявка уже была {currentStatusText}",
                                      "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Обновляем статус
                    orderToProcess.Status = newStatus;
                    orderToProcess.ApprovalDate = DateTime.Now;
                    orderToProcess.ApprovedById = _currentUserId > 0 ? _currentUserId : (int?)null; // Прямое использование

                    // Если заявка подтверждена - обновляем склад
                    if (newStatus == "Approved")
                    {
                        ProcessApprovedOrder(context, orderToProcess, orderId);
                    }

                    context.SaveChanges();

                    // Обновляем обе таблицы
                    LoadPendingOrders();
                    LoadProcessedOrders(GetSelectedProcessedFilter());
                    UpdateTabHeaders();

                    MessageBox.Show($"✅ Заявка #{orderId} успешно {statusText}!\n\nЗаявка перемещена во вкладку 'Обработанные'.",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка");
            }
        }

        // ===== НОВЫЙ МЕТОД ДЛЯ ОБРАБОТКИ ПОДТВЕРЖДЕННЫХ ЗАЯВОК =====
        private void ProcessApprovedOrder(AppDbContext context, OrderRequest order, int originalOrderId)
        {
            // Находим все заявки на этот же инструмент с тем же статусом Pending
            var similarOrders = context.OrderRequests
                .Where(o => o.InstrumentId == order.InstrumentId &&
                           o.Status == "Pending" &&
                           o.Id != originalOrderId)
                .ToList();

            // Суммируем количества
            int totalQuantity = order.Quantity;
            foreach (var similarOrder in similarOrders)
            {
                totalQuantity += similarOrder.Quantity;
                // Помечаем похожие заявки как обработанные
                similarOrder.Status = "Approved";
                similarOrder.ApprovalDate = DateTime.Now;
                similarOrder.Notes += $" (Объединена с заявкой #{originalOrderId})";
            }

            if (order.InstrumentId.HasValue)
            {
                var instrument = context.Instruments.FirstOrDefault(i => i.Id == order.InstrumentId.Value);
                if (instrument != null)
                {
                    int oldQuantity = instrument.StockQuantity;
                    instrument.StockQuantity += totalQuantity;

                    string similarOrdersText = similarOrders.Count > 0
                        ? $"\n📊 Объединено с {similarOrders.Count} другими заявками (+{similarOrders.Sum(o => o.Quantity)} шт.)"
                        : "";

                    MessageBox.Show(
                        $"✅ Инструмент обновлен!{similarOrdersText}\n\n" +
                        $"{instrument.Brand} {instrument.Model}\n" +
                        $"Было: {oldQuantity} шт.\n" +
                        $"Добавлено: {totalQuantity} шт.\n" +
                        $"Стало: {instrument.StockQuantity} шт.",
                        "Склад обновлен",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("⚠️ Инструмент не найден в каталоге", "Предупреждение");
                }
            }
            else
            {
                // Создаем новый инструмент если его нет в каталоге
                var newInstrument = new Instrument
                {
                    Brand = order.Brand,
                    Model = order.Model,
                    Category = order.Category,
                    Price = order.EstimatedPrice,
                    StockQuantity = totalQuantity
                };
                context.Instruments.Add(newInstrument);

                string similarOrdersText = similarOrders.Count > 0
                    ? $"\n📊 Объединено с {similarOrders.Count} другими заявками"
                    : "";

                MessageBox.Show(
                    $"✅ Новый инструмент добавлен!{similarOrdersText}\n\n" +
                    $"{order.Brand} {order.Model}\n" +
                    $"Категория: {order.Category}\n" +
                    $"Количество: {totalQuantity} шт.\n" +
                    $"Цена: {order.EstimatedPrice} br",
                    "Новый инструмент",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private string GetStatusText(string status)
        {
            return status switch
            {
                "Pending" => "в ожидании",
                "Approved" => "подтверждена",
                "Rejected" => "отклонена",
                _ => status
            };
        }


        // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ ОБНОВЛЕНИЯ ИНТЕРФЕЙСА ===

        private void UpdateOrderCounts()
        {
            try
            {
                using var context = new AppDbContext();

                // Получаем актуальные счетчики
                var pendingCount = context.OrderRequests.Count(or => or.Status == "Pending");
                var approvedCount = context.OrderRequests.Count(or => or.Status == "Approved");
                var rejectedCount = context.OrderRequests.Count(or => or.Status == "Rejected");

                // Обновляем заголовки вкладок
                UpdatePendingTabHeader(pendingCount);
                UpdateProcessedTabHeader(approvedCount + rejectedCount, GetSelectedProcessedFilter());

                // Показываем общую статистику в заголовке окна
                UpdateWindowTitle(pendingCount, approvedCount, rejectedCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления счетчиков: {ex.Message}");
            }
        }

        private void UpdateWindowTitle(int pendingCount, int approvedCount, int rejectedCount)
        {
            // Находим родительское окно и обновляем заголовок
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Title = $"Управление заявками | ⏳{pendingCount} ✅{approvedCount} ❌{rejectedCount}";
            }
        }

        // Метод для обновления цвета кнопок в зависимости от статуса
        private void UpdateButtonColors()
        {
            // Этот метод можно вызвать после загрузки данных
            // чтобы настроить цвета кнопок в зависимости от контекста
            foreach (var item in PendingOrdersGrid.Items)
            {
                if (PendingOrdersGrid.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row)
                {
                    var buttonsPanel = FindVisualChild<StackPanel>(row);
                    if (buttonsPanel != null)
                    {
                        foreach (Button button in buttonsPanel.Children)
                        {
                            // Можно настроить дополнительные стили
                            if (button.Content.ToString().Contains("Подтвердить"))
                            {
                                button.ToolTip = "Подтвердить заявку и добавить товар на склад";
                            }
                            else if (button.Content.ToString().Contains("Отклонить"))
                            {
                                button.ToolTip = "Отклонить заявку без изменений на складе";
                            }
                        }
                    }
                }
            }
        }

        // Вспомогательный метод для поиска дочерних элементов
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var childResult = FindVisualChild<T>(child);
                if (childResult != null)
                    return childResult;
            }
            return null;
        }
    }
}