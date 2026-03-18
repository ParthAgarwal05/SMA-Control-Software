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
        private double _inputVoltage;
        private double _requiredVoltage;
        private DispatcherTimer _timer;
        public enum ControlMode
        {
            None,
            OpenLoop,
            ClosedLoop
        }

        public ActuatorChannel()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        private ControlMode _mode = ControlMode.None;

        public bool IsAvailableInClosedLoop
        {
            get
            {
                return Mode == ControlMode.None || Mode == ControlMode.ClosedLoop;
            }
        }

        public bool IsAvailableInOpenLoop
        {
            get
            {
                return Mode == ControlMode.None || Mode == ControlMode.OpenLoop;
            }
        }
        public ControlMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAvailableInClosedLoop));
                OnPropertyChanged(nameof(IsAvailableInOpenLoop));
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (Mode == ControlMode.ClosedLoop)
            {
                if (CurrentDisplacement < DesiredDisplacement)
                {
                    CurrentDisplacement += 1;
                }
                else
                {
                    _timer.Stop();
                    IsRunning = false;
                    CurrentDisplacement = 0;
                    RequiredVoltage = 0;
                    Mode = ControlMode.None;                   
                }
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

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning == value)
                {
                    OnPropertyChanged();
                    return;
                }

                _isRunning = value;

                if (_isRunning)
                {
                    CurrentDisplacement = 0;
                    ComputeVoltage();
                    _timer.Start();
                }
                else
                {
                    _timer.Stop();
                    CurrentDisplacement = 0;
                    RequiredVoltage = 0; 
                    Mode = ControlMode.None;
                }

                OnPropertyChanged();
            }
        }
        public double InputVoltage
        {
            get => _inputVoltage;
            set
            {
                _inputVoltage = value;
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
