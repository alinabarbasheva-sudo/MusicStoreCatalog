using MusicStoreCatalog.Data;
using MusicStoreCatalog.Models;
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
namespace MusicStoreCatalog.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            User user = null;
            MainWindow mainWindow = null;
            string UserRole = "";
            string UserSpecialization = ""; // Добавляем

            using (var context = new AppDbContext())
            {
                user = context.Users.FirstOrDefault(u => u.Login == UsernameBox.Text);
            }

            if (user != null)
            {
                if (BCrypt.Net.BCrypt.Verify(PasswordBox.Text, user.PasswordHash))
                {
                    if (user is Admin)
                    {
                        UserRole = "Администратор";
                        UserSpecialization = ""; // У админа нет специализации
                    }
                    else if (user is Consultant consultant)
                    {
                        UserRole = "Консультант";
                        UserSpecialization = consultant.Specialization; // Сохраняем специализацию
                    }

                    mainWindow = new MainWindow();
                    mainWindow.UserLogin = UsernameBox.Text;
                    mainWindow.UserRole = UserRole;
                    mainWindow.UserId = user.ID;
                    mainWindow.UserSpecialization = UserSpecialization; // Передаем специализацию
                    mainWindow.FunWelcomeText();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неправильный логин или пароль");
                }
            }
            else
            {
                MessageBox.Show("Неправильный логин или пароль");
            }
        }
    }
}
