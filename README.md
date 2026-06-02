# Playnite Local Achievements Plugin

<p align="center">
  <img src="docs/images/banner.png" alt="Local Achievements settings" width="1200" />
</p>

<p align="center">
  <a href="https://github.com/JosefNemec/Playnite">
    <img alt="Playnite" src="https://img.shields.io/badge/Playnite-Desktop-1f6feb?style=for-the-badge" />
  </a>
  <img alt="Platform" src="https://img.shields.io/badge/Platform-Windows-0ea5e9?style=for-the-badge" />
  <img alt="License" src="https://img.shields.io/badge/License-GPLv3-22c55e?style=for-the-badge" />
  <img alt="Status" src="https://img.shields.io/badge/Status-Learning%20Project-f59e0b?style=for-the-badge" />
</p>

> [!IMPORTANT]
> This project was built mostly with AI-assisted vibe-coding, with manual fixes while learning C#. It is a fun learning project and the code can be messy in some areas.

## What It Does

- Reads local achievements from `.ini`/text files used by Steam emulator-style setups.
- Shows achievements inside Playnite.
- Displays unlock notifications with custom sound/themes.
- Includes language selection: English and Spanish.
- Current readers are tested mainly with CODEX/RUNE-style files.
- Designed to add more readers over time.

### Scope

- Intended for Steam-version game layouts that use `steam_api64.dll`.

## Screenshots

| Achievements List | Achievements List 2 | Settings |
|---|---|---|
| ![Achievements List](docs/images/achievements-list1.png) | ![Achievements List 2](docs/images/achievements-list2.png) | ![Settings](docs/images/settings-screen.png) |

## Installation (Recommended): From Release `.pext`

> [!TIP]
> This is the easiest method for users with zero coding knowledge.

1. Go to **Releases** in this repository.
2. Download the latest `Local-Achievements-<version>.pext`.
3. Open Playnite and drag/drop the `.pext` into Playnite.
4. Restart Playnite if requested.
5. Open `Settings -> Add-ons -> Generic -> Local Achievements`.
6. Configure your local achievement paths and save.

## Manual Installation (Alternative)

If you install manually, copy plugin output files into `%APPDATA%\Playnite\Extensions\<PluginId>`.

## For Developers

```powershell
dotnet restore
dotnet build -c Release
powershell -ExecutionPolicy Bypass -File scripts/PackPext.ps1 -Configuration Release
```

Generated package:
- `dist/Local-Achievements-1.0.1.pext` (friendly name)

## Transparency

- AI-assisted project (vibe-coding heavy).
- I am not a professional C# developer.
- GitHub beginner; improving over time.
- Forks and improvements are welcome.

## License

GNU GPL v3.0 (`LICENSE`).

## Contributing

Please read `CONTRIBUTING.md`.

## Security

Please read `SECURITY.md`.
