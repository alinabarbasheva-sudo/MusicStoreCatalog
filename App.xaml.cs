using System.Configuration;
using System.Data;
using System.Windows;
using MusicStoreCatalog.Services;
using Microsoft.EntityFrameworkCore; 
using MusicStoreCatalog.Data; 

namespace MusicStoreCatalog
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }
}