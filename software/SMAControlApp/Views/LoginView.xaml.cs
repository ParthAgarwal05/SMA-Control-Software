using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SMAControlApp.Data;
using System.Collections.ObjectModel;

namespace SMAControlApp.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Clear previous error
            lblError.Text = "";

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            using (var db = new AppDbContext())
            {
                var user = db.Users
                             .Include(u => u.Config)
                             .Include(u => u.Actuators)
                             .FirstOrDefault(u => u.UserName == username && u.PasswordHash == password);

                if (user != null)
                {
                    App.CurrentUser = user;
                    App.Config = user.Config;
                    App.Actuators = new ObservableCollection<Models.ActuatorChannel>(
                        user.Actuators.OrderBy(a => a.ChannelId));
                    OnLoginSuccess();
                }
                else
                {
                    lblError.Text = "Invalid username or password.";
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
        }

        private void OnLoginSuccess()
        {
            MessageBox.Show("Login Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                App.GraphVM = new ViewModels.GraphViewModel();
                mainWindow.LoginArea.Visibility = Visibility.Collapsed;
                mainWindow.DashboardLayout.Visibility = Visibility.Visible;
                mainWindow.MainContent.Content = new OpenLoopView();
            }
        }
    }
}