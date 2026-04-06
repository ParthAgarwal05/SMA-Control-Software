using SMAControlApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using static SMAControlApp.Models.ActuatorChannel;

namespace SMAControlApp.ViewModels
{
    public class OpenLoopViewModel : INotifyPropertyChanged
    {
        public OpenLoopViewModel()
        {
            foreach (var c in Channels)
                c.PropertyChanged += Channel_PropertyChanged;
        }

        private void Channel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ActuatorChannel.IsRunning))
                OnPropertyChanged(nameof(IsAllRunning));

            if (e.PropertyName == nameof(ActuatorChannel.Mode))
                OnPropertyChanged(nameof(GlobalControlsEnabled));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ObservableCollection<ActuatorChannel> Channels => App.Actuators;

        public bool IsAllRunning
        {
            get => Channels.Count > 0 && Channels.All(c => c.IsRunning && c.Mode == ControlMode.OpenLoop);
            set
            {
                foreach (var c in Channels)
                {
                    if (value) // Start All
                    {
                        if (c.Mode == ControlMode.None || c.Mode == ControlMode.OpenLoop)
                        {
                            c.Mode = ControlMode.OpenLoop;
                            c.IsRunning = true;
                        }
                    }
                    else // Stop All
                    {
                        c.IsRunning = false;
                    }
                }
                OnPropertyChanged();
            }
        }

        public bool GlobalControlsEnabled => Channels.All(c => c.Mode != ControlMode.ClosedLoop);

        private double _globalVoltage;
        public double GlobalVoltage
        {
            get => _globalVoltage;
            set { _globalVoltage = value; OnPropertyChanged(); }
        }

        public void ApplyVoltageToAll()
        {
            foreach (var ch in Channels)
                ch.InputVoltage = GlobalVoltage;
        }

        public void StartChannel(ActuatorChannel ch)
        {
            if (ch.Mode != ControlMode.None && ch.Mode != ControlMode.OpenLoop)
                return;

            ch.Mode = ControlMode.OpenLoop;
            ch.IsRunning = true;
        }
    }
}