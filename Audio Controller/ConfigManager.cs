using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Audio_Controller.Audio_Controller
{
    public static class ConfigManager
    {
        private static readonly string ConfigFilePath = "config.json"; // Speicherort der Konfigurationsdatei

        public static AppConfig LoadConfig()
        {
            if (!File.Exists(ConfigFilePath))
            {
                Console.WriteLine("Keine vorhandene Konfiguration gefunden. Erstelle eine neue.");
                return new AppConfig
                {
                    ComPort = null,
                    ChannelDeviceMap = new Dictionary<int, string>()
                };
            }

            try
            {
                string json = File.ReadAllText(ConfigFilePath);
                return JsonConvert.DeserializeObject<AppConfig>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden der Konfiguration: {ex.Message}");
                return null;
            }
        }

        public static void SaveConfig(AppConfig config)
        {
            try
            {
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
                Console.WriteLine("Konfiguration erfolgreich gespeichert.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Speichern der Konfiguration: {ex.Message}");
            }
        }
    }
}
