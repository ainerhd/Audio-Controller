using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;


namespace Audio_Controller.Audio_Controller
{
    public static class MixerIdentifier
    {
        public static string FindDeviceByMessage(string requestMessage, string expectedResponse)
        {
            foreach (string port in SerialPort.GetPortNames())
            {
                try
                {
                    Console.WriteLine($"Prüfe Port: {port}");

                    using (var serialPort = new SerialPort(port, 9600))
                    {
                        serialPort.Open();

                        // Puffer leeren, um alte Daten zu verwerfen
                        serialPort.DiscardInBuffer();
                        serialPort.DiscardOutBuffer();

                        serialPort.WriteLine(requestMessage); // Nachricht senden
                        serialPort.ReadTimeout = 3000;       // Warte bis zu 3 Sekunden auf Antwort

                        string response = serialPort.ReadLine()?.Trim(); // Antwort lesen und bereinigen
                        Console.WriteLine($"Antwort von {port}: {response}");

                        if (response != null && response.Contains(expectedResponse))
                        {
                            Console.WriteLine($"Gerät erkannt auf {port} mit Antwort: {response}");
                            return port;
                        }
                    }
                }
                catch (TimeoutException)
                {
                    Console.WriteLine($"Keine Antwort von {port} (Timeout).");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler bei Port {port}: {ex.Message}");
                }
            }

            Console.WriteLine("Kein Gerät gefunden.");
            return null;
        }




    }



}

