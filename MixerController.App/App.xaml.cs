using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MixerController.App.Services;
using MixerController.App.ViewModels;
using MixerController.Audio;
using MixerController.Core.Contracts;
using MixerController.Core.Persistence;
using MixerController.Serial;
using Serilog;

namespace MixerController.App;

public partial class App : Application
{
    private IHost? host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string basePath = Path.Combine(appData, "MixerController");
        Directory.CreateDirectory(basePath);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(Path.Combine(basePath, "logs", "app-.log"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices(services =>
            {
                services.AddSingleton<ISettingsStore>(new JsonSettingsStore(Path.Combine(basePath, "settings.json")));
                services.AddSingleton<ISerialMixerClient, SerialMixerClient>();
                services.AddSingleton<IAudioDeviceService, NaudioDeviceService>();
                services.AddSingleton<MixerRuntimeService>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();

        await host.StartAsync();
        MainWindow = host.Services.GetRequiredService<MainWindow>();
        MainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (host is not null)
        {
            await host.StopAsync();
            host.Dispose();
        }

        Log.CloseAndFlush();
        base.OnExit(e);
    }
}

