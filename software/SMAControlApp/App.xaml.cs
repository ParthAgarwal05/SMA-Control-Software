using SMAControlApp.Models;
using SMAControlApp.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace SMAControlApp
{
    public partial class App : Application
    {
        public static Configuration Config { get; private set; }
        public static ObservableCollection<ActuatorChannel> Actuators { get; private set; }

        // Singleton GraphViewModel – alive for entire app lifetime
        public static GraphViewModel GraphVM { get; private set; }

        private void BuildActuators()
        {
            Actuators.Clear();
            for (int i = 1; i <= Config.ActuatorCount; i++)
            {
                Actuators.Add(new ActuatorChannel
                {
                    ChannelId = i,
                    DesiredDisplacement = 0,
                    CurrentDisplacement = 0,
                    IsRunning = false
                });
            }
        }

        private void Config_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Configuration.ActuatorCount))
                BuildActuators();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Build config and actuators first
            Config = new Configuration();
            Config.ActuatorCount = 35;
            Config.EquationCoefficients = new List<double> { 2, 5 }; // v = 2s + 5

            Actuators = new ObservableCollection<ActuatorChannel>();
            Config.PropertyChanged += Config_PropertyChanged;
            BuildActuators();

            // 2. Create GraphVM AFTER actuators exist so it can subscribe to all of them
            GraphVM = new GraphViewModel();

            // 3. NOW show the window – App.GraphVM is guaranteed non-null
            //    StartupUri was removed from App.xaml to ensure this order
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}