using System;

namespace MusicStoreCatalog.Models
{
    public class Consultant : User
    {
        public string Specialization { get; set; }
        public int SalesCount { get; set; }

        // Свойство для расчета рейтинга
        public int Rating
        {
            get
            {
                return SalesCount switch
                {
                    >= 50 => 5,
                    >= 40 => 4,
                    >= 30 => 3,
                    >= 20 => 2,
                    >= 10 => 1,
                    _ => 0
                };
            }
        }

        // Свойство для отображения рейтинга текстом
        public string RatingText
        {
            get
            {
                return Rating switch
                {
                    5 => "⭐⭐⭐⭐⭐",
                    4 => "⭐⭐⭐⭐☆",
                    3 => "⭐⭐⭐☆☆",
                    2 => "⭐⭐☆☆☆",
                    1 => "⭐☆☆☆☆",
                    _ => "☆☆☆☆☆"
                };
            }
        }
    }
}