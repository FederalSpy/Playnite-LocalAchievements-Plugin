# Playnite Local Achievements Plugin

<p align="center">
  <img src="docs/images/settings-screen.png" alt="Local Achievements settings" width="900" />
</p>

<p align="center">
  <img alt="Playnite" src="https://img.shields.io/badge/Playnite-Desktop-1f6feb?style=for-the-badge" />
  <img alt="Platform" src="https://img.shields.io/badge/Platform-Windows-0ea5e9?style=for-the-badge" />
  <img alt="License" src="https://img.shields.io/badge/License-GPLv3-22c55e?style=for-the-badge" />
  <img alt="Status" src="https://img.shields.io/badge/Status-Learning%20Project-f59e0b?style=for-the-badge" />
</p>

> [!IMPORTANT]
> This project was built mostly with AI-assisted vibe-coding, with manual fixes while learning C#. It is a fun learning project and the code can be messy in some areas.

> [!IMPORTANT]
> Este proyecto fue hecho casi por completo con IA (vibe-coding), con correcciones manuales mientras aprendía C#. Es un proyecto de diversión/aprendizaje y el código puede ser desordenado en algunas partes.

## What It Does
- Reads local achievements from `.ini`/text files used by Steam emulator-style setups.
- Shows achievements inside Playnite.
- Displays unlock notifications with custom sound/themes.
- Current readers are tested mainly with CODEX/RUNE-style files.
- Designed to add more readers over time.

### Scope
- Intended for Steam-version game layouts that use `steam_api64.dll`.

## Qué Hace
- Lee logros locales desde archivos `.ini`/texto usados por configuraciones tipo emulador de Steam.
- Muestra logros dentro de Playnite.
- Lanza notificaciones de desbloqueo con sonido/tema personalizable.
- Los readers actuales están probados principalmente con formatos tipo CODEX/RUNE.
- Está pensado para ir añadiendo más readers con el tiempo.

### Alcance
- Pensado para juegos con estructura de versión de Steam que usan `steam_api64.dll`.

## Screenshots

| Achievements List | Settings |
|---|---|
| ![Achievements List](docs/images/achievements-list1.png) | ![Settings](docs/images/settings-screen.png) |

![Achievements List 2](docs/images/achievements-list2.png)

## Installation (Recommended): From Release `.pext`

> [!TIP]
> This is the easiest method for users with zero coding knowledge.

### English
1. Go to **Releases** in this repository.
2. Download the latest `Local-Achievements-<version>.pext`.
3. Open Playnite and drag/drop the `.pext` into Playnite.
4. Restart Playnite if requested.
5. Open `Settings -> Add-ons -> Generic -> Local Achievements`.
6. Configure your local achievement paths and save.

### Español
1. Ve a **Releases** en este repositorio.
2. Descarga el último `Local-Achievements-<versión>.pext`.
3. Abre Playnite y arrastra/suelta el `.pext` sobre Playnite.
4. Reinicia Playnite si lo pide.
5. Abre `Ajustes -> Complementos -> Genérica -> Local Achievements`.
6. Configura tus rutas de logros locales y guarda.

## Manual Installation (Alternative)

### English
If you install manually, copy plugin output files into `%APPDATA%\Playnite\Extensions\<PluginId>`.

### Español
Si instalas manualmente, copia los archivos de salida del plugin en `%APPDATA%\Playnite\Extensions\<PluginId>`.

## For Developers

```powershell
dotnet restore
dotnet build -c Release
powershell -ExecutionPolicy Bypass -File scripts/PackPext.ps1 -Configuration Release
```

Generated package:
- `dist/Local-Achievements-1.0.pext` (friendly name)

## Transparency

### English
- AI-assisted project (vibe-coding heavy).
- I am not a professional C# developer.
- GitHub beginner; improving over time.
- Forks and improvements are welcome.

### Español
- Proyecto asistido por IA (vibe-coding intensivo).
- No soy desarrollador profesional de C#.
- Novato en GitHub; iré mejorando con el tiempo.
- Se agradecen forks y mejoras.

## License
GNU GPL v3.0 (`LICENSE`).

## Contributing
Please read `CONTRIBUTING.md`.

## Security
Please read `SECURITY.md`.
