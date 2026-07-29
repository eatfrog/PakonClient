# PakonClient

## About the Kodak Pakon

The Pakon is an automated, high-throughput film-scanner family made for photo
lab and minilab workflows. It transports film through the scanner, detects and
frames images, applies the Pakon imaging pipeline, and produces files for
printing or delivery. The family handles 135 (35 mm) film and, on supported
models, APS film; the F-135 manual specifically describes 35 mm colour-negative
strip scanning. [Contemporary product material](https://tribex.ch/tribex_english/pakon.php)
describes the scanners as minilab, kiosk, or standalone equipment, while the
[F-135/F-135 Plus manual](https://usermanual.wiki/Document/UserManualF135.1429007035.pdf)
documents the compact F-135 model.

The recovered software identifies these model families and variants:

| Family | Variants represented by the software |
| --- | --- |
| F-135 | F-135, F-135 Plus |
| F-235 | F-235, F-235 Plus, F-235C |
| F-335 | F-335, F-335C |

This repository currently targets an F-135 Plus. The exact capability matrix
for every model variant remains a hardware-specific question; model names alone
must not be used to select low-level scanner commands.

PakonClient is a new .NET 10 scanner application for the Pakon F-135 Plus.
The production direction is direct driver communication without a COM
dependency. Until the device packet protocol is recovered and validated,
`Pakon.Transport` intentionally exposes only documented, read-only driver
metadata access.

The installed TLX COM server is retained only as a behavioural reference. All
temporary TLX use is isolated in `Pakon.LegacyBridge`, an x86 process reached
through named pipes. The new application does not expose TLX types in its
public API.

The application-facing scanner boundary is defined in `Pakon.Scanner`.
`Pakon.Scanner.Legacy` implements that contract through the existing bridge
and is the only scanner-workflow project that knows TLX operation values or
save-control flags. `Pakon.Client` selects one workflow backend for the whole
scanner session, allowing a managed F-135 backend to be introduced beside the
legacy implementation without changing the UI workflow.

## Project history and acknowledgements

This project began by preserving and studying the existing Pakon software
interfaces: the 32-bit TLX COM facade, its managed wrapper, and the scanner
driver. That work established a controlled reference path for observing a real
scan while the new application recovers the direct driver protocol.

The project builds on the important Pakon driver reverse-engineering work by
[Kai Kaufman](https://ktkaufman03.github.io/blog/2022/09/04/pakon-reverse-engineering/),
including the [FX35 driver source](https://github.com/ktkaufman03/FX35). His
research made the modern direct-driver direction substantially more practical.

## Reference documentation

- [TLX facade and observed scanner behaviour](docs/tlx.md)
- [Low-level driver and ABI evidence](docs/tlx-lowlevel.md)
- [Colour pipeline research](docs/tlx-colour.md)

## Build and safe transport probe

```powershell
dotnet build .\Pakon.sln
dotnet run --project .\src\Pakon.Transport.Cli
```

The default CLI probe uses only `IOCTL_EZUSB_GET_DRIVER_VERSION`
(`0x222074`). It does not send scanner packet commands or move hardware.

## TLX reference recording

TLX is 32-bit, so the bridge must run elevated under the 32-bit .NET host.
Build the solution, then start the bridge in one elevated PowerShell window:

```powershell
& 'C:\Program Files (x86)\dotnet\dotnet.exe' C:\Code\PakonClient\src\Pakon.LegacyBridge\bin\x86\Debug\net10.0-windows\Pakon.LegacyBridge.dll --pipe PakonLegacyBridge
```

In another elevated PowerShell window, run the controller. It initializes the
scanner, waits until it is ready, prompts before film motion, scans the fixed
baseline profile, promotes the roll, saves JPEGs, and closes the session.

```powershell
dotnet run --project .\src\Pakon.Transport.Cli -- --run-tlx-trace C:\PakonTraces\roll-001
```

Add `--capture-fileio-etw` to collect an optional passive Windows ETW light
`FileIO` timeline. It correlates PFS file activity with TLX callbacks but does
not expose `DeviceIoControl` payload bytes.

The recording retains a versioned manifest, timestamped callback stream,
metadata snapshots, and runtime evidence. It drains the native TLX error queue
after callback errors. PFS cleanup is limited to buffers created or changed by
the bridge's own session; existing unchanged buffers are left alone. The
recorder observes the known facade only: it neither hooks the driver nor
records or replays scanner packets or bulk pixels.

## Windows desktop application

`Pakon.Client` is the .NET 10 WPF scanning application. It provides automatic
scanner initialization, Base 16 scanning, paged 3x3 previews, multi-frame
editing, per-frame inclusion, and 16-bit PNG or quality-95 JPEG export.
Filename prefixes are optional; DX frame names are preferred over sequential
numbering.

Build and launch from an elevated Windows terminal:

```powershell
dotnet build .\Pakon.sln
dotnet run --project .\src\Pakon.Client
```

The application starts `Pakon.LegacyBridge` automatically with the 32-bit .NET
host. The 32-bit .NET runtime, registered Pakon COM server, and its native
runtime files are required until the remaining legacy DLL implementations are
replaced.
