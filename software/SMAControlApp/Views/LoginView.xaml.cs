using System.Windows;
using System.Windows.Controls;

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

            string username = txtUsername.Text;
            string password = txtPassword.Password;

            // Hardcoded check for "User1" and "admin123"
            if (username == "User1" && password == "admin123")
            {
                OnLoginSuccess();
            }
            else
            {
                lblError.Text = "Invalid username or password.";
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        private void OnLoginSuccess()
        {
            MessageBox.Show("Login Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // TODO: Add your navigation logic here to switch to the Main Dashboard
            // Example: ((MainWindow)Application.Current.MainWindow).MainContent.Content = new ConfigurationView();
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.LoginArea.Visibility = Visibility.Collapsed;
                mainWindow.DashboardLayout.Visibility = Visibility.Visible;
            }
        }
    }
}

