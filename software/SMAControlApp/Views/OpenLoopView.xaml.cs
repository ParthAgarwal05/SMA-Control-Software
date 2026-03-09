using SMAControlApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SMAControlApp.Views
{
    public partial class OpenLoopView : UserControl
    {
        public OpenLoopView()
        {
            InitializeComponent();
            DataContext = new OpenLoopViewModel();
        }

        private void ApplyAll_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as OpenLoopViewModel;
            vm?.ApplyVoltageToAll();
        }
    }
}