using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using MusicStoreCatalog.Views;

namespace MusicStoreCatalog.Services
{
    public static class SeedService
    {
        public static bool IsFirstRun()
        {
            using var context = new AppDbContext();
            return !context.Users.Any(u => u is Admin);
        }

        public static void CreateFirstAdmin(string login, string password)
        {
            using var context = new AppDbContext();

            // Создаем админа только если его нет
            if (!context.Users.Any(u => u.Login == login))
            {
                var admin = new Admin
                {
                    Login = login,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    FirstName = "Admin",
                    LastName = "Admin",
                    PhoneNumber = "00000000000"
                };
                context.Users.Add(admin);
                context.SaveChanges();
            }

            // Создаем инструменты только если их нет
            if (!context.Instruments.Any())
            {
                var instruments = new List<Instrument>
        {
            new Instrument {
                Brand = "Yamaha", Model = "F310", Category = "Гитара",
                Price = 15000, StockQuantity = 5,
                Description = "Акустическая гитара", SerialNumber = "YAM001"
            },
            new Instrument {
                Brand = "Casio", Model = "CT-S100", Category = "Синтезатор",
                Price = 25000, StockQuantity = 3,
                Description = "Цифровой синтезатор", SerialNumber = "CAS001"
            }
        };
                context.Instruments.AddRange(instruments);
                context.SaveChanges();
            }
        }
    }
    }
