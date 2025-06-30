using Audio_Controller;
using Audio_Controller.Audio_Controller;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        //Configmanager starten
        AppConfig config = ConfigManager.LoadConfig();

        //
        var volumeController = new VolumeController();
        var mmDeviceMap = ConvertToMMDeviceMap(config.ChannelDeviceMap, volumeController);


        // Überprüfe, ob eine gültige Konfiguration vorhanden ist
        if (CheckConfig(config))
        {
            Console.WriteLine("Eine gültige Konfiguration wurde gefunden.");
            Console.Write("Möchten Sie die gespeicherte Konfiguration verwenden? (y/n): ");
            string useConfig = Console.ReadLine();
            if (useConfig?.ToLower() == "y")
            {
                // Nutze gespeicherte Konfiguration
                Console.WriteLine("Gespeicherte Konfiguration wird verwendet.");
                volumeController = new VolumeController();
                mmDeviceMap = ConvertToMMDeviceMap(config.ChannelDeviceMap, volumeController);
                StartSerialConnection(config.ComPort, mmDeviceMap);
                return;
            }
        }

        // Erstelle eine neue Konfiguration
        Console.WriteLine("Willkommen zum Multi-Geräte-Lautstärkeregler!");

        // COM-Port suchen
        string comPort = InitializeComPort();
        if (string.IsNullOrEmpty(comPort)) return;

        // VolumeController initialisieren
        volumeController = new VolumeController();
        if (!volumeController.HasDevices())
        {
            Console.WriteLine("Keine Audio-Geräte gefunden. Bitte prüfen Sie Ihre Audio-Einstellungen.");
            return;
        }

        // Kanäle und Geräte zuordnen
        var channelToDeviceMap = AssignDevicesToChannels(volumeController);
        mmDeviceMap = ConvertToMMDeviceMap(channelToDeviceMap, volumeController);

        // Serielle Verbindung starten
        StartSerialConnection(comPort, mmDeviceMap);

        // Konfiguration speichern
        config.ComPort = comPort;
        config.ChannelDeviceMap = channelToDeviceMap;
        ConfigManager.SaveConfig(config);
    }


    static string InitializeComPort()
    {
        Console.WriteLine("Starte Suche nach einem passenden COM-Port...");

        // Liste aller verfügbaren Ports anzeigen
        string[] availablePorts = SerialPort.GetPortNames();
        if (availablePorts.Length == 0)
        {
            Console.WriteLine("Keine COM-Ports verfügbar. Bitte überprüfen Sie die Verbindung.");
            return null;
        }

        Console.WriteLine("Gefundene COM-Ports:");
        foreach (var port in availablePorts)
        {
            Console.WriteLine($"- {port}");
        }

        // Versuche, den Mixer über Nachrichtenerkennung zu finden
        try
        {
            Console.WriteLine("Versuche, ein Gerät mit der Nachrichtenerkennung zu finden...");
            string comPort = MixerIdentifier.FindDeviceByMessage("HELLO_MIXER", "MIXER_READY");

            if (!string.IsNullOrEmpty(comPort))
            {
                Console.WriteLine($"✅ Gerät gefunden auf {comPort}.");
                return comPort;
            }
            else
            {
                Console.WriteLine("⚠️ Kein passendes Gerät über Nachrichtenerkennung gefunden.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fehler bei der Nachrichtenerkennung: {ex.Message}");
        }

        // Fallback: Benutzer manuell einen Port auswählen lassen
        Console.WriteLine("Bitte wählen Sie einen COM-Port aus der Liste aus:");
        for (int i = 0; i < availablePorts.Length; i++)
        {
            Console.WriteLine($"{i}: {availablePorts[i]}");
        }

        Console.Write("Eingabe (Nummer): ");
        if (int.TryParse(Console.ReadLine(), out int selectedIndex) && selectedIndex >= 0 && selectedIndex < availablePorts.Length)
        {
            Console.WriteLine($"Manuell ausgewählter Port: {availablePorts[selectedIndex]}");
            return availablePorts[selectedIndex];
        }

        Console.WriteLine("Ungültige Eingabe. Kein COM-Port ausgewählt. Das Programm wird beendet.");
        return null;
    }

    static Dictionary<int, string> AssignDevicesToChannels(VolumeController volumeController)
    {
        volumeController.ListDevices();
        Console.Write("Wie viele Kanäle sollen gelesen werden? ");
        if (!int.TryParse(Console.ReadLine(), out int channelCount) || channelCount < 1)
        {
            Console.WriteLine("Ungültige Eingabe. Es wird standardmäßig 1 Kanal verwendet.");
            channelCount = 1;
        }

        var channelToDeviceMap = new Dictionary<int, string>();
        for (int i = 1; i <= channelCount; i++)
        {
            Console.Write($"Kanal {i}: Wähle ein Gerät (Nummer): ");
            if (int.TryParse(Console.ReadLine(), out int deviceIndex))
            {
                var device = volumeController.GetDeviceByIndex(deviceIndex);
                if (device != null)
                {
                    channelToDeviceMap[i] = device.FriendlyName; // Speichere den FriendlyName
                }
                else
                {
                    Console.WriteLine($"Kanal {i} wurde keinem Gerät zugewiesen.");
                }
            }
            else
            {
                Console.WriteLine($"Ungültige Eingabe für Kanal {i}. Überspringe.");
            }
        }

        return channelToDeviceMap;
    }

    static Dictionary<int, MMDevice> ConvertToMMDeviceMap(Dictionary<int, string> channelDeviceMap, VolumeController volumeController)
    {
        var deviceMap = new Dictionary<int, MMDevice>();

        foreach (var entry in channelDeviceMap)
        {
            var device = volumeController.GetDeviceByName(entry.Value);
            if (device != null)
            {
                deviceMap[entry.Key] = device;
            }
            else
            {
                Console.WriteLine($"[WARN] Gerät '{entry.Value}' konnte nicht gefunden werden.");
            }
        }

        return deviceMap;
    }

    static void StartSerialConnection(string comPort, Dictionary<int, MMDevice> channelToDeviceMap)
    {
        var serialConnection = new SerialConnection(comPort);
        var consoleUpdater = new ConsoleUpdater(channelToDeviceMap.Count);
        var dataProcessor = new DataProcessor(channelToDeviceMap.Count);
        var volumeController = new VolumeController();

        serialConnection.DataReceived += rawData =>
        {
            try
            {
                int[] percentages = dataProcessor.Process(rawData);

                if (percentages.Length > 0)
                {
                    for (int i = 0; i < percentages.Length; i++)
                    {
                        int channel = i + 1;
                        consoleUpdater.UpdateChannel(channel, percentages[i]);

                        if (channelToDeviceMap.TryGetValue(channel, out var device))
                        {
                            volumeController.SetVolume(device, percentages[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler bei der Verarbeitung der Daten: {ex.Message}");
            }
        };

        try
        {
            serialConnection.Open();
            Console.WriteLine("\nDrücke eine beliebige Taste, um das Programm zu beenden.");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Verbindung konnte nicht geöffnet werden: {ex.Message}");
        }
        finally
        {
            serialConnection.Close();
            Console.WriteLine("Serielle Verbindung geschlossen.");
        }
    }

    static bool CheckConfig(AppConfig config)
    {
        if (config == null)
        {
            Console.WriteLine("[ERROR] Die Konfiguration ist null.");
            return false;
        }

        if (string.IsNullOrEmpty(config.ComPort))
        {
            Console.WriteLine("[WARN] Der gespeicherte COM-Port ist ungültig oder leer.");
            return false;
        }

        if (config.ChannelDeviceMap == null || config.ChannelDeviceMap.Count == 0)
        {
            Console.WriteLine("[WARN] Keine Gerätezuordnungen in der Konfiguration gefunden.");
            return false;
        }

        // Prüfen, ob der COM-Port verfügbar ist
        if (!SerialPort.GetPortNames().Contains(config.ComPort))
        {
            Console.WriteLine($"[WARN] Der gespeicherte COM-Port '{config.ComPort}' ist nicht verfügbar.");
            return false;
        }

        // Überprüfen, ob die Geräte existieren
        var volumeController = new VolumeController();
        foreach (var deviceName in config.ChannelDeviceMap.Values)
        {
            if (volumeController.GetDeviceByName(deviceName) == null)
            {
                Console.WriteLine($"[WARN] Das gespeicherte Gerät '{deviceName}' wurde nicht gefunden.");
                return false;
            }
        }

        return true;
    }

}
