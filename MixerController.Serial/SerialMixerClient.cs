using System.IO.Ports;
using MixerController.Core.Contracts;

namespace MixerController.Serial;

public sealed class SerialMixerClient : ISerialMixerClient
{
    private SerialPort? serialPort;

    public event EventHandler<string>? LineReceived;

    public bool IsConnected => serialPort is { IsOpen: true };

    public Task ConnectAsync(string portName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsConnected)
        {
            return Task.CompletedTask;
        }

        serialPort = new SerialPort(portName, 9600)
        {
            ReadTimeout = 500,
            WriteTimeout = 500,
            DtrEnable = true,
            RtsEnable = true,
            NewLine = "\n"
        };

        serialPort.DataReceived += OnDataReceived;
        serialPort.Open();
        return Task.CompletedTask;
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (serialPort is null)
        {
            return Task.CompletedTask;
        }

        serialPort.DataReceived -= OnDataReceived;
        if (serialPort.IsOpen)
        {
            serialPort.Close();
        }

        serialPort.Dispose();
        serialPort = null;
        return Task.CompletedTask;
    }

    public IReadOnlyList<string> GetAvailablePorts()
    {
        return SerialPort.GetPortNames().OrderBy(static p => p).ToArray();
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (serialPort is null || !serialPort.IsOpen)
        {
            return;
        }

        try
        {
            string? line = serialPort.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(line))
            {
                LineReceived?.Invoke(this, line);
            }
        }
        catch (TimeoutException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }
}
