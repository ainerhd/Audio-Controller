using System;
using System.Collections.Generic;

namespace Audio_Controller.Audio_Controller
{
    public class AppConfig
    {
        public string ComPort { get; set; } // Der ausgewählte COM-Port
        public Dictionary<int, string> ChannelDeviceMap { get; set; } // Zuordnung von Kanälen zu Geräten (FriendlyName)
        public int BufferSize { get; set; } = 5; // Glättung
        public int DeadZone { get; set; } = 5;  // Dead-Zone

        // Konstruktor mit Standardwerten
        public AppConfig()
        {
            ComPort = null;
            ChannelDeviceMap = new Dictionary<int, string>();
            BufferSize = 5;
            DeadZone = 5;
        }
    }
}
