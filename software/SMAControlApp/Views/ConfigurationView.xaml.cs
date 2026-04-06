using SMAControlApp.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SMAControlApp.Views
{
    public partial class ConfigurationView : UserControl
    {
        public ConfigurationView()
        {
            InitializeComponent();

            // DataContext = App.Config so all TextBox bindings work
            DataContext = App.Config;

            // Rebuild coefficient boxes if coefficients already loaded from file
            if (App.Config.EquationCoefficients.Count > 0)
            {
                int degree = App.Config.EquationCoefficients.Count - 1;
                DegreeBox.Text = degree.ToString();
                BuildCoefficientBoxes(degree, App.Config.EquationCoefficients);
            }
        }

        // Called when user changes the degree number
        private void Degree_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!int.TryParse(DegreeBox.Text, out int degree) || degree < 1 || degree > 10)
            {
                CoefficientsPanel.Items.Clear();
                return;
            }
            BuildCoefficientBoxes(degree, null);
        }

        // Builds (degree + 1) input boxes labeled a_n ... a_0
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

                // Label: a3, a2, a1, a0
                panel.Children.Add(new TextBlock
                {
                    Text = $"a{degree - i}",
                    FontSize = 11,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center
                });

                // Input box
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

        // Reads all coefficient boxes → List<double>
        private List<double>? ReadCoefficients()
        {
            var result = new List<double>();
            foreach (var item in CoefficientsPanel.Items)
            {
                if (item is StackPanel sp)
                {
                    foreach (var child in sp.Children)
                    {
                        if (child is TextBox tb)
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
            }
            return result;
        }

        // Save button
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            
            var coefficients = ReadCoefficients();
            if (coefficients == null) return;

            App.Config.EquationCoefficients = coefficients;

            try
            {
                string path = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "config.json");

                

                ConfigurationService.Save(App.Config);

                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ERROR:\n{ex.Message}\n\n{ex.StackTrace}", "Save Failed");
            }

            App.BuildActuators();
            MessageBox.Show("Configuration saved successfully.", "Saved",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Reset button — restores defaults and clears the JSON file
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
            App.Config.MaxVoltage = 0;
            App.Config.AmplifierGain = 0;
            App.Config.EquationCoefficients = new List<double>();

            DegreeBox.Text = string.Empty;
            CoefficientsPanel.Items.Clear();

            ConfigurationService.Save(App.Config);
            App.BuildActuators();
        }
    }
}