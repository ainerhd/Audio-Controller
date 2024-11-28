using System;
using System.Collections.Generic;
using System.Linq;

public class DataProcessor
{
    private readonly int channelCount;
    private readonly Queue<int>[] valueBuffers;

    public int BufferSize { get; set; } = 5; // Anzahl der Werte für den gleitenden Durchschnitt

    public DataProcessor(int channelCount)
    {
        this.channelCount = channelCount;
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
                int currentValue = int.Parse(rawValues[i]);

                // Füge neuen Wert zum Puffer hinzu
                var buffer = valueBuffers[i];
                buffer.Enqueue(currentValue);

                // Entferne ältesten Wert, wenn der Puffer zu groß wird
                if (buffer.Count > BufferSize)
                {
                    buffer.Dequeue();
                }

                // Berechne den Durchschnitt des Puffers
                smoothedValues[i] = (int)buffer.Average() * 100 / 1023;
            }

            return smoothedValues;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fehler bei der Verarbeitung der Daten: " + ex.Message);
            return Array.Empty<int>();
        }
    }
}