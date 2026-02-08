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
        //private void Channel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(ActuatorChannel.IsRunning))
        //    {
        //        OnPropertyChanged(nameof(IsAllRunning));
        //    }
        //}
        //private void Channels_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.NewItems != null)
        //        foreach (ActuatorChannel c in e.NewItems)
        //            c.PropertyChanged += Channel_PropertyChanged;

        //    if (e.OldItems != null)
        //        foreach (ActuatorChannel c in e.OldItems)
        //            c.PropertyChanged -= Channel_PropertyChanged;
        //}

        public ObservableCollection<ActuatorChannel> Channels { get; set; }

        public ClosedLoopViewModel()
        {
            Channels = new ObservableCollection<ActuatorChannel>();

            for (int i = 1; i <= 5; i++)
            {
                Channels.Add(new ActuatorChannel
                {
                    ChannelId = i,
                    DesiredDisplacement = 10,
                    CurrentDisplacement = 0,
                    IsRunning = false
                });
            }
            //foreach (var c in Channels)
            //    c.PropertyChanged += Channel_PropertyChanged;
            //Channels.CollectionChanged += Channels_CollectionChanged;

        }
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
