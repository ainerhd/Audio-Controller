using MixerController.Core.Models;

namespace MixerController.Core.Contracts;

public interface ISettingsStore
{
    Task<MixerSettings> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(MixerSettings settings, CancellationToken cancellationToken = default);
}
