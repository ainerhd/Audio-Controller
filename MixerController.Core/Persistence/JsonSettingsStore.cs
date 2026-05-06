using System.Text.Json;
using MixerController.Core.Contracts;
using MixerController.Core.Models;

namespace MixerController.Core.Persistence;

public sealed class JsonSettingsStore : ISettingsStore
{
    private readonly string path;
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonSettingsStore(string path)
    {
        this.path = path;
    }

    public async Task<MixerSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
        {
            return new MixerSettings();
        }

        await using FileStream stream = File.OpenRead(path);
        MixerSettings? settings = await JsonSerializer.DeserializeAsync<MixerSettings>(stream, Options, cancellationToken);
        return settings ?? new MixerSettings();
    }

    public async Task SaveAsync(MixerSettings settings, CancellationToken cancellationToken = default)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using FileStream stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, settings, Options, cancellationToken);
    }
}
