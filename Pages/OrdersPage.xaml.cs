using Microsoft.EntityFrameworkCore;
using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using MusicStoreCatalog.Utilities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MusicStoreCatalog.Pages
{
    public partial class OrdersPage : UserControl
    {
        private int _currentUserId;

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
            LoadPendingOrders();
            LoadProcessedOrders("Все заявки");
            UpdateTabHeaders();
        }

        public void SetCurrentUserId(int userId)
        {
            _currentUserId = userId;
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
                UpdatePendingTabHeader(pendingOrders.Count);
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Ошибка загрузки заявок: {ex.Message}");
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
                if (statusFilter == null)
                {
                    statusFilter = GetSelectedProcessedFilter();
                }

                using var context = new AppDbContext();

                // Базовый запрос
                var query = context.OrderRequests
                    .Include(o => o.Instrument)
                    .Include(o => o.RequestedBy)
                    .Where(or => or.Status == "Approved" || or.Status == "Rejected");

                // Применяем фильтр
                if (statusFilter != "Все заявки" && statusFilter != "Все")
                {
                    string status = GetStatusFromFilter(statusFilter);
                    query = query.Where(or => or.Status == status);
                }

                var processedOrders = query
                    .OrderByDescending(or => or.ApprovalDate)
                    .ToList();

                ProcessedOrdersGrid.ItemsSource = processedOrders;
                UpdateProcessedTabHeader(processedOrders.Count, statusFilter);
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Ошибка загрузки обработанных заявок: {ex.Message}");
            }
        }

        private string GetStatusFromFilter(string statusFilter)
        {
            return statusFilter switch
            {
                "Только подтвержденные" => "Approved",
                "Только отклоненные" => "Rejected",
                _ => statusFilter
            };
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
            else if (ProcessedFilterCombo.SelectedValue != null)
            {
                return ProcessedFilterCombo.SelectedValue.ToString();
            }

            return "Все заявки";
        }

        // === ОБНОВЛЕНИЕ ЗАГОЛОВКОВ ВКЛАДОК ===

        private void UpdateTabHeaders()
        {
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
            if (PendingTab != null)
            {
                string emoji = count > 0 ? "🟡" : "⚪";
                PendingTab.Header = $"{emoji} В ожидании ({count})";
                PendingTab.Foreground = count > 0 ? Brushes.DarkOrange : Brushes.Gray;
            }
        }

        private void UpdateProcessedTabHeader(int count, string filter)
        {
            // Находим вкладку "Обработанные" и обновляем заголовок
            if (ProcessedTab != null)
            {
                string filterText = (filter == "Все заявки" || filter == "Все") ? "" : $" - {filter}";
                ProcessedTab.Header = $"📋 Обработанные ({count}){filterText}";
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

                var result = MessageHelper.ShowQuestion($"Вы уверены, что хотите {actionText} заявку #{orderId}?");
                if (result != MessageBoxResult.Yes) return;

                using (var context = new AppDbContext())
                {
                    var orderToProcess = context.OrderRequests
                        .Include(o => o.Instrument)
                        .FirstOrDefault(o => o.Id == orderId);

                    if (orderToProcess == null)
                    {
                        MessageHelper.ShowError("Заявка не найдена");
                        return;
                    }

                    // Проверяем статус
                    if (orderToProcess.Status != "Pending")
                    {
                        string currentStatusText = GetStatusText(orderToProcess.Status);
                        MessageHelper.ShowInfo($"Эта заявка уже была {currentStatusText}");
                        return;
                    }

                    // Обновляем заявку
                    orderToProcess.Status = newStatus;
                    orderToProcess.ApprovalDate = DateTime.Now;
                    orderToProcess.ApprovedById = _currentUserId > 0 ? _currentUserId : (int?)null;

                    // Обработка подтвержденной заявки
                    if (newStatus == "Approved")
                    {
                        ProcessApprovedOrder(context, orderToProcess, orderId);
                    }

                    context.SaveChanges();

                    // Обновляем интерфейс
                    LoadPendingOrders();
                    LoadProcessedOrders(GetSelectedProcessedFilter());
                    UpdateTabHeaders();

                    MessageHelper.ShowSuccess($"✅ Заявка #{orderId} успешно {statusText}!\n\nЗаявка перемещена во вкладку 'Обработанные'.");
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"❌ Ошибка: {ex.Message}");
            }
        }

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
                similarOrder.Status = "Approved";
                similarOrder.ApprovalDate = DateTime.Now;
                similarOrder.Notes += $" (Объединена с заявкой #{originalOrderId})";
            }

            // Если инструмент уже есть в каталоге
            if (order.InstrumentId.HasValue)
            {
                UpdateExistingInstrument(context, order, totalQuantity, similarOrders.Count);
            }
            else
            {
                CreateNewInstrument(context, order, totalQuantity, similarOrders.Count);
            }
        }

        private void UpdateExistingInstrument(AppDbContext context, OrderRequest order, int totalQuantity, int similarOrdersCount)
        {
            var instrument = context.Instruments.FirstOrDefault(i => i.Id == order.InstrumentId.Value);
            if (instrument != null)
            {
                int oldQuantity = instrument.StockQuantity;
                instrument.StockQuantity += totalQuantity;

                string similarOrdersText = similarOrdersCount > 0
                    ? $"\n📊 Объединено с {similarOrdersCount} другими заявками (+{totalQuantity - order.Quantity} шт.)"
                    : "";

                MessageHelper.ShowInfo($"✅ Инструмент обновлен!{similarOrdersText}\n\n" +
                                     $"{instrument.Brand} {instrument.Model}\n" +
                                     $"Было: {oldQuantity} шт.\n" +
                                     $"Добавлено: {totalQuantity} шт.\n" +
                                     $"Стало: {instrument.StockQuantity} шт.");
            }
            else
            {
                MessageHelper.ShowWarning("⚠️ Инструмент не найден в каталоге");
            }
        }

        private void CreateNewInstrument(AppDbContext context, OrderRequest order, int totalQuantity, int similarOrdersCount)
        {
            var newInstrument = new Instrument
            {
                Brand = order.Brand,
                Model = order.Model,
                Category = order.Category,
                Price = order.EstimatedPrice / order.Quantity, // Цена за единицу
                StockQuantity = totalQuantity
            };

            context.Instruments.Add(newInstrument);

            string similarOrdersText = similarOrdersCount > 0
                ? $"\n📊 Объединено с {similarOrdersCount} другими заявками"
                : "";

            MessageHelper.ShowInfo($"✅ Новый инструмент добавлен!{similarOrdersText}\n\n" +
                                 $"{order.Brand} {order.Model}\n" +
                                 $"Категория: {order.Category}\n" +
                                 $"Количество: {totalQuantity} шт.\n" +
                                 $"Цена: {newInstrument.Price} br");
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

        // === ОБНОВЛЕНИЕ ИНТЕРФЕЙСА ===

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления счетчиков: {ex.Message}");
            }
        }
    }
}