using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using SMAControlApp.Models;

namespace SMAControlApp.ViewModels
{
    public class ClosedLoopViewModel
    {
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
        }
    }

}
