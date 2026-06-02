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
                new CodexRuneReader(),
                new GoldbergReader()
            };

            this.schemaFetcher = new SteamSchemaFetcher(api, plugin.GetPluginUserDataPath());
        }

        public LocalFileResult FindAchievementsFile(Game game, string appId, List<string> searchPaths)
        {
            if (string.IsNullOrEmpty(appId)) return LocalFileResult.NotFound();
            if (searchPaths == null) return LocalFileResult.NotFound();

            var candidates = new List<Candidate>();

            foreach (var rawPath in searchPaths)
            {
                string dir = ResolvePath(rawPath, game, appId);
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) continue;

                string[] files;
                try { files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories); }
                catch { continue; }

                foreach (var file in files)
                {
                    long size;
                    try { size = new FileInfo(file).Length; } catch { continue; }
                    if (size == 0) continue;

                    string ext = Path.GetExtension(file).ToLowerInvariant();
                    if (ext != ".json" && ext != ".ini" && ext != ".txt") continue;

                    if (!IsAssociatedWithAppId(file, dir, appId)) continue;

                    candidates.Add(new Candidate
                    {
                        File = file,
                        Score = ScoreCandidate(file, ext, dir, appId)
                    });
                }
            }

            if (candidates.Count == 0) return LocalFileResult.NotFound();

            var ordered = candidates.OrderByDescending(c => c.Score).ToList();

            foreach (var c in ordered)
            {
                string ext = Path.GetExtension(c.File).ToLowerInvariant();
                if (LooksLikeAchievementData(c.File, ext))
                {
                    return LocalFileResult.Found(c.File);
                }
            }

            return LocalFileResult.Empty(ordered[0].File);
        }

        private class Candidate
        {
            public string File;
            public int Score;
        }

        private bool IsAssociatedWithAppId(string filePath, string dir, string appId)
        {
            string name = Path.GetFileName(filePath) ?? "";
            return name.IndexOf(appId, StringComparison.OrdinalIgnoreCase) >= 0
                || (dir ?? "").IndexOf(appId, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private int ScoreCandidate(string filePath, string ext, string dir, string appId)
        {
            int score = 0;
            string name = (Path.GetFileNameWithoutExtension(filePath) ?? "").ToLowerInvariant();
            string dirName = (Path.GetFileName(dir) ?? "").ToLowerInvariant();

            if (name.Contains("achievement") || name.Contains("logro") ||
                name.Contains("unlock") || name.Contains("stats"))
                score += 10;
            if (name.IndexOf(appId, StringComparison.OrdinalIgnoreCase) >= 0) score += 5;
            if (ext == ".json") score += 3;
            else if (ext == ".ini") score += 2;
            else score += 1;
            if (dirName.IndexOf(appId, StringComparison.OrdinalIgnoreCase) >= 0) score += 2;
            return score;
        }

        private bool LooksLikeAchievementData(string filePath, string ext)
        {
            try
            {
                string head;
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    var buf = new char[8192];
                    int read = sr.Read(buf, 0, buf.Length);
                    head = new string(buf, 0, read);
                }
                if (string.IsNullOrWhiteSpace(head)) return false;

                if (ext == ".json")
                {
                    string t = head.TrimStart();
                    if (t.Length == 0 || (t[0] != '{' && t[0] != '[')) return false;
                    string lower = head.ToLowerInvariant();
                    return lower.Contains("achievement") || lower.Contains("unlocked")
                        || lower.Contains("earned") || lower.Contains("apiname")
                        || lower.Contains("\"name\"") || lower.Contains("\"path\"");
                }
                else
                {
                    string lower = head.ToLowerInvariant();
                    return lower.Contains("[steamachievements]") || lower.Contains("[achievements]")
                        || lower.Contains("achieved=") || lower.Contains("unlocktime=")
                        || lower.Contains("earned_time=") || lower.Contains("apiname=");
                }
            }
            catch { return false; }
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
