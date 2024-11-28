using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audio_Controller
{
    public class ConsoleUpdater
    {
        private readonly int[] lastValues;
        private readonly int channelCount;

        public ConsoleUpdater(int channelCount)
        {
            this.channelCount = channelCount;
            lastValues = new int[channelCount];

            Console.Clear();
            for (int i = 0; i < channelCount; i++)
            {
                Console.WriteLine($"Kanal {i + 1}: 0%");
            }
        }

        public void UpdateChannel(int channel, int percentage)
        {
            if (lastValues[channel - 1] != percentage) // Nur aktualisieren, wenn sich der Wert geändert hat
            {
                lastValues[channel - 1] = percentage;

                Console.SetCursorPosition(0, channel - 1);
                Console.Write($"Kanal {channel}: {percentage}%  "); // Füge Leerzeichen hinzu, um alte Werte zu überschreiben
            }
        }
    }

}
