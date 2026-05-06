using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audio_Controller
{
    public class SerialConnection : IDisposable
    {
        private SerialPort serialPort;
        private bool disposed;

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

        public bool IsOpen => serialPort?.IsOpen == true;

        public void Open()
        {
            if (IsOpen)
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
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.DataReceived -= OnDataReceived;
                serialPort.Close();
                Console.WriteLine("Verbindung geschlossen.");
            }
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!IsOpen || disposed)
            {
                return;
            }

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

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            try
            {
                Close();
                serialPort?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Freigeben der seriellen Verbindung: {ex.Message}");
            }
        }
    }
}
