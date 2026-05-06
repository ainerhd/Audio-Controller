using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MixerController.App.Services;
using MixerController.Core.Contracts;
using MixerController.Core.Models;

namespace MixerController.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly MixerRuntimeService runtimeService;
    private readonly ISettingsStore settingsStore;

    public MainViewModel(MixerRuntimeService runtimeService, ISettingsStore settingsStore)
    {
        this.runtimeService = runtimeService;
        this.settingsStore = settingsStore;
        this.runtimeService.ValuesUpdated += RuntimeServiceOnValuesUpdated;
        _ = InitializeAsync();
    }

    public ObservableCollection<ChannelStateViewModel> Channels { get; } = [];
    public ObservableCollection<AudioOutputDevice> Devices { get; } = [];
    public ObservableCollection<string> AvailablePorts { get; } = [];

    [ObservableProperty]
    private string? selectedPort;

    [ObservableProperty]
    private string channelCountText = "4";

    [ObservableProperty]
    private string bufferSizeText = "5";

    [ObservableProperty]
    private string deadZoneText = "5";

    [ObservableProperty]
    private string statusText = "Bereit.";

    [RelayCommand]
    private async Task StartAsync()
    {
        if (!TryReadParameters(out int channelCount, out int bufferSize, out int deadZone))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedPort))
        {
            StatusText = "Bitte Port auswählen.";
            return;
        }

        EnsureChannelCount(channelCount);
        IReadOnlyList<string> deviceIds = Channels.Select(c => c.SelectedDevice?.Id ?? string.Empty).ToArray();

        await runtimeService.StartAsync(SelectedPort, channelCount, bufferSize, deadZone, deviceIds);
        StatusText = $"Verbunden mit {SelectedPort}.";
    }

    [RelayCommand]
    private async Task StopAsync()
    {
        await runtimeService.StopAsync();
        StatusText = "Verbindung gestoppt.";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!TryReadParameters(out int channelCount, out int bufferSize, out int deadZone))
        {
            return;
        }

        MixerSettings settings = new()
        {
            PreferredPortName = SelectedPort,
            ChannelCount = channelCount,
            BufferSize = bufferSize,
            DeadZone = deadZone,
            Assignments = Channels.Select(c => new ChannelAssignment
            {
                Channel = c.ChannelNumber,
                DeviceId = c.SelectedDevice?.Id,
                DeviceName = c.SelectedDevice?.DisplayName
            }).ToList()
        };

        await settingsStore.SaveAsync(settings);
        StatusText = "Einstellungen gespeichert.";
    }

    [RelayCommand]
    private void RefreshPorts()
    {
        AvailablePorts.Clear();
        foreach (string port in runtimeService.GetPorts())
        {
            AvailablePorts.Add(port);
        }

        if (AvailablePorts.Count > 0 && string.IsNullOrWhiteSpace(SelectedPort))
        {
            SelectedPort = AvailablePorts[0];
        }
    }

    private async Task InitializeAsync()
    {
        RefreshPorts();

        Devices.Clear();
        foreach (AudioOutputDevice device in runtimeService.GetDevices())
        {
            Devices.Add(device);
        }

        MixerSettings settings = await settingsStore.LoadAsync();
        ChannelCountText = settings.ChannelCount.ToString();
        BufferSizeText = settings.BufferSize.ToString();
        DeadZoneText = settings.DeadZone.ToString();
        if (!string.IsNullOrWhiteSpace(settings.PreferredPortName))
        {
            SelectedPort = settings.PreferredPortName;
        }

        EnsureChannelCount(settings.ChannelCount);
        foreach (ChannelAssignment assignment in settings.Assignments)
        {
            ChannelStateViewModel? channel = Channels.FirstOrDefault(c => c.ChannelNumber == assignment.Channel);
            if (channel is null)
            {
                continue;
            }

            channel.SelectedDevice = Devices.FirstOrDefault(d => d.Id == assignment.DeviceId)
                ?? Devices.FirstOrDefault(d => d.DisplayName == assignment.DeviceName);
        }
    }

    private bool TryReadParameters(out int channelCount, out int bufferSize, out int deadZone)
    {
        channelCount = 0;
        bufferSize = 0;
        deadZone = 0;

        bool ok = int.TryParse(ChannelCountText, out channelCount)
                  && int.TryParse(BufferSizeText, out bufferSize)
                  && int.TryParse(DeadZoneText, out deadZone)
                  && channelCount is >= 1 and <= 16
                  && bufferSize is >= 1 and <= 50
                  && deadZone is >= 0 and <= 200;

        if (!ok)
        {
            StatusText = "Ungültige Parameter (Kanäle 1-16, Puffer 1-50, DeadZone 0-200).";
        }

        return ok;
    }

    private void EnsureChannelCount(int count)
    {
        if (count < 1)
        {
            count = 1;
        }

        while (Channels.Count < count)
        {
            Channels.Add(new ChannelStateViewModel(Channels.Count + 1)
            {
                SelectedDevice = Devices.FirstOrDefault()
            });
        }

        while (Channels.Count > count)
        {
            Channels.RemoveAt(Channels.Count - 1);
        }
    }

    private void RuntimeServiceOnValuesUpdated(object? sender, IReadOnlyList<int> values)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            for (int i = 0; i < values.Count && i < Channels.Count; i++)
            {
                Channels[i].Value = values[i];
            }
        });
    }
}
