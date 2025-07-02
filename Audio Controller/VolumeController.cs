using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;


namespace Audio_Controller
{
    public class VolumeController
    {
        private readonly List<MMDevice> devices;

        public VolumeController()
        {
            devices = new List<MMDevice>();

            try
            {
                var deviceEnumerator = new MMDeviceEnumerator();
                foreach (var device in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                {
                    devices.Add(device);
                }

                if (devices.Count == 0)
                {
                    Console.WriteLine("Keine aktiven Audiogeräte gefunden.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Abrufen der Audiogeräte: " + ex.Message);
            }
        }

        public void ListDevices()
        {
            Console.WriteLine("Verfügbare Geräte:");
            for (int i = 0; i < devices.Count; i++)
            {
                Console.WriteLine($"{i + 1}: {devices[i].FriendlyName}");
            }
        }

        public MMDevice GetDeviceByIndex(int index)
        {
            if (index >= 1 && index <= devices.Count)
            {
                return devices[index - 1];
            }

            Console.WriteLine("Ungültige Auswahl. Gerät nicht gefunden.");
            return null;
        }

        public bool HasDevices()
        {
            return devices != null && devices.Count > 0;
        }

        public void SetVolume(MMDevice device, int volumePercent)
        {
            if (device == null)
            {
                Console.WriteLine("[WARN] Kein Gerät angegeben.");
                return;
            }

            volumePercent = Math.Min(Math.Max(volumePercent, 0), 100);

            try
            {
                float linear = volumePercent / 100f; // von 0-100% nach 0.0-1.0
                // logarithmische Skalierung für ein natürlicheres Lautstärkegefühl
                // 0% -> 0.0, 100% -> 1.0
                float volume = (float)Math.Log10(1 + 9 * linear);
                device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Einstellen der Lautstärke für {device.FriendlyName}: {ex.Message}");
            }
        }

        public MMDevice GetDeviceByName(string friendlyName)
        {
            return devices.Find(d => string.Equals(d.FriendlyName?.Trim(), friendlyName?.Trim(), StringComparison.OrdinalIgnoreCase));
        }


        public List<string> GetDeviceNames()
        {
            var deviceNames = new List<string>();
            foreach (var device in devices)
            {
                deviceNames.Add(device.FriendlyName);
            }
            return deviceNames;
        }


    }

}
