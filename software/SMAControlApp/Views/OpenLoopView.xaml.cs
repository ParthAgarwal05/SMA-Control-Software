using SMAControlApp.Models;
using SMAControlApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SMAControlApp.Views
{
    public partial class OpenLoopView : UserControl
    {
        public OpenLoopView()
        {
            InitializeComponent();
            DataContext = new OpenLoopViewModel();
            var vm = DataContext as OpenLoopViewModel;
        }

        private void ApplyAll_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as OpenLoopViewModel;
            vm?.ApplyVoltageToAll();
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleButton;
            var actuator = toggle?.DataContext as ActuatorChannel;
            var vm = DataContext as OpenLoopViewModel;

            if (actuator == null || vm == null)
                return;

            if (actuator.IsRunning)
            {
                actuator.IsRunning = false;
            }
            else
            {
                vm.StartChannel(actuator);
            }
        }
    }
}