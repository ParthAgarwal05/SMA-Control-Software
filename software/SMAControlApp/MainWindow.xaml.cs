using System.Windows;
using SMAControlApp.Views;

namespace SMAControlApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenLoop_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new OpenLoopView();
        }

        private void ClosedLoop_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ClosedLoopView();
        }

        private void Graph_Click(object sender, RoutedEventArgs e)
        {
            var view = new GraphView();
            // Use the singleton – never new GraphViewModel() here
            // A fresh instance would miss all events that already fired
            view.DataContext = App.GraphVM;
            MainContent.Content = view;
        }

        private void Config_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ConfigurationView();
        }
    }
}