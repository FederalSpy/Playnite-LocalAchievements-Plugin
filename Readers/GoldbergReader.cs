// Archivo: Readers/GoldbergReader.cs
// Propósito: Lector de logros para formato Goldberg (parsing y extracción).
// Revisado: 2026-02-04 — encabezado autoañadido.
// Nota: considerar compartir utilidades de parsing con otros lectores.
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace LocalAchievements
{
    public class GoldbergReader : ILocalAchievementReader
    {
        public string Name => "Goldberg/JSON Reader";

        public bool CanRead(string filePath)
        {
            return filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
        }

        public List<LocalUnlockInfo> Read(string filePath)
        {
            var results = new List<LocalUnlockInfo>();
            try
            {
                string jsonText = File.ReadAllText(filePath);
                dynamic data = Serialization.FromJson<dynamic>(jsonText);

                foreach (dynamic item in data)
                {
                    string name = null;
                    long earnedTime = 0;

                    // Goldberg a veces usa una estructura donde la clave es el nombre, o una propiedad "Name"
                    try { name = item.Name; } catch { }
                    if (string.IsNullOrEmpty(name)) try { name = item.Path; } catch { } // Empress usa 'Path'

                    // Si es un array de objetos sin nombre explícito, el item en sí podría ser la clave si iteramos un diccionario
                    // Pero Serialization.FromJson<dynamic> suele devolver una lista o un objeto wrapper.
                    // Para simplificar, usamos lógica defensiva similar a SuccessStory:
                    if (string.IsNullOrEmpty(name)) name = item.ToString();

                    try
                    {
                        // Buscar earned_time
                        if (item.earned_time != null) earnedTime = (long)item.earned_time;
                        else if (item.First != null && item.First.earned_time != null) earnedTime = (long)item.First.earned_time;
                    }
                    catch { }

                    if (!string.IsNullOrEmpty(name) && earnedTime > 0)
                    {
                        results.Add(new LocalUnlockInfo
                        {
                            ApiName = name,
                            UnlockDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(earnedTime).ToLocalTime(),
                            IsUnlocked = true
                        });
                    }
                }
            }
            catch { }
            return results;
        }
    }
}