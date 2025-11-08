using Microsoft.EntityFrameworkCore;
using MusicStoreCatalog.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStoreCatalog.Data
{
    internal class AppDbContext : DbContext
    {
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Consultant> Consultants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Instrument> Instruments { get; set; }
        public DbSet<Schedule> Shedules { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string projectPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string dbPath = Path.Combine(projectPath, "MusicStore.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}
