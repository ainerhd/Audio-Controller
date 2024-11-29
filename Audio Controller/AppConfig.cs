using System;
using System.Collections.Generic;

namespace Audio_Controller.Audio_Controller
{
    public class AppConfig
    {
        public string ComPort { get; set; } // Der ausgewählte COM-Port
        public Dictionary<int, string> ChannelDeviceMap { get; set; } // Zuordnung von Kanälen zu Geräten (FriendlyName)

        // Konstruktor mit Standardwerten
        public AppConfig()
        {
            ComPort = null;
            ChannelDeviceMap = new Dictionary<int, string>();
        }
    }
}
