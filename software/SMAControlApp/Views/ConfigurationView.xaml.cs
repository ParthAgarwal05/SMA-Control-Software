using Microsoft.EntityFrameworkCore;
using SMAControlApp.Data;
using SMAControlApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SMAControlApp.Views
{
    public partial class ConfigurationView : UserControl
    {
        public ConfigurationView()
        {
            InitializeComponent();
            DataContext = App.Config;

            if (App.Config.EquationCoefficients.Count > 0)
            {
                int degree = App.Config.EquationCoefficients.Count - 1;
                DegreeBox.Text = degree.ToString();
                BuildCoefficientBoxes(degree, App.Config.EquationCoefficients);
            }
        }

        private void Degree_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!int.TryParse(DegreeBox.Text, out int degree) || degree < 1 || degree > 10)
            {
                CoefficientsPanel.Items.Clear();
                return;
            }
            BuildCoefficientBoxes(degree, null);
        }

        private void BuildCoefficientBoxes(int degree, List<double>? existing)
        {
            CoefficientsPanel.Items.Clear();

            for (int i = 0; i <= degree; i++)
            {
                var panel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(0, 0, 8, 0)
                };

                panel.Children.Add(new TextBlock
                {
                    Text = $"a{degree - i}",
                    FontSize = 11,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center
                });

                panel.Children.Add(new TextBox
                {
                    Width = 70,
                    Height = 28,
                    Background = System.Windows.Media.Brushes.White,
                    Foreground = System.Windows.Media.Brushes.Black,
                    Text = (existing != null && i < existing.Count)
                           ? existing[i].ToString()
                           : "0"
                });

                CoefficientsPanel.Items.Add(panel);
            }
        }

        private List<double>? ReadCoefficients()
        {
            var result = new List<double>();

            foreach (var item in CoefficientsPanel.Items)
            {
                if (item is StackPanel sp)
                {
                    var tb = sp.Children.OfType<TextBox>().FirstOrDefault();
                    if (tb != null)
                    {
                        if (!double.TryParse(tb.Text, out double val))
                        {
                            MessageBox.Show(
                                $"Invalid coefficient value: '{tb.Text}'.\nPlease enter numeric values.",
                                "Validation Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return null;
                        }
                        result.Add(val);
                    }
                }
            }
            return result;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var coefficients = ReadCoefficients();
            if (coefficients == null) return;

            App.Config.EquationCoefficients = coefficients;
            App.SyncActuatorsWithDatabase(); // SyncActuatorsWithDatabase already saves ActuatorCount to DB

            // 3. Persist remaining config fields to SQLite
            using (var db = new AppDbContext())
            {
                var existingConfig = db.Configs.FirstOrDefault(c => c.Id == App.Config.Id);

                if (existingConfig != null)
                {
                    db.Entry(existingConfig).CurrentValues.SetValues(App.Config);
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

                int changes = db.SaveChanges();
                MessageBox.Show(changes > 0
                    ? "Saved successfully!"
                    : "No changes detected.",
                    "Save", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            App.SyncActuatorsWithDatabase();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Reset all settings to defaults?",
                "Reset Configuration",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            // 1. UPDATE RAM FIRST
            App.Config.ActuatorCount = 17;
            App.Config.MinVoltage = 0;
            App.Config.MaxVoltage = 120;
            App.Config.AmplifierGain = 0;
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

