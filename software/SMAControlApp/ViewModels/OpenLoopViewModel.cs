using SMAControlApp.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ObservableCollection<ActuatorChannel> Channels => App.Actuators;

        //private bool _isAllRunning;
        public bool IsAllRunning
        {
            get
            {
                foreach (var c in Channels)
                {
                    if (!c.IsRunning || c.Mode != ControlMode.OpenLoop)
                        return false;
                }
                return true;
            }

            set
            {
                foreach (var c in Channels)
                {
                    if (value)
                    {
                        if (c.Mode == ControlMode.None || c.Mode == ControlMode.OpenLoop)
                        {
                            c.Mode = ControlMode.OpenLoop;
                            c.IsRunning = true;
                        }
                    }
                    else
                    {
                        if (c.Mode == ControlMode.OpenLoop)
                            c.IsRunning = false;
                    }
                }

                OnPropertyChanged();
            }
        }

        public bool GlobalControlsEnabled
        {
            get
            {
                foreach (var c in Channels)
                {
                    if (c.Mode == ControlMode.ClosedLoop)
                        return false;
                }
                return true;
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
                ch.InputVoltage = GlobalVoltage;
            }
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