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

            // 2. Update the Global Object
            // Note: Changing ActuatorCount here triggers the PropertyChanged event in App.xaml.cs
            // which automatically runs SyncActuatorsWithDatabase().
            App.Config.ActuatorCount = count;
            App.Config.EquationCoefficients = coefficients;

            // 3. Persist to SQLite
            using (var db = new AppDbContext())
            {
                // Check if the record actually exists in DB first
                var existingConfig = db.Configs.FirstOrDefault(c => c.Id == App.Config.Id);

                if (existingConfig != null)
                {
                    // Update the existing tracked record
                    db.Entry(existingConfig).CurrentValues.SetValues(App.Config);
                    existingConfig.EquationCoefficients = coefficients; // Manually assign the list
                }
                else
                {
                    db.Configs.Update(App.Config);
                }

                int changes = db.SaveChanges();

                if (changes > 0)
                    MessageBox.Show("Saved successfully to SQLite!");
                else
                    MessageBox.Show("No changes were detected by the database.");
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Reset all settings to default?", "Confirm", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            // Update RAM
            App.Config.ActuatorCount = 17;
            App.Config.AmplifierGain = 1;
            App.Config.MinVoltage = 0;
            App.Config.MaxVoltage = 120;
            App.Config.EquationCoefficients = new List<double>();

            // Sync UI
            ActuatorCountBox.Text = "17";
            DegreeBox.Text = "";
            CoefficientsPanel.Items.Clear();

            // Persist Reset to DB
            using (var db = new AppDbContext())
            {
                db.Configs.Update(App.Config);
                db.SaveChanges();
            }
        }
    }
}