using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LocalAchievements.Services;

namespace LocalAchievements
{
    public class FileWatcherService : IDisposable
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private readonly MyPlugin plugin;
        private readonly LocalService localService;
        private readonly ExophaseService exoService;
        private readonly NotificationService notifService;
        private readonly AppIdResolver appIdResolver;

        private List<FileSystemWatcher> globalWatchers = new List<FileSystemWatcher>();
        private ConcurrentDictionary<string, DateTime> fileDebounce = new ConcurrentDictionary<string, DateTime>();

        public FileWatcherService(MyPlugin plugin, LocalService local, ExophaseService exo, NotificationService notif, AppIdResolver resolver)
        {
            this.plugin = plugin;
            this.localService = local;
            this.exoService = exo;
            this.notifService = notif;
            this.appIdResolver = resolver;
        }

        public void StartGlobalWatching()
        {
            StopAll();
            var paths = plugin.settings?.Settings?.AchievementPaths;
            if (paths == null) return;

            var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var raw in paths)
            {
                string resolved = localService.ResolvePath(raw);

                string root = resolved;
                while (!string.IsNullOrEmpty(root) && !Directory.Exists(root))
                {
                    root = Path.GetDirectoryName(root);
                }

                if (!string.IsNullOrEmpty(root) && Directory.Exists(root)) roots.Add(root);
            }

            foreach (var root in roots)
            {
                try
                {
                    var w = new FileSystemWatcher(root)
                    {
                        IncludeSubdirectories = true,
                        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.CreationTime,
                        Filter = "*.*",
                        EnableRaisingEvents = true
                    };
                    w.Changed += OnFileChanged;
                    w.Created += OnFileChanged;
                    globalWatchers.Add(w);
                }
                catch (Exception ex) { logger.Error(ex, $"Fallo al vigilar: {root}"); }
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Solo nos interesan archivos relevantes
            if (!e.Name.EndsWith(".ini", StringComparison.OrdinalIgnoreCase) &&
                !e.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) return;

            // Debounce: Evita procesar el mismo archivo múltiples veces en menos de 1 segundo
            if (fileDebounce.TryGetValue(e.FullPath, out var lastTime) && (DateTime.Now - lastTime).TotalSeconds < 1)
                return;

            fileDebounce[e.FullPath] = DateTime.Now;
            Task.Run(() => ProcessFileWithRetry(e.FullPath));
        }

        private async Task ProcessFileWithRetry(string path)
        {
            // SISTEMA DE REINTENTOS: Intenta leer 3 veces si el archivo está bloqueado
            int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // Pequeña espera para dar tiempo al juego a terminar de escribir
                    await Task.Delay(200 * (i + 1));

                    ProcessFile(path);
                    return; // Éxito, salimos
                }
                catch (IOException)
                {
                    // Archivo bloqueado, reintentamos
                    if (i == maxRetries - 1) logger.Warn($"No se pudo leer {path} tras {maxRetries} intentos.");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error fatal procesando {path}");
                    return;
                }
            }
        }

        private void ProcessFile(string path)
        {
            // Extraer AppID
            var match = System.Text.RegularExpressions.Regex.Match(path, @"\d+");
            if (!match.Success) return;
            string appId = match.Value;

            var game = plugin.PlayniteApi.Database.Games.FirstOrDefault(x =>
                x.IsInstalled && appIdResolver.GetOrResolveAppId(x) == appId);

            if (game == null) return;

            // Leer datos (esto puede fallar si el archivo está bloqueado, por eso el Retry arriba)
            var localData = localService.ReadSpecificFile(path);

            // Si leemos una lista vacía pero el archivo existe, puede ser un error de lectura parcial.
            // Abortamos para no borrar los logros del usuario por error.
            if (localData.Count == 0 && new FileInfo(path).Length > 100) return;

            var exoCache = exoService.LoadCache(game.Id);
            if (exoCache == null || exoCache.Count == 0) return;

            var schema = localService.GetCachedSchema(appId);

            // FOTO DEL ANTES
            var previousState = exoCache.ToDictionary(
                x => x.ApiName ?? x.Name,
                x => x.IsUnlocked
            );

            // ACTUALIZAR ESTADO
            AchievementMatcher.Merge(exoCache, exoCache, localData, schema);

            bool anyChange = false;

            foreach (var ach in exoCache)
            {
                string key = ach.ApiName ?? ach.Name;

                if (previousState.TryGetValue(key, out bool wasUnlocked))
                {
                    // DETECTAR NUEVO DESBLOQUEO
                    if (ach.IsUnlocked && !wasUnlocked)
                    {
                        notifService.ShowNotification(ach.Name, ach.Description, ach.ImageUrl);

                        // IMPORTANTE: Esto dispara el evento para la Ventana
                        exoService.NotifyUpdate(game.Id, key, true);
                        anyChange = true;
                    }
                    else if (ach.IsUnlocked != wasUnlocked)
                    {
                        // Cambio de estado silencioso (ej: rebloqueo)
                        exoService.NotifyUpdate(game.Id, key, ach.IsUnlocked);
                        anyChange = true;
                    }
                }
            }

            if (anyChange)
            {
                exoService.SaveCache(game.Id, exoCache);
            }
        }

        public void StopAll()
        {
            foreach (var w in globalWatchers)
            {
                w.EnableRaisingEvents = false;
                w.Dispose();
            }
            globalWatchers.Clear();
        }

        public void Dispose() => StopAll();
    }
}
