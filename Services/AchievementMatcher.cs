using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LocalAchievements.Services
{
    public static class AchievementMatcher
    {
        public static void Merge(
            List<ExoAchievement> englishList,  // Para lógica (coincide con SteamDB)
            List<ExoAchievement> displayList,  // Para visualización (idioma local)
            List<LocalUnlockInfo> localData,
            SchemaResult schemaResult)
        {
            // 1. Preparar Mapa Local (ApiName -> Datos)
            var localMap = new Dictionary<string, LocalUnlockInfo>(StringComparer.OrdinalIgnoreCase);
            if (localData != null)
            {
                foreach (var l in localData)
                {
                    if (!string.IsNullOrEmpty(l.ApiName))
                        localMap[l.ApiName.Trim()] = l;
                }
            }

            // 2. Preparar Mapa de Traducción (Inglés -> ApiName)
            var englishToApiMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            if (schemaResult?.Schema != null)
            {
                foreach (var kvp in schemaResult.Schema)
                {
                    // kvp.Key = ACHIEVEMENT_1, kvp.Value = "The Journey Begins"
                    string norm = Normalize(kvp.Value);
                    if (!string.IsNullOrEmpty(norm))
                        englishToApiMap[norm] = kvp.Key;
                }
            }

            // Mapa de Orden (ApiName -> Índice)
            var schemaOrderMap = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            if (schemaResult?.OrderedApiNames != null)
            {
                for (int i = 0; i < schemaResult.OrderedApiNames.Count; i++)
                    schemaOrderMap[schemaResult.OrderedApiNames[i]] = i;
            }

            // 3. Iteramos las listas en paralelo
            // Asumimos que Exophase devuelve los logros en el mismo orden para todos los idiomas
            for (int i = 0; i < englishList.Count; i++)
            {
                var engAch = englishList[i];

                // Obtenemos el logro correspondiente en el idioma visual
                // Si por error las listas tienen distinto tamaño, fallback al inglés
                var displayAch = (i < displayList.Count) ? displayList[i] : engAch;

                bool isUnlocked = false;
                DateTime unlockDate = default;
                int sortIndex = int.MaxValue;

                // --- LÓGICA DE MATCHING (USANDO INGLÉS) ---

                string webNameNorm = Normalize(engAch.Name); // "thejourneybegins"
                string realApiKey = webNameNorm; // Por defecto

                // A) Intentamos traducir Nombre Inglés -> ApiName (ACHIEVEMENT_1)
                if (englishToApiMap.TryGetValue(webNameNorm, out string apiName))
                {
                    realApiKey = apiName;
                }
                // B) Si Exophase ya traía ApiName
                else if (!string.IsNullOrEmpty(engAch.ApiName))
                {
                    realApiKey = engAch.ApiName;
                }

                // --- BÚSQUEDA EN LOCAL ---
                // Buscamos usando el ApiName encontrado (o el nombre en inglés como fallback)
                if (localMap.TryGetValue(realApiKey, out var match))
                {
                    isUnlocked = match.IsUnlocked;
                    unlockDate = match.UnlockDate;
                    if (match.Index != int.MaxValue) sortIndex = match.Index;

                    // Asignamos el ApiName real al objeto visual para referencias futuras
                    displayAch.ApiName = match.ApiName;
                }
                else if (localMap.TryGetValue(webNameNorm, out var directMatch))
                {
                    // Fallback: Coincidencia directa con nombre en inglés
                    isUnlocked = directMatch.IsUnlocked;
                    unlockDate = directMatch.UnlockDate;
                    if (directMatch.Index != int.MaxValue) sortIndex = directMatch.Index;
                }

                // --- ORDENAMIENTO DE RESPALDO ---
                if (sortIndex == int.MaxValue && schemaOrderMap.TryGetValue(realApiKey, out int schemaIdx))
                {
                    sortIndex = schemaIdx;
                }

                // --- ACTUALIZAR EL OBJETO VISUAL (ESPAÑOL) ---
                displayAch.IsUnlocked = isUnlocked;
                displayAch.UnlockedDate = isUnlocked ? (unlockDate != DateTime.MinValue ? unlockDate : DateTime.Now) : (DateTime?)null;
                displayAch.Index = sortIndex;

                // Si encontramos el ApiName real, aseguremonos de guardarlo en el objeto visual
                if (realApiKey != webNameNorm) displayAch.ApiName = realApiKey;
            }
        }

        private static string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return Regex.Replace(input, "[^a-zA-Z0-9]", "").ToLowerInvariant();
        }
    }
}