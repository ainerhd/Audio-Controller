namespace MixerController.Core.Models;

public sealed class ChannelAssignment
{
    public int Channel { get; init; }
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }
}
