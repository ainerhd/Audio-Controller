using Audio_Controller;
using System;
using System.IO.Ports;
using System.Linq;

class Program
{
    private static SerialPort serialPort;
    private static int channelCount = 1; // Standardanzahl der Kanäle

    static void Main(string[] args)
    {
        Console.WriteLine("Willkommen zum seriellen Lautstärkeregler!");

        // Benutzer nach COM-Port fragen
        Console.Write("Gib den COM-Port ein (z.B. COM3): ");
        string comPort = "Com" + Console.ReadLine();

        // Benutzer nach Anzahl der Kanäle fragen
        Console.Write("Wie viele Kanäle sollen gelesen werden? ");
        if (!int.TryParse(Console.ReadLine(), out int channelCount) || channelCount < 1)
        {
            Console.WriteLine("Ungültige Eingabe. Es wird standardmäßig 1 Kanal verwendet.");
            channelCount = 1;
        }

        // Instanzen der Klassen erstellen
        var serialConnection = new SerialConnection(comPort);
        var dataProcessor = new DataProcessor(channelCount);
        var volumeController = new VolumeController();

        // Datenverarbeitung bei empfangenen Daten
        serialConnection.DataReceived += rawData =>
        {
            int[] percentages = dataProcessor.Process(rawData);
            if (percentages.Length > 0)
            {
                volumeController.SetVolume(percentages);
            }
        };

        try
        {
            serialConnection.Open();
            Console.WriteLine("Drücke eine beliebige Taste, um das Programm zu beenden.");
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
