using MixerController.Core.Contracts;
using MixerController.Core.Models;

namespace MixerController.Core.Processing;

public sealed class DelimitedMixerFrameParser : IMixerFrameParser
{
    public bool TryParse(string rawLine, int expectedChannels, out MixerFrame? frame)
    {
        frame = null;
        if (string.IsNullOrWhiteSpace(rawLine) || expectedChannels <= 0)
        {
            return false;
        }

        string[] segments = rawLine.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != expectedChannels)
        {
            return false;
        }

        var values = new List<int>(expectedChannels);
        foreach (string segment in segments)
        {
            if (!int.TryParse(segment, out int value))
            {
                return false;
            }

            values.Add(Math.Clamp(value, 0, 1023));
        }

        frame = new MixerFrame(values);
        return true;
    }
}
