using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;


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
                    Console.WriteLine($"🔍 Prüfe Port: {port}");

                    using (var serialPort = new SerialPort(port, 9600))
                    {
                        serialPort.Handshake = Handshake.None; // Verhindert Blockaden
                        serialPort.ReadTimeout = 3000;
                        serialPort.WriteTimeout = 1000;  // Falls `WriteLine()` hängt, nach 1 Sekunde abbrechen

                        serialPort.Open();
                        serialPort.DtrEnable = true;
                        serialPort.RtsEnable = true;
                        serialPort.DiscardInBuffer();
                        serialPort.DiscardOutBuffer();

                        Thread.Sleep(100);  // ⚠ Warten, um den Port stabil zu machen

                        Console.WriteLine($"📡 Sende Test-Nachricht an {port}...");

                        // Prüfen, ob der Port tatsächlich zum Schreiben bereit ist
                        if (!serialPort.IsOpen)
                        {
                            Console.WriteLine($"⚠ Port {port} konnte nicht korrekt geöffnet werden, überspringe.");
                            continue;
                        }

                        try
                        {
                            serialPort.WriteLine(requestMessage);
                            Console.WriteLine($"✅ Nachricht gesendet, warte auf Antwort...");
                        }
                        catch (IOException ex)
                        {
                            Console.WriteLine($"❌ IO-Fehler beim Senden an {port}: {ex.Message}");
                            serialPort.Close();
                            continue;
                        }
                        catch (InvalidOperationException ex)
                        {
                            Console.WriteLine($"❌ Ungültige Operation auf {port}: {ex.Message}");
                            serialPort.Close();
                            continue;
                        }
                        catch (TimeoutException)
                        {
                            Console.WriteLine($"⏳ `WriteLine()` Timeout auf {port}, weiter...");
                            serialPort.Close();
                            continue;
                        }

                        // Antwort empfangen
                        var readTask = Task.Run(() =>
                        {
                            try
                            {
                                return serialPort.ReadLine()?.Trim();
                            }
                            catch (TimeoutException)
                            {
                                return null;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"❌ Fehler beim Lesen von {port}: {ex.Message}");
                                return null;
                            }
                        });

                        if (readTask.Wait(3000))
                        {
                            string response = readTask.Result;
                            Console.WriteLine($"📩 Antwort von {port}: {response}");

                            if (!string.IsNullOrEmpty(response) && response.Contains(expectedResponse))
                            {
                                Console.WriteLine($"🎯 Gerät erkannt auf {port} mit Antwort: {response}");
                                return port;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"⌛ Timeout erreicht für Port {port}.");
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"🔒 Zugriff auf {port} verweigert (evtl. belegt?).");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Fehler bei Port {port}: {ex.Message}");
                }
            }

            Console.WriteLine("🔎 Kein passendes Gerät gefunden.");
            return null;
        }


    }



}

