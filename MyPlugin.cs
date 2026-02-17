using LocalAchievements.Services;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LocalAchievements
{
    public class MyPlugin : GenericPlugin
    {
        //private static readonly ILogger logger = LogManager.GetLogger();
        public override Guid Id { get; } = Guid.Parse("be2a47a2-04ab-4293-9b40-b39f7fd0835a");
        // Ensure transitive runtime assemblies are referenced so Costura embeds them in the final plugin DLL.
        private static readonly Type[] EmbeddedDependencyAnchors = new[]
        {
            typeof(System.Buffers.ArrayPool<byte>),
            typeof(System.Numerics.Vector<byte>),
            typeof(System.Runtime.CompilerServices.Unsafe),
            typeof(System.Text.CodePagesEncodingProvider)
        };

        public LocalAchievementsSettingsViewModel settings { get; private set; }

        private LocalService localService;
        private ExophaseService exoService;
        private FileWatcherService fileWatcher;
        private AppIdResolver appIdResolver;
        private NotificationService notifService;

        public MyPlugin(IPlayniteAPI api) : base(api)
        {
            _ = EmbeddedDependencyAnchors.Length;
            
            string pluginDataPath = GetPluginUserDataPath();
            Services.ExtendedLogger.Initialize(pluginDataPath);

            //api.Dialogs.ShowMessage("¡Plugin LocalAchievements ACTUALIZADO cargado correctamente!", "Verificación de Versión");
            
            ExtendedLogger.Initialize(pluginDataPath);
            ExtendedLogger.Info("=== MY LOCAL ACHIEVEMENTS PLUGIN REINICIADO ===");
            
            settings = new LocalAchievementsSettingsViewModel(this);

            if (string.IsNullOrEmpty(settings.Settings.AchievementsLanguage))
                Localization.CurrentLanguage = api.ApplicationSettings.Language;
            else
                Localization.CurrentLanguage = settings.Settings.AchievementsLanguage;

            Properties = new GenericPluginProperties { HasSettings = true };

            // 2. Inicializar Servicios (Orden estricto)
            localService = new LocalService(this, api);
            appIdResolver = new AppIdResolver(this);
            notifService = new NotificationService(this);

            // ExophaseService necesita resolver IDs y acceder a datos locales
            exoService = new ExophaseService(this, api, localService, appIdResolver);

            // FileWatcher necesita todo lo anterior
            fileWatcher = new FileWatcherService(this, localService, exoService, notifService, appIdResolver);
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Iniciar vigilancia global en hilo secundario para no congelar Playnite
            Task.Run(() =>
            {
                try
                {
                    fileWatcher.StartGlobalWatching();
                    ExtendedLogger.Info("Vigilancia Global iniciada correctamente.");
                }
                catch (Exception ex)
                {
                    ExtendedLogger.Error("Error fatal al iniciar vigilancia global", ex);
                }
            });
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            try { fileWatcher.StopAll(); } catch { }
        }

        public override ISettings GetSettings(bool firstRunSettings) => settings;
        public override UserControl GetSettingsView(bool firstRunSettings) => new LocalAchievementsSettingsView();

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            string submenu = Localization.Get("MenuSection");
            var game = args.Games.LastOrDefault();
            if (game == null) yield break;

            // Ver Logros
            yield return new GameMenuItem
            {
                MenuSection = submenu,
                Description = Localization.Get("MenuView"),
                Action = (c) => exoService.ShowAchievements(game)
            };

            yield return new GameMenuItem { Description = "-", MenuSection = submenu };

            // Escanear (Automático)
            yield return new GameMenuItem
            {
                MenuSection = submenu,
                Description = Localization.Get("MenuScan"),
                Action = (c) => RunOrchestrator(game, false)
            };

            // Vincular (Manual)
            yield return new GameMenuItem
            {
                MenuSection = submenu,
                Description = Localization.Get("MenuLink"),
                Action = (c) => RunOrchestrator(game, true)
            };

            yield return new GameMenuItem
            {
                MenuSection = submenu,
                Description = Localization.Get("MenuClear"),
                Action = (c) => exoService.ClearCache(game)
            };
        }

        private void RunOrchestrator(Game game, bool forceManual)
        {
            // 1. Resolver AppID
            string appId = appIdResolver.GetOrResolveAppId(game);

            // 2. Override Manual si el usuario eligió "Vincular"
            if (forceManual)
            {
                var input = PlayniteApi.Dialogs.SelectString(
                    "Ingrese el AppID de Steam o nombre de carpeta para buscar logros:",
                    "Configuración Manual",
                    appId);

                if (input.Result && !string.IsNullOrWhiteSpace(input.SelectedString))
                {
                    appId = input.SelectedString.Trim();
                    // Guardamos la preferencia manual
                    var dict = LoadPluginUserData<Dictionary<Guid, string>>("ManualAppIds") ?? new Dictionary<Guid, string>();
                    dict[game.Id] = appId;
                    SavePluginUserData("ManualAppIds", dict);

                    ExtendedLogger.Info($"Usuario definió manualmente AppID para {game.Name}: {appId}");
                }
                else return; // Cancelado
            }

            // 3. Buscar archivo local
            string localPath = localService.FindAchievementsFile(game, appId, settings.Settings.AchievementPaths.ToList());
            string resLocal = !string.IsNullOrEmpty(localPath)
                ? string.Format(Localization.Get("LocalFound"), localPath)
                : Localization.Get("LocalNotFound");

            // 4. Escaneo Web y Fusión
            string resWeb = exoService.ScanExophase(game, forceManual, appId); // Pasamos appId explícito

            PlayniteApi.Dialogs.ShowMessage($"{resLocal}\n(AppID: {appId})\n\n{resWeb}", game.Name);
        }

        public void SavePluginUserData<T>(string filename, T data) where T : class
        {
            try
            {
                string path = Path.Combine(GetPluginUserDataPath(), filename + ".json");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, Serialization.ToJson(data));
            }
            catch (Exception ex) { ExtendedLogger.Error($"Error guardando {filename}", ex); }
        }

        public T LoadPluginUserData<T>(string filename) where T : class
        {
            try
            {
                string path = Path.Combine(GetPluginUserDataPath(), filename + ".json");
                if (File.Exists(path)) return Serialization.FromJson<T>(File.ReadAllText(path));
            }
            catch { }
            return null;
        }

        public string GetExtensionInstallPath()
        {
            try
            {
                var asmPath = Assembly.GetExecutingAssembly().Location;
                var asmDir = Path.GetDirectoryName(asmPath);
                if (!string.IsNullOrWhiteSpace(asmDir) && Directory.Exists(asmDir))
                {
                    return asmDir;
                }
            }
            catch
            {
            }

            return GetPluginUserDataPath();
        }

        public List<string> GetThemeDirectories()
        {
            var dirs = new List<string>();

            var installThemes = Path.Combine(GetExtensionInstallPath(), "Themes");
            if (Directory.Exists(installThemes))
            {
                dirs.Add(installThemes);
            }

            var userDataThemes = Path.Combine(GetPluginUserDataPath(), "Themes");
            if (Directory.Exists(userDataThemes))
            {
                dirs.Add(userDataThemes);
            }

            return dirs
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public string GetThemeFilePath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            foreach (var dir in GetThemeDirectories())
            {
                var fullPath = Path.Combine(dir, fileName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return null;
        }

        public void RestartGlobalWatching()
        {
            try
            {
                fileWatcher?.StartGlobalWatching();
            }
            catch (Exception ex)
            {
                ExtendedLogger.Error("Error reiniciando vigilancia global", ex);
            }
        }
    }
}
