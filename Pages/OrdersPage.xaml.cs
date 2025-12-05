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
        public OrdersPage()
        {
            InitializeComponent();

            // Назначаем обработчики
            RefreshPendingBtn.Click += RefreshPendingBtn_Click;
            RefreshProcessedBtn.Click += RefreshProcessedBtn_Click;
            ProcessedFilterCombo.SelectionChanged += ProcessedFilterCombo_SelectionChanged;

            // Загружаем данные при открытии
            LoadPendingOrders();
            LoadProcessedOrders();

            // Обновляем заголовки вкладок с количеством
            UpdateTabHeaders();
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

        private void LoadProcessedOrders(string statusFilter = "Все")
        {
            try
            {
                using var context = new AppDbContext();

                IQueryable<OrderRequest> query = context.OrderRequests
                    .Where(or => or.Status == "Approved" || or.Status == "Rejected");

                // Фильтрация по статусу
                if (statusFilter != "Все")
                {
                    string status = statusFilter == "Подтверждено" ? "Approved" : "Rejected";
                    query = query.Where(or => or.Status == status);
                }

                var processedOrders = query
                    .OrderByDescending(or => or.ApprovalDate)
                    .ToList();

                ProcessedOrdersGrid.ItemsSource = processedOrders;

                // Обновляем заголовок вкладки
                UpdateProcessedTabHeader(processedOrders.Count, statusFilter);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки обработанных заявок: {ex.Message}", "Ошибка");
            }
        }

        private void RefreshProcessedBtn_Click(object sender, RoutedEventArgs e)
        {
            string filter = GetSelectedProcessedFilter();
            LoadProcessedOrders(filter);
        }

        private void ProcessedFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string filter = GetSelectedProcessedFilter();
            LoadProcessedOrders(filter);
        }

        private string GetSelectedProcessedFilter()
        {
            if (ProcessedFilterCombo.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content.ToString();
            }
            return "Все";
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
                if (tabItem.Header.ToString().Contains("✅"))
                {
                    string filterText = filter == "Все" ? "" : $" - {filter}";
                    tabItem.Header = $"✅ Обработанные ({count}){filterText}";
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

                using var context = new AppDbContext();
                var order = context.OrderRequests.FirstOrDefault(o => o.Id == orderId);

                if (order == null)
                {
                    MessageBox.Show("Заявка не найдена", "Ошибка");
                    return;
                }

                // Проверяем, что заявка еще в статусе Pending
                if (order.Status != "Pending")
                {
                    string currentStatusText = GetStatusText(order.Status);
                    MessageBox.Show($"Эта заявка уже была {currentStatusText}",
                                  "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Обновляем статус
                order.Status = newStatus;
                order.ApprovalDate = DateTime.Now;
                // TODO: установить ApprovedById когда будет ID текущего админа

                // Если заявка подтверждена - обновляем склад
                if (newStatus == "Approved")
                {
                    if (order.InstrumentId.HasValue)
                    {
                        var instrument = context.Instruments.FirstOrDefault(i => i.Id == order.InstrumentId.Value);
                        if (instrument != null)
                        {
                            instrument.StockQuantity += order.Quantity;

                            MessageBox.Show(
                                $"✅ Инструмент обновлен!\n\n" +
                                $"{instrument.Brand} {instrument.Model}\n" +
                                $"Добавлено: {order.Quantity} шт.\n" +
                                $"Теперь в наличии: {instrument.StockQuantity} шт.",
                                "Склад обновлен",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        var newInstrument = new Instrument
                        {
                            Brand = order.Brand,
                            Model = order.Model,
                            Category = order.Category,
                            Price = order.EstimatedPrice,
                            StockQuantity = order.Quantity,
                            Description = string.IsNullOrEmpty(order.Notes)
                                ? $"Добавлено по заявке #{orderId}"
                                : order.Notes,
                            SerialNumber = $"ORDER-{orderId}"
                        };
                        context.Instruments.Add(newInstrument);

                        MessageBox.Show(
                            $"✅ Новый инструмент добавлен!\n\n" +
                            $"{order.Brand} {order.Model}\n" +
                            $"Количество: {order.Quantity} шт.\n" +
                            $"Цена: {order.EstimatedPrice:C}",
                            "Новый инструмент",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }

                context.SaveChanges();

                // ОБНОВЛЯЕМ ОБЕ ТАБЛИЦЫ И ЗАГОЛОВКИ
                LoadPendingOrders();
                LoadProcessedOrders(GetSelectedProcessedFilter());
                UpdateTabHeaders();

                MessageBox.Show($"✅ Заявка #{orderId} успешно {statusText}!\n\nЗаявка перемещена во вкладку 'Обработанные'.",
                              "Успех",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка");
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