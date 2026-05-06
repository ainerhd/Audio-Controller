using MixerController.Core.Contracts;
using NAudio.CoreAudioApi;

namespace MixerController.Audio;

public sealed class NaudioDeviceService : IAudioDeviceService
{
    private readonly MMDeviceEnumerator enumerator = new();
    private readonly Dictionary<string, MMDevice> devicesById = new(StringComparer.OrdinalIgnoreCase);
    private bool disposed;

    public IReadOnlyList<AudioOutputDevice> GetRenderDevices()
    {
        EnsureNotDisposed();

        devicesById.Clear();
        List<AudioOutputDevice> result = [];
        foreach (MMDevice device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
        {
            devicesById[device.ID] = device;
            result.Add(new AudioOutputDevice(device.ID, device.FriendlyName));
        }

        return result;
    }

    public bool TrySetVolume(string deviceId, int volumePercent)
    {
        EnsureNotDisposed();
        if (!devicesById.TryGetValue(deviceId, out MMDevice? device))
        {
            return false;
        }

        int clamped = Math.Clamp(volumePercent, 0, 100);
        float linear = clamped / 100f;
        float adjusted = (float)Math.Log10(1 + (9 * linear));
        device.AudioEndpointVolume.MasterVolumeLevelScalar = adjusted;
        return true;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        foreach (MMDevice device in devicesById.Values)
        {
            device.Dispose();
        }

        devicesById.Clear();
        enumerator.Dispose();
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
    }
}
