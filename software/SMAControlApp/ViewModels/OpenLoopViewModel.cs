using SMAControlApp.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static SMAControlApp.Models.ActuatorChannel;

namespace SMAControlApp.ViewModels
{
    public class OpenLoopViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ObservableCollection<ActuatorChannel> Channels => App.Actuators;

        private bool _isAllRunning = false;
        public bool IsAllRunning
        {
            get => _isAllRunning;
            set
            {
                _isAllRunning = value;

                foreach (var c in Channels)
                    c.IsRunning = _isAllRunning;

                OnPropertyChanged();
            }
        }

        private double _globalVoltage;

        public double GlobalVoltage
        {
            get => _globalVoltage;
            set
            {
                _globalVoltage = value;
                OnPropertyChanged();
            }
        }

        public void ApplyVoltageToAll()
        {
            foreach (var ch in Channels)
            {
                ch.Mode = ControlMode.OpenLoop;
                ch.InputVoltage = GlobalVoltage;
            }
        }
    }
}