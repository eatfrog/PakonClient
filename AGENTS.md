# AGENTS.md

.NET 10 Windows client for the Kodak Pakon F-135 Plus film scanner. Long-term
direction: direct driver communication with no COM dependency. Until the packet
protocol is recovered, the 32-bit TLX COM stack is used only through an
isolated bridge process.

## Build and verify

- `dotnet build .\Pakon.sln` is the only build target. There are **no test
  projects**; verification = a clean build (several projects set
  `TreatWarningsAsErrors`).
- Windows-only; requires the .NET 10 SDK (`net10.0` / `net10.0-windows`, WPF).
- Version lives only in `Directory.Build.props`; `CHANGELOG.md` has one
  section per release. Keep both in sync when bumping.

## Project boundaries

- `Pakon.sln` builds the new app only. `src/Legacy/` is the preserved
  reference implementation with its own `src/Legacy/PakonClient.sln` — do not
  wire it into `Pakon.sln`.
- `Pakon.Scanner` — application-facing scanner boundary; no TLX types.
- `Pakon.Scanner.Legacy` — the only project allowed to know TLX operation
  values or save-control flags; implements the boundary over the bridge.
- `Pakon.LegacyBridge` — x86-only exe hosting the TLX COM interop. References
  `Interop.TLXLib.dll` from the installed Pakon COM server
  (`$(ProgramFiles32)\Pakon\F-X35 COM SERVER\`; override with
  `-p:PakonTlxInteropPath=...`). Builds into `bin\x86\...`.
- `Pakon.LegacyBridge.Protocol` — `netstandard2.0`, C# 7.3, implicit usings
  and nullable disabled, because it is shared with the x86 bridge. Keep it
  compilable there.
- `Pakon.Transport` — direct driver access. Intentionally exposes only
  documented read-only metadata (driver-version IOCTL `0x222074`, read-only
  status query via `0x222090`). Do not add scanner packet or motion commands
  here.
- `Pakon.RawImageConverter` — a separate exe the WPF client launches as a
  process (`--input/--output/--format`); referenced with
  `ReferenceOutputAssembly="false"`. Do not turn it into a library call.
- `Pakon.Client` — WPF app (assembly name `Pakon`); its `app.manifest`
  requires administrator.

## Running against hardware (always elevated)

- `dotnet run --project .\src\Pakon.Client` — auto-starts the bridge via the
  32-bit host `C:\Program Files (x86)\dotnet\dotnet.exe` on pipe
  `PakonLegacyBridge`. `BridgeProcessHost` finds `Pakon.LegacyBridge.dll` by
  walking up from the app directory looking for `Pakon.sln`, with a hardcoded
  `C:\Code\PakonClient` fallback.
- `dotnet run --project .\src\Pakon.Transport.Cli` — safe probe; by default
  sends only the read-only driver-version IOCTL (no scanner commands, no
  hardware motion).
- Full TLX trace recording is a manual two-window sequence (start bridge
  elevated, then `--run-tlx-trace`); see `Readme.md`.

## Hard constraints

- **Firmware update is permanently prohibited.** Never pass
  `INITIALIZE_FirmwareUpdate` (`0x2`) to TLC, issue firmware packets, or add
  firmware probes.
- Follow the evidence rules in `docs/research-notes.md`: type-library enum or
  method names are labels, not proof of effect; a driver command becomes a
  managed candidate only after its exact bytes, response validation, and
  purpose are recovered. Hardware tests validate static conclusions, they do
  not replace them.
- Model names alone must not select low-level scanner commands; the
  per-variant capability matrix is unresolved.
- TLX types must not appear in the new app's public API; nothing outside
  `Pakon.LegacyBridge` references the COM interop.

## Reference docs

- `docs/tlx.md`, `docs/tlx-lowlevel.md`, `docs/tlx-colour.md` — hardware/COM
  behavioural reference (current source of truth).
- `docs/research-notes.md` — replacement work queue and evidence rules. Its
  backend-selection row predates the resolved F135 TLA hand-off; where it
  conflicts, the tlx docs win.
