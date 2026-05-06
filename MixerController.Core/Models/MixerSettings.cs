namespace MixerController.Core.Models;

public sealed class MixerSettings
{
    public string? PreferredPortName { get; set; }
    public int ChannelCount { get; set; } = 4;
    public int BufferSize { get; set; } = 5;
    public int DeadZone { get; set; } = 5;
    public List<ChannelAssignment> Assignments { get; set; } = [];
}
