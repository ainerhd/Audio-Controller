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
                DrawLine(i + 1, 0);
            }
        }

        public void UpdateChannel(int channel, int percentage)
        {
            if (channel < 1 || channel > channelCount)
            {
                Console.WriteLine($"[WARN] Ungültiger Kanal: {channel}");
                return;
            }

            percentage = Math.Clamp(percentage, 0, 100);

            if (lastValues[channel - 1] != percentage) // Nur aktualisieren, wenn sich der Wert geändert hat
            {
                lastValues[channel - 1] = percentage;

                DrawLine(channel, percentage);
            }
        }

        private void DrawLine(int channel, int percentage)
        {
            const int barLength = 20;
            int filledLength = percentage * barLength / 100;
            string bar = new string('█', filledLength) + new string('░', barLength - filledLength);

            try
            {
                Console.SetCursorPosition(0, channel - 1);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"Kanal {channel}: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"[{bar}] ");
                Console.ResetColor();
                Console.Write($"{percentage,3}%  ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Fehler beim Aktualisieren der Konsole: {ex.Message}");
            }
        }
    }

}
