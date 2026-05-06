using MixerController.Core.Models;

namespace MixerController.Core.Contracts;

public interface IMixerFrameParser
{
    bool TryParse(string rawLine, int expectedChannels, out MixerFrame? frame);
}
