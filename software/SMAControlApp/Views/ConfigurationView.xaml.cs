using SMAControlApp.Models;
using SMAControlApp.Data; // Ensure this points to your DbContext
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SMAControlApp.Views
{
    public partial class ConfigurationView : UserControl
    {
        public ConfigurationView()
        {
            InitializeComponent();
            // DataContext is linked to the global Config loaded in App.xaml.cs
            DataContext = App.Config;
            Loaded += ConfigurationView_Loaded;
        }

        private void ConfigurationView_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.Config == null) return;

            ActuatorCountBox.Text = App.Config.ActuatorCount.ToString();

            int degree = App.Config.EquationCoefficients?.Count > 0
                ? App.Config.EquationCoefficients.Count - 1
                : 0;

            if (degree <= 0) return;

            DegreeBox.Text = degree.ToString();
            Degree_TextChanged(null, null);

            // Populate the dynamic TextBoxes with existing coefficients
            int index = 0;
            foreach (var item in CoefficientsPanel.Items)
            {
                if (item is StackPanel sp)
                {
                    var tb = sp.Children.OfType<TextBox>().FirstOrDefault();
                    if (tb != null && index < App.Config.EquationCoefficients.Count)
                    {
                        tb.Text = App.Config.EquationCoefficients[index].ToString();
                        index++;
                    }
                }
            }
        }

        private void Degree_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CoefficientsPanel == null) return;
            CoefficientsPanel.Items.Clear();

            if (!int.TryParse(DegreeBox.Text, out int n) || n < 1 || n > 10)
                return;

            for (int i = n; i >= 0; i--)
            {
                string label = i == 0 ? "a₀ (const)" : $"a{i} · V^{i}";

                var stack = new StackPanel { Margin = new Thickness(0, 0, 10, 0), Width = 90 };
                stack.Children.Add(new TextBlock
                {
                    Text = label,
                    FontSize = 10,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 0, 0, 3),
                    TextWrapping = TextWrapping.Wrap
                });

                stack.Children.Add(new TextBox { Height = 25, Tag = i });
                CoefficientsPanel.Items.Add(stack);
            }
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            PasswordErrorText.Text = "";

            string current = CurrentPasswordBox.Password;
            string newPwd = NewPasswordBox.Password;
            string confirm = ConfirmPasswordBox.Password;

            if (current != App.CurrentUser.PasswordHash)
            {
                PasswordErrorText.Text = "Current password is incorrect.";
                return;
            }

            if (string.IsNullOrWhiteSpace(newPwd))
            {
                PasswordErrorText.Text = "New password cannot be empty.";
                return;
            }

            if (newPwd != confirm)
            {
                PasswordErrorText.Text = "New passwords do not match.";
                return;
            }

            using (var db = new AppDbContext())
            {
                var dbUser = db.Users.FirstOrDefault(u => u.UserId == App.CurrentUser.UserId);
                if (dbUser != null)
                {
                    dbUser.PasswordHash = newPwd;
                    db.SaveChanges();
                    App.CurrentUser.PasswordHash = newPwd;
                }
            }

            CurrentPasswordBox.Clear();
            NewPasswordBox.Clear();
            ConfirmPasswordBox.Clear();
            MessageBox.Show("Password changed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validation
            if (!int.TryParse(ActuatorCountBox.Text, out int count) || count < 1 || count > 32)
            {
                MessageBox.Show("Actuator count must be between 1 and 32.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var coefficients = new List<double>();
            foreach (var item in CoefficientsPanel.Items)
            {
                if (item is StackPanel sp)
                {
                    var tb = sp.Children.OfType<TextBox>().FirstOrDefault();
                    if (tb != null)
                    {
                        if (!double.TryParse(tb.Text, out double val))
                        {
                            MessageBox.Show($"Invalid coefficient: '{tb.Text}'", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        coefficients.Add(val);
                    }
                }
            }

            // 2. Update the global object first, then sync actuators
            App.Config.ActuatorCount = count;
            App.Config.EquationCoefficients = coefficients;
            App.SyncActuatorsWithDatabase(); // SyncActuatorsWithDatabase already saves ActuatorCount to DB

            // 3. Persist remaining config fields to SQLite
            using (var db = new AppDbContext())
            {
                var existingConfig = db.Configs.FirstOrDefault(c => c.Id == App.Config.Id);

                if (existingConfig != null)
                {
                    // Copy scalar fields individually to avoid EF tracking conflicts
                    existingConfig.ActuatorCount = App.Config.ActuatorCount;
                    existingConfig.AmplifierGain = App.Config.AmplifierGain;
                    existingConfig.MinVoltage = App.Config.MinVoltage;
                    existingConfig.MaxVoltage = App.Config.MaxVoltage;
                    existingConfig.EquationCoefficients = coefficients;
                }
                else
                {
                    // Safe insert path: create a detached copy, never attach App.Config directly
                    var newConfig = new Configuration
                    {
                        UserId = App.Config.UserId,
                        ActuatorCount = App.Config.ActuatorCount,
                        AmplifierGain = App.Config.AmplifierGain,
                        MinVoltage = App.Config.MinVoltage,
                        MaxVoltage = App.Config.MaxVoltage,
                        EquationCoefficients = new List<double>(coefficients)
                    };
                    db.Configs.Add(newConfig);
                }

                db.SaveChanges();
                MessageBox.Show("Saved successfully!");
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Reset all settings to default?", "Confirm", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            // 1. UPDATE RAM FIRST
            App.Config.ActuatorCount = 17;
            App.Config.AmplifierGain = 1;
            App.Config.MinVoltage = 0;
            App.Config.MaxVoltage = 120;
            App.Config.EquationCoefficients = new List<double>();

            // 2. SYNC ACTUATORS (reads ActuatorCount from RAM, updates DB channels)
            App.SyncActuatorsWithDatabase();

            // 3. Update UI
            ActuatorCountBox.Text = "17";
            DegreeBox.Text = "";
            CoefficientsPanel.Items.Clear();

            // 4. PERSIST remaining config fields to DB
            using (var db = new AppDbContext())
            {
                var existingConfig = db.Configs.FirstOrDefault(c => c.Id == App.Config.Id);

                if (existingConfig != null)
                {
                    // Copy scalar fields individually to avoid EF tracking conflicts
                    existingConfig.ActuatorCount = 17;
                    existingConfig.AmplifierGain = 1;
                    existingConfig.MinVoltage = 0;
                    existingConfig.MaxVoltage = 120;
                    existingConfig.EquationCoefficients = new List<double>();
                }
                else
                {
                    var newConfig = new Configuration
                    {
                        UserId = App.Config.UserId,
                        ActuatorCount = 17,
                        AmplifierGain = 1,
                        MinVoltage = 0,
                        MaxVoltage = 120,
                        EquationCoefficients = new List<double>()
                    };
                    db.Configs.Add(newConfig);
                }

                db.SaveChanges();
                MessageBox.Show("Reset and saved successfully!");
            }
        }
    }
}