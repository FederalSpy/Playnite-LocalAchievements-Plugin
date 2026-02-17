// Archivo: SteamService.cs
// Propósito: Servicio para interacción con Steam (descarga/esquema de logros, metadatos).
// Revisado: 2026-02-04 — encabezado autoañadido para facilitar mantenimiento.
// Nota: Añadir documentación XML a métodos públicos y extraer lógica reutilizable.
using Playnite.SDK;
using Playnite.SDK.Models;
using MyLocalAchievements.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;

namespace MyLocalAchievements
{
    public class SteamService
    {
        private readonly MyPlugin plugin;
        public Dictionary<Guid, string> ManualAppIds { get; set; } = new Dictionary<Guid, string>();

        public SteamService(MyPlugin plugin)
        {
            this.plugin = plugin;
            // Cargar datos al iniciar
            var saved = plugin.LoadPluginUserData<Dictionary<Guid, string>>("ManualAppIds");
            if (saved != null) ManualAppIds = saved;
        }

        public string GetGameAppId(Game game)
        {
            if (game == null) return "0";

            // 1. Manual (Prioridad máxima)
            if (ManualAppIds.ContainsKey(game.Id)) return ManualAppIds[game.Id];

            // 1b. Buscar steam_appid.txt en la carpeta de instalación del juego
            try
            {
                string foundId;
                if (TryFindAppIdInInstallFolder(game, out foundId) && !string.IsNullOrEmpty(foundId))
                {
                    ManualAppIds[game.Id] = foundId;
                    plugin.SavePluginUserData("ManualAppIds", ManualAppIds);
                    return foundId;
                }
            }
            catch { }

            // 2. Oficial (Si el juego viene de la librería de Steam)
            if (game.PluginId == Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab")) return game.GameId;

            // 3. Buscar en Steam Store (HTML search + validación por nombre)
            try
            {
                var sid = FindSteamAppIdFromStoreHtml(game.Name);
                if (!string.IsNullOrEmpty(sid))
                {
                    ManualAppIds[game.Id] = sid;
                    plugin.SavePluginUserData("ManualAppIds", ManualAppIds);
                    return sid;
                }
            }
            catch { }

            // 4. Buscar en SteamDB
            try
            {
                var sid2 = FindSteamAppIdFromSteamDb(game.Name);
                if (!string.IsNullOrEmpty(sid2))
                {
                    ManualAppIds[game.Id] = sid2;
                    plugin.SavePluginUserData("ManualAppIds", ManualAppIds);
                    return sid2;
                }
            }
            catch { }

            // 5. Prompt manual al usuario si no se ha encontrado
            try
            {
                SetManualAppId(game, plugin.PlayniteApi);
                if (ManualAppIds.ContainsKey(game.Id)) return ManualAppIds[game.Id];
            }
            catch { }

            return "0";
        }

        public void SetManualAppId(Game game, IPlayniteAPI api)
        {
            var res = api.Dialogs.SelectString(
                Localization.Get("InputAppID"),
                Localization.Get("ConfigTitle"),
                GetGameAppId(game));

            if (res.Result)
            {
                ManualAppIds[game.Id] = res.SelectedString;
                plugin.SavePluginUserData("ManualAppIds", ManualAppIds);
                api.Dialogs.ShowMessage(string.Format(Localization.Get("SteamIdSaved"), res.SelectedString));
            }
        }

        private string FetchFromWeb(string name)
        {
            try
            {
                string url = $"https://store.steampowered.com/api/storesearch/?term={Uri.EscapeDataString(name)}&l=english&cc=US";
                // Use centralized WebFetcher to retrieve page content via Playnite's offscreen WebView.
                string json = WebFetcher.GetPageSource(plugin.PlayniteApi, url);
                if (!string.IsNullOrEmpty(json))
                {
                    int idx = json.IndexOf("\"id\":");
                    if (idx != -1)
                    {
                        string sub = json.Substring(idx + 5);
                        int comma = sub.IndexOf(",");
                        if (comma != -1) return sub.Substring(0, comma).Trim();
                    }
                }
            }
            catch { }
            return "0";
        }

        // Exophase extraction removed: we no longer use Exophase links to derive appId.

        private bool TryFindAppIdInInstallFolder(Game game, out string appId)
        {
            appId = null;
            if (game == null) return false;

            try
            {
                // Intentar obtener la carpeta de instalación mediante reflexión (diversas propiedades posibles)
                var gtype = game.GetType();
                string[] candidates = new[] { "InstallDirectory", "InstallationDirectory", "InstallLocation", "InstallPath", "Path" };
                string installDir = null;
                foreach (var c in candidates)
                {
                    var prop = gtype.GetProperty(c);
                    if (prop != null)
                    {
                        var val = prop.GetValue(game) as string;
                        if (!string.IsNullOrEmpty(val)) { installDir = val; break; }
                    }
                }

                // Si no se encontró vía propiedades, intentar buscar en la librería de Playnite mediante InstalledGames (fallback no implementado aquí)
                if (string.IsNullOrEmpty(installDir)) return false;

                // Buscar steam_appid.txt en la carpeta y hasta 3 niveles arriba
                var dir = installDir;
                for (int i = 0; i < 4 && !string.IsNullOrEmpty(dir); i++)
                {
                    string candidate = Path.Combine(dir, "steam_appid.txt");
                    if (File.Exists(candidate))
                    {
                        var text = File.ReadAllText(candidate).Trim();
                        if (Regex.IsMatch(text, "^\\d+$"))
                        {
                            appId = text;
                            return true;
                        }
                    }
                    dir = Directory.GetParent(dir)?.FullName;
                }
            }
            catch { }
            return false;
        }

        private string NormalizeNameForCompare(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var ch in input.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(ch)) sb.Append(ch);
            }
            return sb.ToString();
        }

        private string FindSteamAppIdFromStoreHtml(string gameTitle)
        {
            try
            {
                string query = Uri.EscapeDataString(gameTitle);
                string url = $"https://store.steampowered.com/search/?term={query}";
                using (var wc = new WebClient())
                {
                    wc.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
                    string html = wc.DownloadString(url);
                    var m = Regex.Match(html, @"/app/(\d+)/");
                    if (m.Success)
                    {
                        string id = m.Groups[1].Value;
                        // Validar por similitud de nombre
                        try
                        {
                            string appUrl = $"https://store.steampowered.com/app/{id}/";
                            string appHtml = wc.DownloadString(appUrl);
                            var titleMatch = Regex.Match(appHtml, "<title>(.*?) - Steam</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            if (titleMatch.Success)
                            {
                                var official = titleMatch.Groups[1].Value;
                                var n1 = NormalizeNameForCompare(official);
                                var n2 = NormalizeNameForCompare(gameTitle);
                                if (!string.IsNullOrEmpty(n1) && !string.IsNullOrEmpty(n2) && (n1.Contains(n2) || n2.Contains(n1)))
                                {
                                    return id;
                                }
                            }
                            else
                            {
                                // If no title parsed, still return the id as a fallback
                                return id;
                            }
                        }
                        catch { return id; }
                    }
                }
            }
            catch { }
            return null;
        }

        private string FindSteamAppIdFromSteamDb(string gameTitle)
        {
            try
            {
                string query = Uri.EscapeDataString(gameTitle);
                string url = $"https://steamdb.info/search/?a=app&q={query}";
                using (var wc = new WebClient())
                {
                    wc.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
                    string html = wc.DownloadString(url);
                    var m = Regex.Match(html, @"/app/(\d+)/");
                    if (m.Success) return m.Groups[1].Value;
                }
            }
            catch { }
            return null;
        }
    }
}