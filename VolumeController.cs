using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audio_Controller
{
    public class VolumeController
    {
        public void SetVolume(int[] percentages)
        {
            if (percentages.Length > 0)
            {
                int volume = percentages[0]; // Nutze den ersten Kanal zur Lautstärkeregelung
                Console.WriteLine($"Lautstärke auf {volume}% gesetzt (noch nicht implementiert).");

                // Hier später Systemlautstärke steuern
                // z. B. über CoreAudioAPI (Windows-spezifisch)
            }
        }
    }
}
