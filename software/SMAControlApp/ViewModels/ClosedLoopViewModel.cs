using SMAControlApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using static SMAControlApp.Models.ActuatorChannel;

namespace SMAControlApp.ViewModels
{
    public class ClosedLoopViewModel : INotifyPropertyChanged
    {
        public ClosedLoopViewModel()
        {
            // Attach listener to every channel so the "Start All" button 
            // knows when to change color automatically
            foreach (var c in Channels)
                c.PropertyChanged += Channel_PropertyChanged;
        }

        private void Channel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ActuatorChannel.IsRunning))
            {
                // If one stops, "IsAllRunning" might no longer be true
                OnPropertyChanged(nameof(IsAllRunning));
            }
            if (e.PropertyName == nameof(ActuatorChannel.Mode))
            {
                OnPropertyChanged(nameof(GlobalControlsEnabled));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ObservableCollection<ActuatorChannel> Channels => App.Actuators;
        public Configuration Config => App.Config;

        public bool GlobalControlsEnabled
        {
            get => Channels.All(c => c.Mode != ControlMode.OpenLoop);
        }

        public bool IsAllRunning
        {
            get
            {
                // Returns true ONLY if every single channel is running
                if (Channels.Count == 0) return false;
                return Channels.All(c => c.IsRunning && c.Mode == ControlMode.ClosedLoop);
            }
            set
            {
                // This is called by the "Start All" button in the View
                foreach (var c in Channels)
                {
                    if (value) // START ALL
                    {
                        if (c.Mode == ControlMode.None || c.Mode == ControlMode.ClosedLoop)
                        {
                            c.Mode = ControlMode.ClosedLoop;
                            c.IsRunning = true;
                        }
                    }
                    else // STOP ALL
                    {
                        c.IsRunning = false;
                        // We don't necessarily change the Mode to None here 
                        // so they stay 'ready' for the next run
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