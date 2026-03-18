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
            if (vm == null) return;

            bool start = !vm.IsAllRunning;
            string action = start ? "start" : "stop";

            var result = MessageBox.Show(
                $"Are you sure you want to {action} all actuators?",
                "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) { 
                vm.IsAllRunning = false;
                return;
            }

            var config = App.Config;

            if (start)
            {
                foreach (var actuator in vm.Channels) 
                {
                    double outputVoltage = actuator.InputVoltage * config.AmplifierGain;

                    if (outputVoltage > config.MaxVoltage || outputVoltage < config.MinVoltage)
                    {
                        MessageBox.Show(
                            $"Cannot start all actuators!\n\n" +
                            $"Actuator: {actuator.ChannelId}\n" +
                            $"Voltage: {outputVoltage:F2} V\n" +
                            $"Allowed: {config.MinVoltage} V to {config.MaxVoltage} V",
                            "Voltage Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                        vm.IsAllRunning = false;
                        return; 
                    }
                }
                vm.IsAllRunning = true;
            }
            else
            {
                vm.IsAllRunning = false;
            }
        }
        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleButton;
            var actuator = toggle?.DataContext as ActuatorChannel;
            var config = App.Config;
            var vm = DataContext as OpenLoopViewModel;

            if (actuator == null || vm == null)
                return;

            if (actuator.IsRunning)
            {
                actuator.IsRunning = false;
            }
            else
            {
                double outputVoltage = actuator.InputVoltage * config.AmplifierGain;

                if (outputVoltage > config.MaxVoltage || outputVoltage < config.MinVoltage)
                {
                    MessageBox.Show(
                        $"Voltage out of range!\n\n" +
                        $"Calculated: {outputVoltage:F2} V\n" +
                        $"Allowed: {config.MinVoltage} V to {config.MaxVoltage} V",
                        "Voltage Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    actuator.IsRunning = false;
                    return;
                }
                vm.StartChannel(actuator);
            }
        }
    }
}