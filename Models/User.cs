using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//базовая структура для любого пользователя 
namespace MusicStoreCatalog.Models
{
    public abstract class User
    {
        public int ID { get; set; } //уникальный идентификатор
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }

    }
}
