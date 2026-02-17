# Local Achievements

## Transparency / Transparencia

### English
This project was built **mostly with AI assistance** (vibe-coding), with manual fixes while learning basic C# on the way.

It is a **for-fun learning project**. I am not a professional C# developer.

The codebase is currently closer to **spaghetti code** than production-grade architecture. Some parts may be hard to read and may include bad practices. Sorry in advance.

I am also a **GitHub beginner**. I will do my best to improve this repository over time.

This project is open source so people who really know what they are doing can take over, fork, improve, and help the community.

My goal is simple: contribute a small grain of sand with my limited skills.

### Español
Este proyecto fue construido **casi por completo con ayuda de IA** (vibe-coding), con correcciones manuales mientras aprendía un poco de C# en el camino.

Es un proyecto hecho **por diversión y aprendizaje**. No soy desarrollador profesional de C#.

La base de código actualmente se parece más a **spaghetti code** que a una arquitectura de nivel producción. Algunas partes pueden ser poco legibles y tener malas prácticas. Perdón de antemano.

También soy **novato en GitHub**. Intentaré mejorar este repositorio lo mejor posible con el tiempo.

Este proyecto es open source para que gente que sí sabe lo que hace pueda tomar el control, hacer fork, mejorar y aportar a la comunidad.

Mi objetivo es simple: aportar un granito de arena con mis capacidades limitadas.

---

## What this plugin does / Qué hace este plugin

### English
- Reads local achievement progress from `.ini`/text files used by Steam emulator setups.
- Displays achievements inside Playnite and can show unlock notifications.
- Current focus has been tested mainly with **CODEX** and **RUNE** style files.
- Planned direction: add more readers over time.

Important scope:
- This plugin is intended for games that have the **Steam version structure** and use `steam_api64.dll`.

### Español
- Lee progreso de logros locales desde archivos `.ini`/texto usados por configuraciones tipo emulador de Steam.
- Muestra logros dentro de Playnite y puede lanzar notificaciones de desbloqueo.
- El enfoque actual está probado principalmente con formatos tipo **CODEX** y **RUNE**.
- Dirección futura: añadir más readers con el tiempo.

Alcance importante:
- Este plugin está pensado para juegos con estructura de versión de **Steam** y que usan `steam_api64.dll`.

## Screenshots / Capturas

> Save images in `docs/images/` (recommended names below) to display them in GitHub.

- `docs/images/achievements-list.png`
- `docs/images/settings-screen.png`

![Achievements List](docs/images/achievements-list.png)
![Settings Screen](docs/images/settings-screen.png)

## Installation (No coding required) / Instalación (Sin programar)

### English
1. Install Playnite (Desktop mode).
2. Download the plugin files from this repository (or from a Release if available).
3. Create this folder if it does not exist:
   - `%APPDATA%\Playnite\Extensions\LocalAchievements`
4. Copy plugin files there (at minimum `LocalAchievements.dll`, `extension.yaml`, and `Themes` folder).
5. Restart Playnite.
6. Open: `Settings > Add-ons > Generic > Local Achievements`.
7. Configure local paths and save.

### Español
1. Instala Playnite (modo Desktop).
2. Descarga los archivos del plugin desde este repositorio (o desde un Release si existe).
3. Crea esta carpeta si no existe:
   - `%APPDATA%\Playnite\Extensions\LocalAchievements`
4. Copia ahí los archivos del plugin (mínimo `LocalAchievements.dll`, `extension.yaml` y carpeta `Themes`).
5. Reinicia Playnite.
6. Abre: `Ajustes > Complementos > Genérica > Local Achievements`.
7. Configura las rutas locales y guarda.

## Build (Developers)

Requirements:
- Windows
- .NET SDK
- Playnite installed (runtime testing)

Commands:
```powershell
dotnet restore
dotnet build
```

## License
This project is licensed under **GNU GPL v3.0**.

What this means in practice:
- Anyone can use, fork, modify, and redistribute.
- If someone redistributes modified versions, source code must remain available under GPL.
- Commercial use is allowed by GPL, but derivative distribution must keep GPL obligations.

## Contributing
Forks and pull requests are welcome.

Please read `CONTRIBUTING.md`.

## Security
If you find a security issue, please read `SECURITY.md`.
