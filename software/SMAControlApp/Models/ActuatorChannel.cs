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
        private double _requiredVoltage;
        public double RequiredVoltage
        {
            get => _requiredVoltage;
            set
            {
                _requiredVoltage = value;
                OnPropertyChanged();
            }
        }

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
        private void ComputeVoltage()
        {
            RequiredVoltage = App.Config.CalculateVoltage(DesiredDisplacement);
        }

        public bool IsRunning {
            get => _isRunning;
            set {
                _isRunning = value;
                OnPropertyChanged();
                if (_isRunning)
                    ComputeVoltage();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
