using System.Configuration;
using System.Data;
using System.Windows;
using MusicStoreCatalog.Services;
using Microsoft.EntityFrameworkCore; 
using MusicStoreCatalog.Data; 

namespace MusicStoreCatalog
{
    public partial class App : Application
    {

protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Настройка БД
            SetupDatabase();
        }

        private void SetupDatabase()
        {
            try
            {
                using var context = new AppDbContext();

                // Проверяем есть ли миграции
                var pendingMigrations = context.Database.GetPendingMigrations().ToList();

                if (pendingMigrations.Any())
                {
                    Console.WriteLine($"Применяем миграции: {string.Join(", ", pendingMigrations)}");
                    context.Database.Migrate();
                }
                else
                {
                    // Если нет миграций, просто создаем БД
                    context.Database.EnsureCreated();
                }

                // Проверяем первый запуск
                if (SeedService.IsFirstRun())
                {
                    SeedService.CreateFirstAdmin("admin", "admin");
                    Console.WriteLine("Создан первый администратор");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка настройки БД: {ex.Message}\nПриложение может работать некорректно.",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                Console.WriteLine($"Ошибка настройки БД: {ex}");
            }
        }
    
    }
}