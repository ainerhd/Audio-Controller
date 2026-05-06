using CommunityToolkit.Mvvm.ComponentModel;
using MixerController.Core.Contracts;

namespace MixerController.App.ViewModels;

public partial class ChannelStateViewModel : ObservableObject
{
    [ObservableProperty]
    private int value;

    [ObservableProperty]
    private AudioOutputDevice? selectedDevice;

    public ChannelStateViewModel(int channelNumber)
    {
        ChannelNumber = channelNumber;
    }

    public int ChannelNumber { get; }
    public string Name => $"Kanal {ChannelNumber}";
    public string ValueText => $"{Value}%";

    partial void OnValueChanged(int oldValue, int newValue)
    {
        OnPropertyChanged(nameof(ValueText));
    }
}
