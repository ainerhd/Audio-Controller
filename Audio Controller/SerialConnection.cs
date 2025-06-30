using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audio_Controller
{
    public class SerialConnection
    {
        private SerialPort serialPort;

        public event Action<string> DataReceived;

        public SerialConnection(string portName, int baudRate = 9600)
        {
            serialPort = new SerialPort(portName, baudRate)
            {
                ReadTimeout = 500,
                WriteTimeout = 500
            };
            serialPort.DataReceived += OnDataReceived;
        }

        public void Open()
        {
            if (serialPort.IsOpen)
            {
                return;
            }

            try
            {
                serialPort.Open();
                Console.WriteLine($"Verbindung zu {serialPort.PortName} hergestellt.");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"[ERROR] Kein Zugriff auf {serialPort.PortName}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Öffnen von {serialPort.PortName} fehlgeschlagen: {ex.Message}");
                throw;
            }
        }

        public void Close()
        {
            if (serialPort.IsOpen)
            {
                serialPort.DataReceived -= OnDataReceived;
                serialPort.Close();
                Console.WriteLine("Verbindung geschlossen.");
            }
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort.ReadLine().Trim();
                DataReceived?.Invoke(data); // Event auslösen
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Lesen der seriellen Daten: " + ex.Message);
            }
        }
    }
}
