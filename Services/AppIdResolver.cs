using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using LocalAchievements;

namespace LocalAchievements.Services
{
    public class AppIdResolver
    {
        private readonly MyPlugin plugin;

        public AppIdResolver(MyPlugin plugin)
        {
            this.plugin = plugin;
        }

        public string GetOrResolveAppId(Game game)
        {
            
            // 2. Steam Oficial
            if (game.PluginId == Guid.Parse("cb91dfc9-2977-416b-8e49-53d2961d1594"))
            {
                return game.GameId;
            }

            // 3. steam_appid.txt
            if (!string.IsNullOrEmpty(game.InstallDirectory) && Directory.Exists(game.InstallDirectory))
            {
                string txtPath = Path.Combine(game.InstallDirectory, "steam_appid.txt");
                if (File.Exists(txtPath))
                {
                    try
                    {
                        string id = File.ReadAllText(txtPath).Trim();
                        if (Regex.IsMatch(id, @"^\d+$")) return id;
                    }
                    catch { }
                }
            }

            // 4. Enlaces de Tienda
            if (game.Links != null)
            {
                foreach (var link in game.Links)
                {
                    if (string.IsNullOrEmpty(link.Url)) continue;
                    var match = Regex.Match(link.Url, @"store\.steampowered\.com\/app\/(\d+)");
                    if (match.Success) return match.Groups[1].Value;
                }
            }

            /* Verificación manual
            var manualIds = plugin.LoadPluginUserData<Dictionary<Guid, string>>("ManualAppIds");
            if (manualIds != null && manualIds.ContainsKey(game.Id))
            {
                return manualIds[game.Id];
            }
            */

            return "0";
        }
    }
}