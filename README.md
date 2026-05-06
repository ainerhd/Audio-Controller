# Mixer Controller Rewrite

Modern rewrite of mixer app for Windows.

## Stack

- .NET 8
- WPF + MVVM
- NAudio
- SerialPort
- Serilog

## Solution

- `MixerController.Rewrite.sln`
- `MixerController.Core`
- `MixerController.Serial`
- `MixerController.Audio`
- `MixerController.App`

## Build

```bash
dotnet build "MixerController.Rewrite.sln"
```

## Run

```bash
dotnet run --project "MixerController.App"
```
