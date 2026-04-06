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
        private void StartStopAll_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as OpenLoopViewModel;
            var toggle = sender as ToggleButton;
            if (vm == null || toggle == null) return;

            // Use OneWay logic: Check what the user INTENDS to do
            bool intendingToStart = !vm.IsAllRunning;

            if (intendingToStart)
            {
                // 1. Safety Check
                var config = App.Config;
                foreach (var actuator in vm.Channels)
                {
                    double outputVoltage = actuator.InputVoltage * config.AmplifierGain;
                    if (outputVoltage > config.MaxVoltage || outputVoltage < config.MinVoltage)
                    {
                        MessageBox.Show($"Voltage Error on Channel {actuator.ChannelId}!", "Safety Abort");
                        toggle.IsChecked = false; // Force UI back to Green
                        return;
                    }
                }

                // 2. Confirmation
                var result = MessageBox.Show("Start all actuators in Open Loop?", "Confirm", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                    vm.IsAllRunning = true;
                else
                    toggle.IsChecked = false;
            }
            else
            {
                vm.IsAllRunning = false; // Stop is always allowed
            }
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            var actuator = (sender as ToggleButton)?.DataContext as ActuatorChannel;
            var vm = DataContext as OpenLoopViewModel;
            if (actuator == null || vm == null) return;

            // TwoWay binding already updated actuator.IsRunning
            if (actuator.IsRunning)
            {
                double outputVoltage = actuator.InputVoltage * App.Config.AmplifierGain;

                if (outputVoltage > App.Config.MaxVoltage || outputVoltage < App.Config.MinVoltage)
                {
                    MessageBox.Show("Voltage out of range!", "Error");
                    actuator.IsRunning = false; // UI flips back automatically
                    return;
                }
                vm.StartChannel(actuator);
            }
        }
    }
}