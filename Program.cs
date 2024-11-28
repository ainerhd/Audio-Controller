using Audio_Controller;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Willkommen zum Multi-Geräte-Lautstärkeregler!");

        // Benutzer nach COM-Port fragen
        Console.Write("Gib den COM-Port ein (z.B. COM3): ");
        string comPort = Console.ReadLine();

        // Benutzer nach Anzahl der Kanäle fragen
        Console.Write("Wie viele Kanäle sollen gelesen werden? ");
        if (!int.TryParse(Console.ReadLine(), out int channelCount) || channelCount < 1)
        {
            Console.WriteLine("Ungültige Eingabe. Es wird standardmäßig 1 Kanal verwendet.");
            channelCount = 1;
        }

        var volumeController = new VolumeController();
        volumeController.ListDevices();

        // Geräte den Kanälen zuordnen
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

        // Konsolenausgabe-Manager erstellen
        var consoleUpdater = new ConsoleUpdater(channelCount);

        var serialConnection = new SerialConnection(comPort);
        var dataProcessor = new DataProcessor(channelCount);

        // Datenverarbeitung
        serialConnection.DataReceived += rawData =>
        {
            int[] percentages = dataProcessor.Process(rawData);

            if (percentages.Length > 0)
            {
                for (int i = 0; i < percentages.Length; i++)
                {
                    int channel = i + 1;

                    // Konsolenanzeige aktualisieren
                    consoleUpdater.UpdateChannel(channel, percentages[i]);

                    // Lautstärke des zugeordneten Geräts anpassen
                    if (channelToDeviceMap.TryGetValue(channel, out var device))
                    {
                        volumeController.SetVolume(device, percentages[i]);
                    }
                }
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
        }
    }
}

