using Audio_Controller;
using Audio_Controller.Audio_Controller;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.IO.Ports;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Willkommen zum Multi-Geräte-Lautstärkeregler!");

        // COM-Port suchen
        string comPort = InitializeComPort();
        if (string.IsNullOrEmpty(comPort)) return;

        // VolumeController initialisieren
        var volumeController = new VolumeController();
        if (!volumeController.HasDevices())
        {
            Console.WriteLine("Keine Audio-Geräte gefunden. Bitte prüfen Sie Ihre Audio-Einstellungen.");
            Console.ReadLine();
            return;
        }

        // Kanäle und Geräte zuordnen
        var channelToDeviceMap = AssignDevicesToChannels(volumeController);

        // Serielle Verbindung starten
        StartSerialConnection(comPort, channelToDeviceMap);
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
                Console.WriteLine($"Gerät gefunden auf {comPort}.");
                return comPort;
            }
            else
            {
                Console.WriteLine("Kein passendes Gerät über Nachrichtenerkennung gefunden.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler bei der Nachrichtenerkennung: {ex.Message}");
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


    static Dictionary<int, MMDevice> AssignDevicesToChannels(VolumeController volumeController)
    {
        volumeController.ListDevices();
        Console.Write("Wie viele Kanäle sollen gelesen werden? ");
        if (!int.TryParse(Console.ReadLine(), out int channelCount) || channelCount < 1)
        {
            Console.WriteLine("Ungültige Eingabe. Es wird standardmäßig 1 Kanal verwendet.");
            channelCount = 1;
        }

        var channelToDeviceMap = new Dictionary<int, MMDevice>();
        for (int i = 1; i <= channelCount; i++)
        {
            Console.Write($"Kanal {i}: Wähle ein Gerät (Nummer): ");
            if (int.TryParse(Console.ReadLine(), out int deviceIndex))
            {
                var device = volumeController.GetDeviceByIndex(deviceIndex);
                if (device != null)
                {
                    channelToDeviceMap[i] = device;
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
            Console.WriteLine("Fehler: " + ex.Message);
        }
        finally
        {
            serialConnection.Close();
            Console.WriteLine("Serielle Verbindung geschlossen.");
        }
    }
}
