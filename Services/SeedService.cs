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
                    PhoneNumber = "-"
                };
                context.Users.Add(admin);
                context.SaveChanges();
            }

        }
    }
}