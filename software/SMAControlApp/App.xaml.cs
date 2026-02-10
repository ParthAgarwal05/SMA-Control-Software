using SMAControlApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;

namespace SMAControlApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Configuration Config { get; private set; }
        public static ObservableCollection<ActuatorChannel> Actuators { get; private set; }
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
            {
                BuildActuators();
            }
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Config = new Configuration();
            Config.ActuatorCount = 35;
            Actuators = new ObservableCollection<ActuatorChannel>();
            Config.PropertyChanged += Config_PropertyChanged;

            BuildActuators();
        }

    }

}
