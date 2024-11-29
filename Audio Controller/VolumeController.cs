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

        public void SetVolume(MMDevice device, int volumePercent)
        {
            if (device != null)
            {
                try
                {
                    float volume = volumePercent / 100f; // Prozent in Bereich 0.0 - 1.0 umwandeln
                    device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Einstellen der Lautstärke für {device.FriendlyName}: " + ex.Message);
                }
            }
        }
    }

}
