using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Threading;


namespace SMAControlApp.Models
{
    public class ActuatorChannel : INotifyPropertyChanged
    {
        private int _channelId;
        private double _desiredDisplacement;
        private double _currentDisplacement;
        private bool _isRunning;
        private double _requiredVoltage;
        private DispatcherTimer _timer;

        public ActuatorChannel()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (CurrentDisplacement < DesiredDisplacement)
            {
                CurrentDisplacement += 1;
            }
            else
            {
                IsRunning = false;
                _timer.Stop();
            }
        }
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
                {
                    CurrentDisplacement = 0;
                    ComputeVoltage();
                    _timer.Start();     
                } else
                {
                    _timer.Stop();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
