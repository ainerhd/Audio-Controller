using System;
using System.Collections.Generic;
using NAudio.CoreAudioApi;
using Audio_Controller.Audio_Controller;

namespace Audio_Controller
{
    public static class ConsoleProgram
    {
        public static void Run(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: <port|auto> <channels> [device names...]");
                return;
            }

            string port = args[0];
            if (string.Equals(port, "auto", StringComparison.OrdinalIgnoreCase))
            {
                port = MixerIdentifier.FindDeviceByMessage("HELLO_MIXER", "MIXER_READY");
                if (string.IsNullOrEmpty(port))
                {
                    Console.WriteLine("Mixer not found.");
                    return;
                }
            }

            if (!int.TryParse(args[1], out int channelCount))
            {
                Console.WriteLine("Invalid channel count.");
                return;
            }

            var volumeController = new VolumeController();
            var deviceMap = new Dictionary<int, MMDevice>();
            for (int i = 0; i < channelCount && i + 2 < args.Length; i++)
            {
                var device = volumeController.GetDeviceByName(args[i + 2]);
                if (device != null)
                {
                    deviceMap[i + 1] = device;
                }
            }

            var processor = new DataProcessor(channelCount);
            var updater = new ConsoleUpdater(channelCount);
            var connection = new SerialConnection(port);
            connection.DataReceived += raw =>
            {
                var values = processor.Process(raw);
                for (int i = 0; i < values.Length; i++)
                {
                    updater.UpdateChannel(i + 1, values[i]);
                    if (deviceMap.TryGetValue(i + 1, out var dev))
                    {
                        volumeController.SetVolume(dev, values[i]);
                    }
                }
            };

            try
            {
                connection.Open();
                Console.WriteLine("Press ENTER to quit");
                Console.ReadLine();
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
