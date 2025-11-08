using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStoreCatalog.Models
{
    internal class Schedule
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsWorking { get; set; }


    }
}

