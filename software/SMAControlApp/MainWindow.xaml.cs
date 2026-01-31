using System.Text;
using System.Windows;
using SMAControlApp.Views;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SMAControlApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
            MainContent.Content = new GraphView();
        }

        private void Config_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ConfigurationView();
        }
    }
}