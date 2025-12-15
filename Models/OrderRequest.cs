using System;

namespace MusicStoreCatalog.Models
{
    public class OrderRequest
    {
        public int Id { get; set; }
        public string DisplayPrice => $"{EstimatedPrice} br";
        // Если инструмент уже есть в каталоге
        public int? InstrumentId { get; set; }
        public Instrument Instrument { get; set; }

        // Если инструмента еще нет в каталоге
        public string InstrumentName { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Category { get; set; }

        // Данные заявки
        public int Quantity { get; set; }
        public decimal EstimatedPrice { get; set; }
        public string Notes { get; set; }

        // Кто создал заявку
        public int RequestedById { get; set; }
        public User RequestedBy { get; set; }
        public DateTime RequestDate { get; set; }

        // Статус заявки
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        // Кто подтвердил/отклонил
        public int? ApprovedById { get; set; }
        public User ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }

        // Для отображения в UI
        public string DisplayName => InstrumentId.HasValue
            ? $"{Instrument?.Brand} {Instrument?.Model}"
            : $"{Brand} {Model}";
    }
}