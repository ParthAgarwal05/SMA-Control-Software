using SMAControlApp.Models;
using System.Collections.Generic;
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
            DataContext = App.Config;
            Loaded += ConfigurationView_Loaded;
        }

        private void ConfigurationView_Loaded(object sender, RoutedEventArgs e)
        {
            ActuatorCountBox.Text = App.Config.ActuatorCount.ToString();

            int degree = App.Config.EquationCoefficients?.Count > 0
                ? App.Config.EquationCoefficients.Count - 1
                : 0;

            if (degree <= 0)
                return;

            DegreeBox.Text = degree.ToString();

            Degree_TextChanged(null, null);

            int index = 0;

            foreach (var item in CoefficientsPanel.Items)
            {
                if (item is StackPanel sp)
                {
                    foreach (var child in sp.Children)
                    {
                        if (child is TextBox tb && index < App.Config.EquationCoefficients.Count)
                        {
                            tb.Text = App.Config.EquationCoefficients[index].ToString();
                            index++;
                        }
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

                var stack = new StackPanel
                {
                    Margin = new Thickness(0, 0, 10, 0),
                    Width = 90
                };

                stack.Children.Add(new TextBlock
                {
                    Text = label,
                    FontSize = 10,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 0, 0, 3),
                    TextWrapping = TextWrapping.Wrap
                });

                stack.Children.Add(new TextBox
                {
                    Height = 25,
                    Foreground = Brushes.Black,
                    Background = Brushes.White,
                    Tag = i
                });

                CoefficientsPanel.Items.Add(stack);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ActuatorCountBox.Text, out int count) || count < 1 || count > 32)
            {
                MessageBox.Show("Actuator count must be between 1 and 32.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (App.Config.AmplifierGain <= 0)
            {
                MessageBox.Show("Amplifier gain must be a positive number.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CoefficientsPanel.Items.Count == 0)
            {
                MessageBox.Show("Please enter the equation degree and coefficients.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var coefficients = new List<double>();
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
                                MessageBox.Show($"Invalid coefficient: '{tb.Text}'",
                                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            coefficients.Add(val);
                        }
                    }
                }
            }

            App.Config.ActuatorCount = count;
            App.Config.EquationCoefficients = coefficients;

            App.BuildActuators();           

            MessageBox.Show("Configuration saved successfully.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            App.Config.ActuatorCount = 17;
            App.Config.AmplifierGain = 1;
            App.Config.MinVoltage = 0;
            App.Config.MaxVoltage = 120;
            App.Config.EquationCoefficients = new List<double>();
            ActuatorCountBox.Text = "17";
            DegreeBox.Text = "";
            CoefficientsPanel.Items.Clear();
        }
    }
}