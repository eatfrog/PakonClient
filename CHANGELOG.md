# Changelog

## 0.3.0 — 2026-07-30

### 16-bit image export

- Corrected the 16-bit PNG conversion so it no longer applies an additional
  display gamma curve that made exported images appear washed out.
- Added an export-format notice explaining that 16-bit PNG files use raw
  scanner data without Pakon's internal color adjustments, so their color and
  tone may not exactly match the processed preview.
- Kept quality-95 JPEG export aligned with the internally color-adjusted
  preview workflow.

## 0.2.0 — 2026-07-29

### Installation and startup

- Bundled the required 32-bit Visual C++ 7.1 runtime files (`msvcr71.dll`
  and `msvcp71.dll`) with Pakon Client. Users no longer need to copy them
  into the Pakon COM Server directory.
- Added startup validation for the registered Pakon F-X35 COM server,
  `PakonImau.dll`, and its known native dependencies.
- Added a dedicated installation-error dialog that lists missing native
  files instead of presenting the failure as a scanner power or USB problem.
- Added the application and standard Pakon installation directories to the
  legacy bridge's process-local native DLL search path.

### Scanning architecture

- Added a scanner workflow API that keeps scanner operations and models
  independent of the legacy TLX bridge.
- Moved TLX-specific scan, render, state, and save-control translation into
  the legacy scanner backend.
- Updated the desktop client to use one scanner workflow for initialization,
  capture, recovery, preview generation, export, and shutdown.
- Split bridge and raw-converter discovery out of the main window.

### Image adjustments and export

- Replaced the percentage brightness control with an exposure control measured
  in stops.
- Applied exposure in linear-light sRGB space and introduced a dedicated
  midpoint-preserving contrast curve.
- Applied the revised exposure and contrast behavior consistently to JPEG and
  16-bit PNG output.
- Replaced the legacy raw image converter project with the current managed
  `Pakon.RawImageConverter` executable.

## 0.1.0 — 2026-07-27

- First packaged Windows preview of the WPF Pakon scanning client.
