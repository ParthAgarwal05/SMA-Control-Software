using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SMAControlApp.Models
{
    public class Configuration : INotifyPropertyChanged
    {
        private int _actuatorCount;
        private int _amplifierGain;
        private double _minVoltage;
        private double _maxVoltage;
        private List<double> _equationCoefficients;
        public double CalculateVoltage(double displacement)
        {
            double result = 0;
            for (int i = 0; i < EquationCoefficients.Count; i++)
            {
                result += EquationCoefficients[i] * Math.Pow(displacement, EquationCoefficients.Count - i - 1);
            }
            return result;
        }

        public int ActuatorCount
        {
            get => _actuatorCount;
            set
            {
                _actuatorCount = value;
                OnPropertyChanged();
            }
        }
        public int AmplifierGain
        {
            get => _amplifierGain;
            set
            {
                _amplifierGain = value;
                OnPropertyChanged();
            }
        }
        public double MinVoltage
        {
            get => _minVoltage;
            set
            {
                _minVoltage = value;
                OnPropertyChanged();
            }
        }
        public double MaxVoltage
        {
            get => _maxVoltage;
            set
            {
                _maxVoltage = value;
                OnPropertyChanged();
            }
        }

        public List<double> EquationCoefficients
        {
            get => _equationCoefficients;
            set {
                _equationCoefficients = value;
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
