using SMAControlApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace SMAControlApp
{
    public partial class App : Application
    {
        public static Configuration Config { get; private set; } = null!;
        public static ObservableCollection<ActuatorChannel> Actuators { get; private set; } = null!;

        public static void BuildActuators()
        {
            int newCount = Config.ActuatorCount;
            int currentCount = Actuators.Count;

            if (newCount > currentCount)
            {
                for (int i = currentCount + 1; i <= newCount; i++)
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
            else if (newCount < currentCount)
            {
                while (Actuators.Count > newCount)
                {
                    Actuators.RemoveAt(Actuators.Count - 1);
                }
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
            Actuators = new ObservableCollection<ActuatorChannel>();
            Config.PropertyChanged += Config_PropertyChanged;
            BuildActuators();
        }
    }
}