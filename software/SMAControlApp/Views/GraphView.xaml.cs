using SMAControlApp.Models;
using SMAControlApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SMAControlApp.Views
{
    public partial class GraphView : UserControl
    {
        private GraphViewModel VM => DataContext as GraphViewModel;

        public GraphView()
        {
            InitializeComponent();
            // DataContext set by MainWindow.Graph_Click = App.GraphVM
            Loaded += GraphView_Loaded;
        }

        // Rebuild series here — chart canvas is guaranteed to be in the visual tree.
        private void GraphView_Loaded(object sender, RoutedEventArgs e)
        {
            VM?.ApplyRebuildIfNeeded();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            VM?.Reset();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            VM?.SaveToCsv();
        }

        private void ColorSwatch_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is SensorSelector selector)
            {
                var dlg = new ColorPickerDialog(selector.Color)
                {
                    Owner = Window.GetWindow(this)
                };
                if (dlg.ShowDialog() == true)
                    selector.Color = dlg.SelectedColor;
            }
        }
    }
}