using System;

namespace MusicStoreCatalog.Models
{
    public class Instrument
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Description { get; set; } = "";// Добавьте это поле обратно
        public string SerialNumber { get; set; } = "";// Добавьте это поле обратно
        // Для отображения в UI
        public string DisplayName => $"{Brand} {Model}";
    }
}