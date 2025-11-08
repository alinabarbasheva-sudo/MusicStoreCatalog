using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MusicStoreCatalog.Data;
namespace MusicStoreCatalog.Pages
{
    /// <summary>
    /// Логика взаимодействия для CatalogPage.xaml
    /// </summary>

    public partial class CatalogPage : UserControl
    {
        public CatalogPage()
        {
            InitializeComponent();
            RefreshBtn.Click += (s, e) => LoadInstruments();
        }
        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadInstruments(); 
        }
        private void LoadInstruments()
        {
            using var context = new AppDbContext();
            InstrumentsGrid.ItemsSource = context.Instruments.ToList();
        }
    }
}
