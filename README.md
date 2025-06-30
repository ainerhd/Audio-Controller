# Audio Controller

This project uses WinForms and the .NET Framework to control audio devices.

## Setup

Run `setup.sh` to install Mono, download NuGet, restore packages, and build the solution.

```bash
./setup.sh
```

After a successful build you can run the application with `mono`:

```bash
mono "Audio Controller/bin/Debug/Audio Controller.exe"
```

### Features

- **Automatic COM port detection**: leave the COM port field empty and the application will search for a mixer via handshake.
- **Stop button** to close the serial connection.
- **Configurable smoothing** (`Puffergröße`) and `DeadZone` values.
- **Settings persistence** for COM port, channel mapping and smoothing parameters.
- **Console mode**: run the program with command line arguments instead of the GUI.
- **Dynamic device list**: available audio devices refresh when selecting a device.

Example for console mode:

```bash
mono "Audio Controller/bin/Debug/Audio Controller.exe" auto 2 "Speakers" "Headphones"
```
