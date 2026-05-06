using MixerController.Core.Contracts;
using MixerController.Core.Models;
using MixerController.Core.Processing;

namespace MixerController.App.Services;

public sealed class MixerRuntimeService
{
    private readonly ISerialMixerClient serialClient;
    private readonly IAudioDeviceService audioDeviceService;
    private readonly IMixerFrameParser parser = new DelimitedMixerFrameParser();
    private IMixerProcessingEngine? engine;
    private IReadOnlyList<string> mappedDeviceIds = [];
    private int channelCount;

    public MixerRuntimeService(ISerialMixerClient serialClient, IAudioDeviceService audioDeviceService)
    {
        this.serialClient = serialClient;
        this.audioDeviceService = audioDeviceService;
        this.serialClient.LineReceived += OnLineReceived;
    }

    public event EventHandler<IReadOnlyList<int>>? ValuesUpdated;

    public IReadOnlyList<AudioOutputDevice> GetDevices() => audioDeviceService.GetRenderDevices();

    public IReadOnlyList<string> GetPorts() => serialClient.GetAvailablePorts();

    public async Task StartAsync(string port, int channels, int bufferSize, int deadZone, IReadOnlyList<string> targetDeviceIds)
    {
        channelCount = channels;
        mappedDeviceIds = targetDeviceIds;
        engine = new MixerProcessingEngine(channels, bufferSize, deadZone);
        await serialClient.ConnectAsync(port);
    }

    public Task StopAsync()
    {
        return serialClient.DisconnectAsync();
    }

    private void OnLineReceived(object? sender, string line)
    {
        if (engine is null)
        {
            return;
        }

        if (!parser.TryParse(line, channelCount, out MixerFrame? frame) || frame is null)
        {
            return;
        }

        ProcessedMixerFrame processed = engine.Process(frame);
        for (int i = 0; i < processed.PercentValues.Count && i < mappedDeviceIds.Count; i++)
        {
            string deviceId = mappedDeviceIds[i];
            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                _ = audioDeviceService.TrySetVolume(deviceId, processed.PercentValues[i]);
            }
        }

        ValuesUpdated?.Invoke(this, processed.PercentValues);
    }
}
