using System.Collections.Generic;
using Playnite.SDK;

namespace LocalAchievements.Services
{
    public class SchemaCacheService
    {
        private readonly SteamSchemaFetcher fetcher;
        // Quitamos caché interna para forzar siempre lectura nueva durante debug
        // private readonly Dictionary<string, SteamSchemaFetcher.SchemaResult> cache...

        public SchemaCacheService(MyPlugin plugin, IPlayniteAPI api)
        {
            // CORRECCIÓN IMPORTANTE: Pasamos la ruta real, no null.
            // Esto permite que el Fetcher escriba los logs en la carpeta del plugin.
            fetcher = new SteamSchemaFetcher(api, plugin.GetPluginUserDataPath());
        }

        public SchemaResult GetSchemaWithLog(string appId)
        {
            if (string.IsNullOrEmpty(appId) || appId == "0") return null;

            // Llamada directa sin caché
            return fetcher.GetSchemaWithLog(appId);
        }
    }
}