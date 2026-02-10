using SMAControlApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing.Text;
using System.Text;

namespace SMAControlApp.ViewModels
{
    public class ClosedLoopViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public ObservableCollection<ActuatorChannel> Channels => App.Actuators;
        public Configuration Config => App.Config;

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

    }

}
