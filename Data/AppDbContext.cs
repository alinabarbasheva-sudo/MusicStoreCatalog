using Microsoft.EntityFrameworkCore;
using MusicStoreCatalog.Models;
using System.IO;

namespace MusicStoreCatalog.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Consultant> Consultants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Instrument> Instruments { get; set; }
        public DbSet<OrderRequest> OrderRequests { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = GetDatabasePath();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            // Включаем подробные логи для отладки
            optionsBuilder.LogTo(System.Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            optionsBuilder.EnableSensitiveDataLogging();
        }

        private string GetDatabasePath()
        {
            // Пробуем несколько путей
            string[] possiblePaths =
            {
                Path.Combine(Directory.GetCurrentDirectory(), "MusicStore.db"),
                Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "MusicStore.db"),
                "MusicStore.db"
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path) || Directory.Exists(Path.GetDirectoryName(path)))
                {
                    return path;
                }
            }

            // Если ничего не нашли, создаем в текущей директории
            return "MusicStore.db";
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка TPH (Table Per Hierarchy) для наследования
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<Admin>("Admin")
                .HasValue<Consultant>("Consultant");

            // Настройка отношения OrderRequest -> RequestedBy
            modelBuilder.Entity<OrderRequest>()
                .HasOne(or => or.RequestedBy)
                .WithMany()
                .HasForeignKey(or => or.RequestedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройка отношения OrderRequest -> ApprovedBy
            modelBuilder.Entity<OrderRequest>()
                .HasOne(or => or.ApprovedBy)
                .WithMany()
                .HasForeignKey(or => or.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройка отношения OrderRequest -> Instrument
            modelBuilder.Entity<OrderRequest>()
                .HasOne(or => or.Instrument)
                .WithMany()
                .HasForeignKey(or => or.InstrumentId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        // Метод для гарантированного создания БД
        public void EnsureDatabaseCreated()
        {
            try
            {
                Console.WriteLine($"Создаем/проверяем БД по пути: {GetDatabasePath()}");

                // Проверяем, существует ли файл БД
                string dbPath = GetDatabasePath();
                bool dbExists = File.Exists(dbPath);

                Console.WriteLine($"Файл БД существует: {dbExists}");

                // Создаем БД если нужно
                Database.EnsureCreated();

                Console.WriteLine($"Проверка создания таблиц...");
                Console.WriteLine($"Таблица Users существует: {Users != null}");

                if (!dbExists)
                {
                    Console.WriteLine("База данных создана впервые");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании БД: {ex.Message}");
                throw;
            }
        }
    }
}