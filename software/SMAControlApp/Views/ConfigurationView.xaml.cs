using SMAControlApp.Models;
using SMAControlApp.Data;
using Microsoft.EntityFrameworkCore;
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
            BuildCoefficientBoxes(degree, App.Config.EquationCoefficients);
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
            int count = degree + 1;

            for (int i = 0; i < count; i++)
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

                var box = new TextBox
                {
                    Width = 70,
                    Height = 28,
                    Background = System.Windows.Media.Brushes.White,
                    Foreground = System.Windows.Media.Brushes.Black,
                    Text = (existing != null && i < existing.Count)
                           ? existing[i].ToString()
                           : "0"
                };
                panel.Children.Add(box);
                CoefficientsPanel.Items.Add(panel);
            }
        }

        private List<double>? ReadCoefficients()
        {
            // Validate actuator count
            if (!int.TryParse(ActuatorCountBox.Text, out int count) || count < 1 || count > 32)
            {
                MessageBox.Show("Actuator count must be between 1 and 32.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;  // FIX: was 'return;'
            }

            var coefficients = new List<double>();  // FIX: was 'result'
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
                        coefficients.Add(val);  // FIX: was 'result.Add'
                    }
                }
            }
            return coefficients;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var coefficients = ReadCoefficients();
            if (coefficients == null) return;

            App.Config.EquationCoefficients = coefficients;

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
                    db.Configs.Update(App.Config);
                }

                int changes = db.SaveChanges();

                if (changes > 0)
                    MessageBox.Show("Saved successfully!", "Saved",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("No changes detected.", "Saved",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }

            App.BuildActuators();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Reset all settings to defaults?",
                "Reset Configuration",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            App.Config.ActuatorCount = 1;
            App.Config.MinVoltage = 0;
            App.Config.MaxVoltage = 120;
            App.Config.AmplifierGain = 0;
            App.Config.EquationCoefficients = new List<double>();

            ActuatorCountBox.Text = "1";
            DegreeBox.Text = string.Empty;
            CoefficientsPanel.Items.Clear();

            using (var db = new AppDbContext())
            {
                db.Configs.Update(App.Config);
                db.SaveChanges();
            }

            App.BuildActuators();
        }
    }
}