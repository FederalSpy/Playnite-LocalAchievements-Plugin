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
> Este proyecto fue hecho casi por completo con IA (vibe-coding), con correcciones manuales mientras aprendía C#. Es un proyecto de diversión/aprendizaje y el código puede ser desordenado en algunas partes.

## Qué Hace

- Lee logros locales desde archivos `.ini`/texto usados por configuraciones tipo emulador de Steam.
- Muestra logros dentro de Playnite.
- Lanza notificaciones de desbloqueo con sonido/tema personalizable.
- Incluye selección de idioma: inglés y español.
- Los readers actuales están probados principalmente con formatos tipo CODEX/RUNE.
- Está pensado para ir añadiendo más readers con el tiempo.

### Alcance

- Pensado para juegos con estructura de versión de Steam que usan `steam_api64.dll`.

## Capturas de Pantalla

| Lista de logros | Lista de logros 2 | Ajustes |
|---|---|---|
| ![Lista de logros](docs/images/achievements-list1.png) | ![Lista de logros 2](docs/images/achievements-list2.png) | ![Ajustes](docs/images/settings-screen.png) |

## Instalación (Recomendada): Desde el `.pext` de Releases

> [!TIP]
> Este es el método más fácil para usuarios sin conocimientos de programación.

1. Ve a **Releases** en este repositorio.
2. Descarga el último `Local-Achievements-<versión>.pext`.
3. Abre Playnite y arrastra/suelta el `.pext` sobre Playnite.
4. Reinicia Playnite si lo pide.
5. Abre `Ajustes -> Complementos -> Genérica -> Local Achievements`.
6. Configura tus rutas de logros locales y guarda.

## Instalación Manual (Alternativa)

Si instalas manualmente, copia los archivos de salida del plugin en `%APPDATA%\Playnite\Extensions\<PluginId>`.

## Para Desarrolladores

```powershell
dotnet restore
dotnet build -c Release
powershell -ExecutionPolicy Bypass -File scripts/PackPext.ps1 -Configuration Release
```

Paquete generado:
- `dist/Local-Achievements-1.0.1.pext` (nombre amigable)

## Transparencia

- Proyecto asistido por IA (vibe-coding intensivo).
- No soy desarrollador profesional de C#.
- Novato en GitHub; iré mejorando con el tiempo.
- Se agradecen forks y mejoras.

## Licencia

GNU GPL v3.0 (`LICENSE`).

## Cómo Contribuir

Lee `CONTRIBUTING.md`.

## Seguridad

Lee `SECURITY.md`.
