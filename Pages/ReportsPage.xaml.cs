using Microsoft.EntityFrameworkCore;
using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MusicStoreCatalog.Pages
{
    public partial class ReportsPage : UserControl
    {
        // Классы для хранения данных
        public class SalesByConsultant
        {
            public string ConsultantName { get; set; }
            public string Specialization { get; set; }
            public int SalesCount { get; set; }
            public int Rating => SalesCount switch
            {
                >= 40 => 5,
                >= 30 => 4,
                >= 20 => 3,
                >= 10 => 2,
                >= 5 => 1,
                _ => 0
            };
        }

        public ReportsPage()
        {
            InitializeComponent();

            // Устанавливаем дату отчета
            if (ReportDateText != null)
            {
                ReportDateText.Text = $"Отчет на {DateTime.Now:dd.MM.yyyy HH:mm}";
            }

            // Загружаем данные после полной инициализации UI
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    LoadAllReports();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при загрузке отчетов: {ex.Message}");
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void LoadAllReports()
        {
            try
            {
                using var context = new AppDbContext();

                // Проверяем подключение к базе
                if (!context.Database.CanConnect())
                {
                    MessageBox.Show("Нет подключения к базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 1. Загружаем ключевые метрики
                LoadKeyMetrics(context);

                // 2. Загружаем продажи по консультантам
                LoadSalesByConsultant(context);

                // 3. Загружаем статистику заявок
                LoadOrderStatistics(context);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить отчеты: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void LoadKeyMetrics(AppDbContext context)
        {
            try
            {
                // 1. Общая стоимость склада
                decimal totalStockValue = 0;
                if (context.Instruments != null && context.Instruments.Any())
                {
                    totalStockValue = context.Instruments.Sum(i => i.Price * i.StockQuantity);
                }

                if (TotalStockValue != null)
                {
                    TotalStockValue.Text = $"{totalStockValue:N0} br";
                }

                // 2. Всего инструментов
                int totalInstruments = context.Instruments?.Count() ?? 0;
                if (TotalInstruments != null)
                {
                    TotalInstruments.Text = totalInstruments.ToString();
                }

                // 3. Инструменты по категориям
                string categoriesText = "нет данных";
                if (context.Instruments != null && context.Instruments.Any())
                {
                    var categories = context.Instruments
                        .Where(i => !string.IsNullOrEmpty(i.Category))
                        .GroupBy(i => i.Category)
                        .Select(g => new { Category = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .ToList();

                    if (categories.Any())
                    {
                        categoriesText = string.Join(", ",
                            categories.Select(c => $"{c.Category}: {c.Count}"));

                        if (categoriesText.Length > 50)
                            categoriesText = categoriesText.Substring(0, 47) + "...";
                    }
                }

                if (InstrumentsByCategory != null)
                {
                    InstrumentsByCategory.Text = categoriesText;
                }

                // 4. Активные консультанты
                var consultants = context.Users?.OfType<Consultant>();
                int activeConsultants = consultants?.Count() ?? 0;

                if (ActiveConsultants != null)
                {
                    ActiveConsultants.Text = activeConsultants.ToString();
                }

                // 5. Лучший консультант
                string topConsultantText = "Лучший: нет данных";
                if (consultants != null && consultants.Any())
                {
                    var topConsultant = consultants
                        .Where(c => c != null && !string.IsNullOrEmpty(c.FirstName))
                        .OrderByDescending(c => c.SalesCount)
                        .FirstOrDefault();

                    if (topConsultant != null)
                    {
                        topConsultantText = $"Лучший: {topConsultant.FirstName} {topConsultant.LastName} ({topConsultant.SalesCount} продаж)";
                    }
                }

                if (TopConsultant != null)
                {
                    TopConsultant.Text = topConsultantText;
                }

                // 6. Активные заявки
                int pendingOrders = context.OrderRequests?.Count(o => o.Status == "Pending") ?? 0;

                if (PendingOrders != null)
                {
                    PendingOrders.Text = pendingOrders.ToString();
                }

                int approvedOrders = context.OrderRequests?.Count(o => o.Status == "Approved") ?? 0;
                int rejectedOrders = context.OrderRequests?.Count(o => o.Status == "Rejected") ?? 0;

                if (OrdersStatus != null)
                {
                    OrdersStatus.Text = $"{approvedOrders} подтверждено, {rejectedOrders} отклонено";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в LoadKeyMetrics: {ex.Message}");
            }
        }

        private void LoadSalesByConsultant(AppDbContext context)
        {
            try
            {
                if (SalesByConsultantList == null) return;

                var consultants = context.Users?.OfType<Consultant>();

                if (consultants == null || !consultants.Any())
                {
                    SalesByConsultantList.ItemsSource = new List<SalesByConsultant>();
                    return;
                }

                var salesData = consultants
                    .Where(c => c != null && !string.IsNullOrEmpty(c.FirstName))
                    .OrderByDescending(c => c.SalesCount)
                    .Select(c => new SalesByConsultant
                    {
                        ConsultantName = $"{c.FirstName} {c.LastName}",
                        Specialization = c.Specialization ?? "Не указана",
                        SalesCount = c.SalesCount
                    })
                    .ToList();

                SalesByConsultantList.ItemsSource = salesData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки продаж: {ex.Message}");
                if (SalesByConsultantList != null)
                {
                    SalesByConsultantList.ItemsSource = new List<SalesByConsultant>();
                }
            }
        }

        private void LoadOrderStatistics(AppDbContext context)
        {
            try
            {
                // Статистика по заявкам
                var pendingCount = context.OrderRequests?.Count(o => o.Status == "Pending") ?? 0;
                var approvedCount = context.OrderRequests?.Count(o => o.Status == "Approved") ?? 0;
                var rejectedCount = context.OrderRequests?.Count(o => o.Status == "Rejected") ?? 0;

                if (PendingCount != null) PendingCount.Text = pendingCount.ToString();
                if (ApprovedCount != null) ApprovedCount.Text = approvedCount.ToString();
                if (RejectedCount != null) RejectedCount.Text = rejectedCount.ToString();

                // Общее количество подтвержденных инструментов
                var approvedQuantity = context.OrderRequests?
                    .Where(o => o.Status == "Approved")
                    .Sum(o => o.Quantity) ?? 0;

                if (ApprovedQuantity != null)
                {
                    ApprovedQuantity.Text = $"{approvedQuantity} шт. инструментов";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки статистики заявок: {ex.Message}");
            }
        }

        // ===== ОБРАБОТЧИКИ СОБЫТИЙ =====

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadAllReports();

            if (ReportDateText != null)
            {
                ReportDateText.Text = $"Отчет на {DateTime.Now:dd.MM.yyyy HH:mm}";
            }

            MessageBox.Show("Данные отчетов обновлены!",
                          "Обновление",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }

        private void PeriodCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAllReports();
        }
    }

    // Конвертер для рейтинга
    public class RatingToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int rating && parameter is string paramString)
            {
                if (int.TryParse(paramString, out int starNumber))
                {
                    return rating >= starNumber ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}