using MixerController.Core.Contracts;
using MixerController.Core.Models;

namespace MixerController.Core.Processing;

public sealed class MixerProcessingEngine : IMixerProcessingEngine
{
    private readonly Queue<int>[] buffers;
    private readonly int bufferSize;
    private readonly int deadZone;

    public MixerProcessingEngine(int channelCount, int bufferSize, int deadZone)
    {
        if (channelCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(channelCount));
        }

        this.bufferSize = Math.Max(1, bufferSize);
        this.deadZone = Math.Clamp(deadZone, 0, 1000);
        buffers = Enumerable.Range(0, channelCount).Select(_ => new Queue<int>()).ToArray();
    }

    public ProcessedMixerFrame Process(MixerFrame frame)
    {
        if (frame.RawValues.Count != buffers.Length)
        {
            throw new ArgumentException("Channel count mismatch.", nameof(frame));
        }

        int[] values = new int[buffers.Length];
        for (int i = 0; i < frame.RawValues.Count; i++)
        {
            Queue<int> queue = buffers[i];
            queue.Enqueue(frame.RawValues[i]);
            while (queue.Count > bufferSize)
            {
                _ = queue.Dequeue();
            }

            int average = (int)Math.Round(queue.Average());
            values[i] = ToPercent(average);
        }

        return new ProcessedMixerFrame(values);
    }

    private int ToPercent(int value)
    {
        if (value <= deadZone)
        {
            return 0;
        }

        if (value >= 1020)
        {
            return 100;
        }

        return Math.Clamp((value - deadZone) * 100 / (1023 - deadZone), 0, 100);
    }
}
