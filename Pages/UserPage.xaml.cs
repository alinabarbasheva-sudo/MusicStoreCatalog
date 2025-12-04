using System.Windows;
using System.Windows.Controls;
using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using System.Linq;
using MusicStoreCatalog.Views;

namespace MusicStoreCatalog.Pages
{
    public partial class UsersPage : UserControl
    {
        public UsersPage()
        {
            InitializeComponent();
            LoadUsers();
            AddUserBtn.Click += AddUserBtn_Click;
        }

        private void LoadUsers()
        {
            using var context = new AppDbContext();
            var consultants = context.Users.OfType<Consultant>().ToList();
            UsersGrid.ItemsSource = consultants;
        }

        private void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddConsultantWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadUsers(); // Обновить список
            }
        }
    }
}