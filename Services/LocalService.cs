using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LocalAchievements
{
    // ---------------------------------------------------------
    // SERVICIO PRINCIPAL
    // ---------------------------------------------------------
    public class LocalService
    {
        private readonly MyPlugin plugin;
        private readonly IPlayniteAPI api;
        private readonly List<ILocalAchievementReader> readers;
        private readonly SteamSchemaFetcher schemaFetcher;

        public LocalService(MyPlugin plugin, IPlayniteAPI api)
        {
            this.plugin = plugin;
            this.api = api;

            // Inicializamos los lectores aquí mismo
            this.readers = new List<ILocalAchievementReader>
            {
                new CodexRuneReader()
            };

            this.schemaFetcher = new SteamSchemaFetcher(api, plugin.GetPluginUserDataPath());
        }

        public string FindAchievementsFile(Game game, string appId, List<string> searchPaths)
        {
            if (string.IsNullOrEmpty(appId)) return null;
            if (searchPaths == null) return null;

            foreach (var rawPath in searchPaths)
            {
                string path = ResolvePath(rawPath, game, appId);
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                                         .Where(f => f.EndsWith(".ini", StringComparison.OrdinalIgnoreCase) ||
                                                     f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));

                    foreach (var file in files)
                    {
                        if (file.IndexOf(appId, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            path.IndexOf(appId, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            foreach (var reader in readers)
                            {
                                if (reader.CanRead(file)) return file;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public List<LocalUnlockInfo> ReadSpecificFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return new List<LocalUnlockInfo>();

            foreach (var reader in readers)
            {
                if (reader.CanRead(path)) return reader.Read(path);
            }
            return new List<LocalUnlockInfo>();
        }

        public SchemaResult GetCachedSchema(string appId)
        {
            if (string.IsNullOrEmpty(appId)) return null;
            
            var cached = plugin.LoadPluginUserData<SchemaResult>($"Schema_{appId}");

            if (cached != null && cached.Schema != null && cached.Schema.Count > 0)
            {
                return cached;
            }

            var freshData = schemaFetcher.GetSchemaWithLog(appId);

            if (freshData != null && freshData.Schema.Count > 0)
            {
                plugin.SavePluginUserData($"Schema_{appId}", freshData);
            }

            return freshData;
        }

        public string ResolvePath(string rawPath, Game game = null, string appId = null)
        {
            if (string.IsNullOrEmpty(rawPath)) return "";

            string p = rawPath;
            string pubDocs = Environment.GetEnvironmentVariable("PUBLIC") ?? "C:\\Users\\Public";

            p = p.Replace("%PUBLIC%", pubDocs)
                 .Replace("%public%", pubDocs)
                 .Replace("%DOCUMENTS%", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
                 .Replace("%documents%", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
                 .Replace("%ProgramData%", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData))
                 .Replace("%programdata%", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

            p = Environment.ExpandEnvironmentVariables(p);

            if (!string.IsNullOrEmpty(appId))
            {
                p = p.Replace("{AppId}", appId);
            }
            if (game != null && !string.IsNullOrWhiteSpace(game.Name))
            {
                p = p.Replace("{GameName}", game.Name);
            }

            return p;
        }
    }

    // ---------------------------------------------------------
    // CLASES DE LECTURA (Integradas aquí para evitar errores de referencia)
    // ---------------------------------------------------------

    public interface ILocalAchievementReader
    {
        bool CanRead(string filePath);
        List<LocalUnlockInfo> Read(string filePath);
    }

    /*
    public class CodexRuneReader : ILocalAchievementReader
    {
        public bool CanRead(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext == ".ini" || ext == ".txt";
        }

        public List<LocalUnlockInfo> Read(string filePath)
        {
            var results = new List<LocalUnlockInfo>();
            if (!File.Exists(filePath)) return results;

            try
            {
                var lines = File.ReadAllLines(filePath);
                bool inSection = false;

                foreach (var line in lines)
                {
                    string clean = line.Trim();
                    if (string.IsNullOrEmpty(clean) || clean.StartsWith(";") || clean.StartsWith("#")) continue;

                    if (clean.StartsWith("[") && clean.EndsWith("]"))
                    {
                        string section = clean.Substring(1, clean.Length - 2).ToLowerInvariant();
                        inSection = (section == "steamachievements" || section == "achievements");
                        continue;
                    }

                    if (inSection)
                    {
                        var parts = clean.Split(new[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string val = parts[1].Trim();

                            bool isUnlocked = false;
                            DateTime unlockDate = DateTime.MinValue;

                            if (long.TryParse(val, out long numVal))
                            {
                                if (numVal == 1)
                                {
                                    isUnlocked = true;
                                    unlockDate = DateTime.MinValue;
                                }
                                else if (numVal > 1600000000)
                                {
                                    isUnlocked = true;
                                    unlockDate = DateTimeOffset.FromUnixTimeSeconds(numVal).LocalDateTime;
                                }
                            }

                            if (isUnlocked)
                            {
                                results.Add(new LocalUnlockInfo
                                {
                                    ApiName = key,
                                    IsUnlocked = true,
                                    UnlockDate = unlockDate
                                });
                            }
                        }
                    }
                }
            }
            catch { }

            return results;
        }
    }
    */
}
