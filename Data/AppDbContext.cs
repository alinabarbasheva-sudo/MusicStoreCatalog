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
    public class AppDbContext : DbContext
    {
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Consultant> Consultants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Instrument> Instruments { get; set; }
        public DbSet<OrderRequest> OrderRequests { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string projectPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string dbPath = Path.Combine(projectPath, "MusicStore.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


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
    }
}