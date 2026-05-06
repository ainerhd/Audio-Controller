namespace MixerController.Core.Contracts;

public interface ISerialMixerClient : IAsyncDisposable
{
    event EventHandler<string>? LineReceived;
    bool IsConnected { get; }
    Task ConnectAsync(string portName, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    IReadOnlyList<string> GetAvailablePorts();
}
