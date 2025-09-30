# LicenseServer Build & Development

This repository ships with two build scripts:

- `build.sh` (recommended on Linux, macOS, and also on Windows via Git Bash / MSYS2 / WSL)
- `build.ps1` (PowerShell for Windows, or cross‑platform if you have `pwsh` installed)

## Fastest path (build + immediately run)
Build only the local platform and run it right away:
```bash
./build.sh --run
```
(If necessary the first time: `chmod +x build.sh`)

The script auto-detects your local Runtime Identifier (RID) (e.g. `linux-x64`, `osx-arm64`, `win-x64`) and only builds that one by default (faster than building everything).

Build all standard RIDs (win-x64, linux-x64, osx-arm64, osx-x64):
```bash
./build.sh --all
```
Or explicitly define a list:
```bash
RIDS="linux-x64 win-x64" ./build.sh
```

## Flags & Help
```bash
./build.sh --help
```
Summary:
- `--run`  -> Runs the produced binary for the current platform after publish
- `--all`  -> Builds all standard RIDs
- `--help` -> Shows help

## Environment Variables
| Variable         | Default                               | Description |
|------------------|----------------------------------------|-------------|
| CONFIGURATION    | Release                                | Build configuration |
| SELF_CONTAINED   | true                                   | Self-contained publish |
| SINGLE_FILE      | true                                   | Produce single-file executable |
| PUBLISH_DIR      | publish                                | Base output directory |
| RIDS             | (empty)                                | Overrides RID selection completely |
| BUILD_ALL_RIDS   | false                                  | Same as `--all` |
| RETRY_ATTEMPTS   | 8                                      | Retries for dotnet commands |
| RETRY_DELAY      | 2                                      | Seconds between retries |

Examples:
```bash
# Build Debug instead of Release
CONFIGURATION=Debug ./build.sh --run

# Disable single-file
SINGLE_FILE=false ./build.sh

# Only publish linux-x64 explicitly
RIDS="linux-x64" ./build.sh

# Build all RIDs without running
./build.sh --all
```

## What the script does
1. Detects the local RID (unless `--all` or `RIDS=` provided)
2. Ensures required NuGet packages (idempotent): Raylib-cs, System.Text.Json, DotNetEnv
3. Restore + build (Release by default)
4. Publish (self-contained, optional single-file) into `publish/<rid>`
5. Copies: `config.json`, `licenses.json`, `.env` (if present)
6. Optionally runs the app (`--run`)

## Binary / Execution
After a simple build (Linux example):
```
publish/linux-x64/LicenseServer
```
macOS (Intel / ARM):
```
publish/osx-x64/LicenseServer
publish/osx-arm64/LicenseServer
```
Windows:
```
publish/win-x64/LicenseServer.exe
```

## Raylib notes (Linux/macOS)
If startup errors mention missing X11 / GL / audio libraries, install (Ubuntu example):
```bash
sudo apt update
sudo apt install -y libx11-6 libxcursor1 libxrandr2 libxinerama1 libxi6 libgl1 libasound2
```

## Security warning: System.Text.Json
Currently (version 8.0.0) `dotnet` emits security advisories (NU1903). Upgrading is recommended:
```bash
dotnet add package System.Text.Json --version 8.0.5
```
(Check versions first: `dotnet list package --outdated`)

## Common Issues
| Problem | Cause | Fix |
|---------|-------|-----|
| "command not found: dotnet" | .NET SDK missing | Install SDK from https://dotnet.microsoft.com/download |
| Nothing in publish/ | Build not run / wrong RID | Re-run `./build.sh`, check output |
| Raylib window fails over SSH | No X forwarding / display | Run locally or configure X11 forwarding |
| NU1903 warnings | Vulnerable package version | Upgrade (see above) |

## Possible Next Steps
- Add GitHub Actions workflow
- Upgrade System.Text.Json
- Generate artifact hashes / checksums
- Try Trim / AOT (careful with native libs)

Feel free to ask if you need anything else.
