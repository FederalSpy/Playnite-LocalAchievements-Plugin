using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using LocalAchievements.Services;

namespace LocalAchievements
{
    public class ExophaseService
    {
        private readonly MyPlugin plugin;
        private readonly IPlayniteAPI api;
        private readonly ExophaseClient client;
        private readonly LocalService localService;
        private readonly AppIdResolver appIdResolver;
        private static readonly ILogger logger = LogManager.GetLogger();

        public Dictionary<Guid, string> LinkedUrls { get; private set; } = new Dictionary<Guid, string>();
        private readonly Dictionary<Guid, List<ExoAchievement>> memoryCache = new Dictionary<Guid, List<ExoAchievement>>();

        public event Action<Guid, string, bool> AchievementUpdated;

        public ExophaseService(MyPlugin plugin, IPlayniteAPI api, LocalService local, AppIdResolver resolver)
        {
            this.plugin = plugin;
            this.api = api;
            this.client = new ExophaseClient(api);
            this.localService = local;
            this.appIdResolver = resolver;

            var saved = plugin.LoadPluginUserData<Dictionary<Guid, string>>("ExophaseLinks");
            if (saved != null) LinkedUrls = saved;
        }

        public string ScanExophase(Game game, bool forceManual, string overrideAppId = null)
        {
            string url = "";
            // Intentar resolver automáticamente al inicio
            string appId = overrideAppId ?? appIdResolver.GetOrResolveAppId(game);

            if (!forceManual && LinkedUrls.ContainsKey(game.Id))
            {
                url = LinkedUrls[game.Id];
            }

            // 1. ABRIR BUSCADOR PRIMERO (Si es manual o no hay link)
            if (forceManual || string.IsNullOrEmpty(url))
            {
                bool cancelled = false;
                string selectedUrl = "";

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var searchWindow = new ExophaseSearch(api, game.Name);
                    if (Application.Current.MainWindow != null)
                        searchWindow.Owner = Application.Current.MainWindow;

                    if (searchWindow.ShowDialog() == true)
                    {
                        selectedUrl = searchWindow.SelectedUrl;
                    }
                    else
                    {
                        cancelled = true;
                    }
                });

                if (cancelled || string.IsNullOrEmpty(selectedUrl)) return "Vinculación cancelada.";
                url = selectedUrl;
            }

            // 2. PEDIR APPID SI FALTA (Solo después de elegir el juego en Exophase)
            if (string.IsNullOrEmpty(appId) || appId == "0")
            {
                var res = api.Dialogs.SelectString("No se detectó el AppID de Steam. Por favor, ingrésalo manualmente:", "AppID Requerido", "");
                if (res.Result && !string.IsNullOrEmpty(res.SelectedString)) appId = res.SelectedString;
                else return "Escaneo abortado: Se requiere un AppID válido para mapear SteamDB.";
            }

            return ExecuteProcess(game, appId, url);
        }

        private string ExecuteProcess(Game game, string appId, string url)
        {
            // 1. DESCARGA EN INGLÉS (Fuente de Verdad para Lógica)
            // Necesario para que coincida con el Schema de SteamDB
            var webListEnglish = client.GetAchievements(url, "English");

            // 2. DESCARGA EN IDIOMA LOCAL (Fuente Visual)
            // Si Playnite está en Español, descargamos también la lista en Español
            List<ExoAchievement> displayList = webListEnglish; // Por defecto

            if (Localization.CurrentLanguage != "English")
            {
                // Pequeña pausa para no saturar al servidor
                System.Threading.Thread.Sleep(500);
                var webListLocal = client.GetAchievements(url, Localization.CurrentLanguage);

                // Solo usamos la lista local si tiene la misma cantidad de logros
                if (webListLocal.Count == webListEnglish.Count)
                {
                    displayList = webListLocal;
                }
            }

            if (webListEnglish.Count == 0) return "No se encontraron logros.";

            // Guardamos el enlace
            LinkedUrls[game.Id] = url;
            plugin.SavePluginUserData("ExophaseLinks", LinkedUrls);

            // Obtener Esquema (Doble Salto / Caché)
            var schema = localService.GetCachedSchema(appId);

            // 3. FUSIÓN LOCAL (Usando ambas listas)
            // Pasamos: (Lista Lógica, Lista Visual, Datos Locales, Diccionario)
            string localPath = localService.FindAchievementsFile(game, appId, plugin.settings.Settings.AchievementPaths.ToList());
            var localData = File.Exists(localPath) ? localService.ReadSpecificFile(localPath) : new List<LocalUnlockInfo>();

            // --- AQUÍ LLAMAMOS AL NUEVO MERGE ---
            AchievementMatcher.Merge(webListEnglish, displayList, localData, schema);

            // Guardamos la lista VISUAL (displayList) en la caché para que el usuario vea su idioma
            SaveCache(game.Id, displayList);

            int unlockedCount = displayList.Count(x => x.IsUnlocked);
            return $"Sincronizado: {unlockedCount}/{displayList.Count} logros.";
        }

        public List<ExoAchievement> LoadCache(Guid gameId)
        {
            if (memoryCache.ContainsKey(gameId)) return memoryCache[gameId];
            var data = plugin.LoadPluginUserData<List<ExoAchievement>>($"Cache_{gameId}");
            if (data != null) memoryCache[gameId] = data;
            return data;
        }

        public void SaveCache(Guid gameId, List<ExoAchievement> data)
        {
            memoryCache[gameId] = data;
            plugin.SavePluginUserData($"Cache_{gameId}", data);
        }

        public void NotifyUpdate(Guid gameId, string apiName, bool newState)
        {
            if (memoryCache.ContainsKey(gameId))
            {
                var list = memoryCache[gameId];
                var ach = list.FirstOrDefault(x => string.Equals(x.ApiName, apiName, StringComparison.OrdinalIgnoreCase));
                if (ach != null)
                {
                    ach.IsUnlocked = newState;
                    ach.UnlockedDate = newState ? (DateTime?)DateTime.Now : null;
                }
            }
            AchievementUpdated?.Invoke(gameId, apiName, newState);
        }

        public void ClearCache(Game game)
        {
            memoryCache.Remove(game.Id);
            LinkedUrls.Remove(game.Id);
            plugin.SavePluginUserData("ExophaseLinks", LinkedUrls);
            string path = Path.Combine(plugin.GetPluginUserDataPath(), $"Cache_{game.Id}.json");
            if (File.Exists(path)) File.Delete(path);
            api.Dialogs.ShowMessage("Vinculación y caché borradas.");
        }

        public void ShowAchievements(Game game)
        {
            var data = LoadCache(game.Id);
            if (data == null) { api.Dialogs.ShowMessage("Sin datos. Escanea el juego primero."); return; }

            var window = new AchievementsWindow(game.Name, data, plugin.settings.Settings.SpoilerSecrets, this, game.Id);
            if (Application.Current?.MainWindow != null) window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Show();
        }
    }
}