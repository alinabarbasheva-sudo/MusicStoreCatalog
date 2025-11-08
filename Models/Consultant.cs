using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStoreCatalog.Models
{
    public class Consultant : User
    {
        public string specialization { get; set; }
        public int salecount { get; set; }
    }
}
