using System;
using System.Collections.Generic;
using System.Linq;

public class DataProcessor
{
    private readonly int channelCount;
    private readonly Queue<int>[] valueBuffers;

    private int bufferSize = 5;
    private int deadZone = 5;

    public int BufferSize
    {
        get => bufferSize;
        set => bufferSize = Math.Max(1, value);
    }

    public int DeadZone
    {
        get => deadZone;
        set => deadZone = Math.Min(Math.Max(value, 0), 1023);
    }

    public DataProcessor(int channelCount, int bufferSize = 5, int deadZone = 5)
    {
        this.channelCount = channelCount;
        BufferSize = bufferSize;
        DeadZone = deadZone;
        valueBuffers = new Queue<int>[channelCount];

        // Initialisiere den Puffer für jeden Kanal
        for (int i = 0; i < channelCount; i++)
        {
            valueBuffers[i] = new Queue<int>();
        }
    }

    public int[] Process(string rawData)
    {
        try
        {
            // Trenne die Werte anhand des Trennzeichens " | "
            string[] rawValues = rawData.Split('|').Select(v => v.Trim()).ToArray();

            if (rawValues.Length != channelCount)
            {
                Console.WriteLine("Fehler: Anzahl der empfangenen Kanäle stimmt nicht mit der Konfiguration überein.");
                return Array.Empty<int>();
            }

            // Verarbeite jeden Kanal
            int[] smoothedValues = new int[channelCount];
            for (int i = 0; i < channelCount; i++)
            {
                if (!int.TryParse(rawValues[i], out int currentValue))
                {
                    Console.WriteLine($"[WARN] Ungültiger Wert '{rawValues[i]}' im Kanal {i + 1}.");
                    currentValue = 0;
                }

                // Füge neuen Wert zum Puffer hinzu
                var buffer = valueBuffers[i];
                buffer.Enqueue(currentValue);

                // Entferne ältesten Wert, wenn der Puffer zu groß wird
                if (buffer.Count > BufferSize)
                {
                    buffer.Dequeue();
                }

                // Berechne den Durchschnitt des Puffers
                int averageValue = (int)buffer.Average();

                // Umrechnung mit Deadzone
                smoothedValues[i] = ConvertToPercentageWithDeadzone(averageValue);
            }

            return smoothedValues;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fehler bei der Verarbeitung der Daten: " + ex.Message);
            return Array.Empty<int>();
        }
    }

    private int ConvertToPercentageWithDeadzone(int adcValue)
    {
        if (adcValue <= DeadZone - 1)
        {
            return 0; // Deadzone für 0%
        }
        if (adcValue >= 1020)
        {
            return 100; // Deadzone für 100%
        }

        // Wertebereich 5–1019 → 1%–99%
        return (adcValue - DeadZone) * 100 / (1023 - DeadZone);
    }
}
