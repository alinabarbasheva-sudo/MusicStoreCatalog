using Microsoft.EntityFrameworkCore;
using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using MusicStoreCatalog.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Price = 15000, StockQuantity = 5,  // цена в br
        Description = "Акустическая гитара", SerialNumber = "YAM001"
    },
    new Instrument {
        Brand = "Casio", Model = "CT-S100", Category = "Синтезатор",
        Price = 25000, StockQuantity = 3,  // цена в br
        Description = "Цифровой синтезатор", SerialNumber = "CAS001"
    }
};
                context.Instruments.AddRange(instruments);
                context.SaveChanges();
            }
            if (!context.Users.Any(u => u.Login == "consultant"))
            {
                var consultant = new Consultant
                {
                    Login = "consultant",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("consultant"),
                    FirstName = "Тестовый",
                    LastName = "Консультант",
                    PhoneNumber = "11111111111",
                    Specialization = "Гитары",
                    SalesCount = 0
                };
                context.Users.Add(consultant);
                context.SaveChanges();
            }
        }
       
    }
    }
