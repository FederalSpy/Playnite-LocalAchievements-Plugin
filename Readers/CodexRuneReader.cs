using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LocalAchievements
{
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

                // Diccionarios temporales
                // Mapa: ApiName -> Datos (Desbloqueado/Fecha)
                var achievementsData = new Dictionary<string, LocalUnlockInfo>(StringComparer.OrdinalIgnoreCase);
                // Mapa: ApiName -> Índice (Orden)
                var indexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                string currentSection = "";

                // Variables temporales de sección
                bool currentAchieved = false;
                long currentUnlockTime = 0;

                foreach (var line in lines)
                {
                    string clean = line.Trim();
                    if (string.IsNullOrEmpty(clean) || clean.StartsWith(";") || clean.StartsWith("#")) continue;

                    // --- DETECCIÓN DE SECCIÓN ---
                    if (clean.StartsWith("[") && clean.EndsWith("]"))
                    {
                        // Guardar datos de la sección anterior si era un logro
                        if (!string.IsNullOrEmpty(currentSection))
                        {
                            StoreAchievementData(achievementsData, currentSection, currentAchieved, currentUnlockTime);
                        }

                        currentSection = clean.Substring(1, clean.Length - 2);
                        currentAchieved = false;
                        currentUnlockTime = 0;
                        continue;
                    }

                    // --- LECTURA DE CONTENIDO ---
                    if (!string.IsNullOrEmpty(currentSection))
                    {
                        var parts = clean.Split(new[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim().ToLowerInvariant();
                            string val = parts[1].Trim();

                            // CASO ESPECIAL: Sección [SteamAchievements] (El Índice)
                            if (currentSection.Equals("steamachievements", StringComparison.OrdinalIgnoreCase) ||
                                currentSection.Equals("achievements", StringComparison.OrdinalIgnoreCase))
                            {
                                // Formato: 00001=ApiName
                                if (int.TryParse(key, out int idx))
                                {
                                    indexMap[val] = idx;
                                }
                                // Aquí podríamos leer "Count", pero realmente el Count útil es el de la lista final.
                                continue;
                            }

                            // CASO NORMAL: Datos de un logro
                            if (key == "achieved" && (val == "1" || val.ToLower() == "true"))
                            {
                                currentAchieved = true;
                            }
                            else if (key == "unlocktime" || key == "time" || key == "timestamp")
                            {
                                long.TryParse(val, out currentUnlockTime);
                            }
                        }
                    }
                }

                // Guardar la última sección
                if (!string.IsNullOrEmpty(currentSection))
                {
                    StoreAchievementData(achievementsData, currentSection, currentAchieved, currentUnlockTime);
                }

                // --- FASE FINAL: COMBINAR DATOS + ÍNDICES ---
                foreach (var kvp in achievementsData)
                {
                    var info = kvp.Value;

                    // Si encontramos su índice en el mapa [SteamAchievements], lo asignamos
                    if (indexMap.TryGetValue(info.ApiName, out int foundIndex))
                    {
                        info.Index = foundIndex;
                    }

                    results.Add(info);
                }

                // Ordenamos por índice para que la lista local ya salga ordenada
                return results.OrderBy(x => x.Index).ToList();
            }
            catch { }

            return results;
        }

        private void StoreAchievementData(Dictionary<string, LocalUnlockInfo> dict, string sectionName, bool achieved, long unlockTime)
        {
            string s = sectionName.ToLowerInvariant();
            if (s == "steamachievements" || s == "achievements" || s == "stats" || s == "settings" || s == "language") return;

            bool isUnlocked = achieved || (unlockTime > 0);

            if (isUnlocked)
            {
                DateTime date = DateTime.MinValue;
                if (unlockTime > 1600000000)
                {
                    try { date = DateTimeOffset.FromUnixTimeSeconds(unlockTime).LocalDateTime; } catch { }
                }

                dict[sectionName] = new LocalUnlockInfo
                {
                    ApiName = sectionName,
                    IsUnlocked = true,
                    UnlockDate = date
                };
            }
        }
    }
}