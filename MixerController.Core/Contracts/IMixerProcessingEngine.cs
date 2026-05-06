using MixerController.Core.Models;

namespace MixerController.Core.Contracts;

public interface IMixerProcessingEngine
{
    ProcessedMixerFrame Process(MixerFrame frame);
}
