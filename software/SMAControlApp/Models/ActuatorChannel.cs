using System;
using System.Collections.Generic;
using System.Text;

namespace SMAControlApp.Models
{
    class ActuatorChannel
    {
        public int ChannelId { get; set; }
        public double DesiredDisplacement { get; set; }
        public double CurrentDisplacement { get; set; }
        public bool IsRunning { get; set; }
    }
}
