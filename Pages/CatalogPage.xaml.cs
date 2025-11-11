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
using MusicStoreCatalog.Models;
namespace MusicStoreCatalog.Pages
{
    /// <summary>
    /// Логика взаимодействия для CatalogPage.xaml
    /// </summary>
   

    public partial class CatalogPage : UserControl
    {
        public string UserRole { get; set; }
        public CatalogPage()
        {
            InitializeComponent();
            RefreshBtn.Click += (s, e) => LoadInstruments();
        }
        public void SetUserRole(string role)
        {
            UserRole = role;
            SellColumn.Visibility = UserRole == "Консультант" ? Visibility.Visible : Visibility.Collapsed;
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


        private void SellButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null && int.TryParse(button.Tag.ToString(), out int instrumentId))
            {
                using var context = new AppDbContext();
                var instruments = context.Instruments.FirstOrDefault(i => i.Id == instrumentId);
                if (instruments != null && instruments.StockQuantity > 0)
                {
                    instruments.StockQuantity -= 1;
                    context.SaveChanges();
                    LoadInstruments();
                }
                else return;

    
            }
        }
    }
}
