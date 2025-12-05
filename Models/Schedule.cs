using System;

namespace MusicStoreCatalog.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } // ← ЭТО свойство должно быть!
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsWorking { get; set; }
    }
}