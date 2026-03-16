using SMAControlApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing.Text;
using System.Text;
using static SMAControlApp.Models.ActuatorChannel;

namespace SMAControlApp.ViewModels
{
    public class ClosedLoopViewModel : INotifyPropertyChanged
    {
        public ClosedLoopViewModel()
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
        public Configuration Config => App.Config;

       // private bool _isAllRunning;

        public bool GlobalControlsEnabled
        {
            get
            {
                foreach (var c in Channels)
                {
                    if (c.Mode == ControlMode.OpenLoop)
                        return false;
                }
                return true;
            }
        }
        public bool IsAllRunning
        {
            get
            {
                foreach (var c in Channels)
                {
                    if (!c.IsRunning || c.Mode != ControlMode.ClosedLoop)
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
                        if (c.Mode == ControlMode.None || c.Mode == ControlMode.ClosedLoop)
                        {
                            c.Mode = ControlMode.ClosedLoop;
                            c.IsRunning = true;
                        }
                    }
                    else
                    {
                        if (c.Mode == ControlMode.ClosedLoop)
                            c.IsRunning = false;
                    }
                }

                OnPropertyChanged();
            }
        }
        private double _globalDisplacement;
        public double GlobalDisplacement
        {
            get => _globalDisplacement;
            set
            {
                _globalDisplacement = value;
                OnPropertyChanged();
            }
        }
        public void ApplyDisplacementToAll()
        {

            foreach (var ch in Channels)
            {
                ch.DesiredDisplacement = GlobalDisplacement;
            }
        }
        public void StartChannel(ActuatorChannel ch)
        {
            if (ch.Mode != ControlMode.None && ch.Mode != ControlMode.ClosedLoop)
                return;

            ch.Mode = ControlMode.ClosedLoop;
            ch.IsRunning = true;
        }
    }

}
