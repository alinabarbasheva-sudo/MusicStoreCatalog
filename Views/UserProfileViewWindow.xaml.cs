using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
using System.Linq;
using System.Windows;

namespace MusicStoreCatalog.Views
{
    public partial class UserProfileViewWindow : Window
    {
        public UserProfileViewWindow(int userId)
        {
            InitializeComponent();
            LoadUserData(userId);
        }

        private void LoadUserData(int userId)
        {
            using var context = new AppDbContext();
            var user = context.Users.FirstOrDefault(u => u.ID == userId);

            if (user != null)
            {
                LoginText.Text = user.Login;
                FirstNameText.Text = user.FirstName;
                LastNameText.Text = user.LastName;
                PhoneText.Text = user.PhoneNumber;

                if (user is Admin)
                {
                    RoleText.Text = "Администратор";
                    SpecializationText.Text = "Не требуется";
                }
                else if (user is Consultant consultant)
                {
                    RoleText.Text = "Консультант";
                    SpecializationText.Text = consultant.Specialization ?? "Не указана";
                }
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}