using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SMAControlApp.Models
{
    public class ActuatorChannel : INotifyPropertyChanged
    {
        private int _channelId;
        private double _desiredDisplacement;
        private double _currentDisplacement;
        private bool _isRunning;
        public int ChannelId { 
            get => _channelId;
            set {
                _channelId = value;
                OnPropertyChanged();
            }
        }
        public double DesiredDisplacement {
            get => _desiredDisplacement;
            set {
                _desiredDisplacement = value;
                OnPropertyChanged();
            }
        }
        public double CurrentDisplacement { 
            get => _currentDisplacement;
            set {
                _currentDisplacement = value;
                OnPropertyChanged();
            }
        }
        public bool IsRunning {
            get => _isRunning;
            set {
                _isRunning = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
