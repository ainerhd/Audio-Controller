using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audio_Controller
{
    public class DataProcessor
    {
        private int channelCount;

        public DataProcessor(int channelCount)
        {
            this.channelCount = channelCount;
        }

        public int[] Process(string rawData)
        {
            try
            {
                // Trenne die Werte anhand des Trennzeichens " | "
                string[] rawValues = rawData.Split('|').Select(v => v.Trim()).ToArray();

                // Prüfen, ob die Anzahl der Werte korrekt ist
                if (rawValues.Length != channelCount)
                {
                    Console.WriteLine("Fehler: Anzahl der empfangenen Kanäle stimmt nicht mit der Konfiguration überein.");
                    return Array.Empty<int>();
                }

                // Konvertiere die Werte in Prozent
                return rawValues
                    .Select(v => int.Parse(v))           // String -> Integer
                    .Select(value => value * 100 / 1023) // Umwandlung in Prozent
                    .ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler bei der Verarbeitung der Daten: " + ex.Message);
                return Array.Empty<int>();
            }
        }
    }
}
