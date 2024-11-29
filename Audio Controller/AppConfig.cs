using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audio_Controller.Audio_Controller
{
    public class AppConfig
    {
        public string ComPort { get; set; } // Der ausgewählte COM-Port
        public Dictionary<int, string> ChannelDeviceMap { get; set; } // Zuordnung von Kanälen zu Geräten
    }
}
