namespace MixerController.Core.Contracts;

public interface IAudioDeviceService : IDisposable
{
    IReadOnlyList<AudioOutputDevice> GetRenderDevices();
    bool TrySetVolume(string deviceId, int volumePercent);
}
