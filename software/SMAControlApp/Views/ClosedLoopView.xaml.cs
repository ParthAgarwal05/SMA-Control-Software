using SMAControlApp.Models;
using SMAControlApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SMAControlApp.Views
{
    /// <summary>
    /// Interaction logic for ClosedLoopView.xaml
    /// </summary>
    public partial class ClosedLoopView : UserControl
    {
        public ClosedLoopView()
        {
            InitializeComponent();
            DataContext = new ClosedLoopViewModel();
        }
        private void ApplyAll_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ClosedLoopViewModel;
            vm?.ApplyDisplacementToAll();
        }

        private void StartStopAll_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ClosedLoopViewModel;
            var toggle = sender as ToggleButton;
            if (vm == null || toggle == null) return;

            // IMPORTANT: Since it's OneWay, 'vm.IsAllRunning' is still the OLD state.
            // We want to flip it, so we check the inverse.
            bool intendedState = !vm.IsAllRunning;

            if (intendedState) // User wants to START
            {
                // 1. Safety Check First
                var config = App.Config;
                foreach (var actuator in vm.Channels)
                {
                    double outputVoltage = config.CalculateVoltage(actuator.DesiredDisplacement) * config.AmplifierGain;
                    if (outputVoltage > config.MaxVoltage || outputVoltage < config.MinVoltage)
                    {
                        MessageBox.Show($"Voltage Error on Channel {actuator.ChannelId}. Aborting.", "Safety Error");
                        toggle.IsChecked = false; // Force the UI button back to Green
                        return;
                    }
                }

                // 2. Confirmation Second
                var result = MessageBox.Show("Start all actuators?", "Confirm", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    vm.IsAllRunning = true; // This triggers the loop in your ViewModel
                }
                else
                {
                    toggle.IsChecked = false; // Force UI button back to Green
                }
            }
            else // User wants to STOP
            {
                vm.IsAllRunning = false;
                // The UI will update to Green automatically because vm.IsAllRunning changed
            }
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            var actuator = (sender as ToggleButton)?.DataContext as ActuatorChannel;
            var vm = DataContext as ClosedLoopViewModel;
            if (actuator == null || vm == null) return;

            // Because of TwoWay binding, 'actuator.IsRunning' is already 
            // set to the new state before this code runs.

            if (actuator.IsRunning) // User just clicked "Start"
            {
                // 1. Validate Voltage
                double v = App.Config.CalculateVoltage(actuator.DesiredDisplacement) * App.Config.AmplifierGain;
                if (v > App.Config.MaxVoltage || v < App.Config.MinVoltage)
                {
                    MessageBox.Show("Voltage Error!");
                    actuator.IsRunning = false; // UI turns Green automatically
                    return;
                }

                // 2. Set Mode
                vm.StartChannel(actuator);
            }
            // If actuator.IsRunning is false, the binding already handled it!
        }

    }
}
