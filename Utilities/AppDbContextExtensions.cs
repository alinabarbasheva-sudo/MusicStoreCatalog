using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using System.Linq;

namespace MusicStoreCatalog.Utilities
{
    public static class AppDbContextExtensions
    {
        public static Consultant GetConsultantById(this AppDbContext context, int userId)
        {
            return context.Users.OfType<Consultant>().FirstOrDefault(c => c.ID == userId);
        }

        public static bool InstrumentExists(this AppDbContext context, string brand, string model, string category)
        {
            return context.Instruments.Any(i =>
                i.Brand == brand &&
                i.Model == model &&
                i.Category == category);
        }
    }
}
