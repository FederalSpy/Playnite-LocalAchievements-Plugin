// Archivo: Localization.cs
// Propósito: Proporciona cadenas localizadas y utilidad para carga de traducciones.
// Revisado: 2026-02-04 — encabezado autoañadido.
// Nota: agregar soporte para recursos y fallback en caso de claves faltantes.
using AngleSharp.Dom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Security.Policy;

namespace LocalAchievements
{
    public static class Localization
    {
        public static string CurrentLanguage = "English";

        public static string Get(string key)
        {
            // Detección flexible
            bool isSpanish = CurrentLanguage != null && (CurrentLanguage == "Español" || CurrentLanguage == "Spanish" || CurrentLanguage.StartsWith("es"));

            var dict = isSpanish ? Spanish : English;
            if (dict.ContainsKey(key)) return dict[key];

            // Si no encuentra la clave, devuelve la clave misma
            return key;
        }

        private static Dictionary<string, string> English = new Dictionary<string, string>
        {
            // --- MENU ---
            { "MenuSection", "Local Achievements" },
            { "MenuScan", "🕵️ Scan All (Local + Web)" },
            { "MenuLink", "🌐 Search / Link Exophase" },
            { "MenuSteam", "✏️ Configure Steam AppID" },
            { "MenuView", "🏆 View Achievements" },
            { "MenuClear", "🗑️ Clear Metadata (Force Redownload)" },
            { "WindowTitle", "Achievements List"},
            
            // --- REPORTES ---
            { "ReportTitle", "Report: {0}" },
            { "LocalFound", "✅ Local: Found in {0}" },
            { "LocalNotFound", "❌ Local: Not found on disk." },
            { "LocalNoAppID", "❌ Local: No AppID available." },

            { "WebConnected", "✅ Web: Connected. {0} achievements.\nEg: {1} ({2}%)" },
            { "WebNoAch", "⚠️ Web: Linked but no achievements detected." },
            { "WebNotLinked", "❌ Web: Not linked. Please link first." },
            { "WebError", "❌ Web Error: {0}" },

            { "SteamIdSaved", "ID Saved: {0}" },
            { "InputAppID", "Enter Steam AppID:" },
            { "ConfigTitle", "Configure ID" },

            // --- ESTADOS Y FILTROS ---
            { "CacheDeleted", "✅ Metadata deleted. Download again to refresh." },
            { "NoCache", "⚠️ No data downloaded yet. Run 'Scan' first." },
            { "SettingSpoiler", "🛡️ Spoiler: Blur secret achievements" },
            { "StatusUnlocked", "Unlocked" },
            { "StatusLocked", "Locked" },
            { "FilterSearch", "Search by name..." },
            { "FilterRarity", "Rarity" },
            { "FilterAll", "Unlocked" },
            { "FilterSecret", "Show Secrets" },
            { "BtnReset", "Reset" },
            { "ClickToReveal", "Click to View" },
            { "SortBy", "Sort by:" },
            { "SortName", "Name" },
            { "SortRarity", "Rarity (%)" },
            { "SortSecret", "Secret Status" },
            { "SortUnlocked", "Unlocked Status" },
            { "SortAsc", "Ascending (A-Z / 0-9)" },
            { "SortDesc", "Descending (Z-A / 9-0)" },

            // --- CONFIGURACIÓN DE SONIDO ---
            { "LOCSoundPathLabel", "Unlock Sound (MP3/WAV):" },
            { "LOCSoundPathNote", "Note: If empty, it looks for 'default_unlock.mp3' in the plugin folder." },
            { "LOCBrowse", "Browse..." },
            { "LOCTestButton", "▶ Test" },
            { "LOCSelectSoundTitle", "Select Achievement Sound" },
            { "LOCTestTitle", "Test Notification" },
            { "LOCTestDesc", "This is how your unlocked achievement will look!" },
            { "LOCNotificationHeader", "Achievement Unlocked" },

            // --- CONFIGURACIÓN DE RUTAS Y TEMAS ---
            { "LOCSettingsLanguageLabel", "Plugin Language / Idioma:" },
            { "LOCSettingsSpoiler", "Spoiler Protection (Blur secrets)" },
            { "LOCSettingsPaths", "Local Achievement File Paths" },
            { "LOCSettingsNote", "Enter one path per line. You can use %appdata%, %public%, etc." },
            { "LOCSettingsTheme", "Notification Visual Style:" },
            { "LOCSettingsThemeNote", "Note: Put your custom .xaml files in the 'Themes' folder inside the plugin data folder." },
            { "LOCSettingsDuration", "Notification Duration (seconds):" },

            { "ManualInputTitle", "Manual Metadata Import" },
            { "ManualInputInstructions", "1. The browser has opened SteamDB.\n" +
            "2. Press F12, go to the ‘Console’ tab, and paste the fetch code.\n" +
            "\t 2.1 If you get an error and cannot execute the code, you must type the command:\n" +
            "\t\tallow pasting\n" +
            "3. Click the ‘Copy’ button at the end of the browser text and paste it into the box below." },
            { "ManualInputButton", "Process Data" },
            { "ManualInputCopyButton", "Copy"},
            { "ManualInputCopySuccess", "Copied" },
            { "ManualInputCopyFailure", "Copying error" },
            { "ManualInputPasteHere", "Paste web metadata here:" },
            { "ManualInputInvalidContent", "The copied content is not in the expected format. Please verify you copied the full SteamDB response." }
        };

        private static Dictionary<string, string> Spanish = new Dictionary<string, string>
        {
            // --- MENU ---
            { "MenuSection", "Logros Locales" },
            { "MenuScan", "🕵️ Escanear Todo (Local + Web)" },
            { "MenuLink", "🌐 Buscar / Vincular Exophase" },
            { "MenuSteam", "✏️ Configurar Steam AppID" },
            { "MenuView", "🏆 Ver Lista de Logros" },
            { "MenuClear", "🗑️ Borrar Metadatos (Forzar Redescarga)" },
            { "WindowTitle", "Lista de Logros"},
            
            // --- REPORTES ---
            { "ReportTitle", "Reporte: {0}" },
            { "LocalFound", "✅ Local: Encontrado en {0}" },
            { "LocalNotFound", "❌ Local: No encontrado en disco." },
            { "LocalNoAppID", "❌ Local: No tenemos AppID para buscar." },

            { "WebConnected", "✅ Web: Conectado. {0} logros.\nEj: {1} ({2}%)" },
            { "WebNoAch", "⚠️ Web: Vinculado pero sin logros detectados." },
            { "WebNotLinked", "❌ Web: No vinculado. Vincula el juego primero." },
            { "WebError", "❌ Web Error: {0}" },

            { "SteamIdSaved", "ID Guardado: {0}" },
            { "InputAppID", "Introduce AppID:" },
            { "ConfigTitle", "Configurar ID" },

            // --- ESTADOS Y FILTROS ---
            { "CacheDeleted", "✅ Metadatos borrados. Escanea de nuevo para actualizar." },
            { "NoCache", "⚠️ No hay datos descargados. Escanea primero." },
            { "SettingSpoiler", "🛡️ Spoiler: Censurar/Difuminar logros secretos" },
            { "StatusUnlocked", "Desbloqueado" },
            { "StatusLocked", "Bloqueado" },
            { "FilterSearch", "Buscar por nombre..." },
            { "FilterRarity", "Rareza" },
            { "FilterAll", "Desbloqueados" },
            { "FilterSecret", "Ver Secretos" },
            { "BtnReset", "Limpiar" },
            { "ClickToReveal", "Clic para ver" },
            { "SortBy", "Ordenar por:" },
            { "SortName", "Nombre" },
            { "SortRarity", "Rareza (%)" },
            { "SortSecret", "Secreto" },
            { "SortUnlocked", "Desbloqueado" },
            { "SortAsc", "Ascendente (A-Z / 0-9)" },
            { "SortDesc", "Descendente (Z-A / 9-0)" },

            // --- CONFIGURACIÓN DE SONIDO ---
            { "LOCSoundPathLabel", "Sonido de Desbloqueo (MP3/WAV):" },
            { "LOCSoundPathNote", "Nota: Si se deja vacío, busca 'default_unlock.mp3' en la carpeta del plugin." },
            { "LOCBrowse", "Buscar..." },
            { "LOCTestButton", "▶ Probar" },
            { "LOCSelectSoundTitle", "Seleccionar sonido de logro" },
            { "LOCTestTitle", "Notificación de Prueba" },
            { "LOCTestDesc", "¡Así se verá tu logro desbloqueado!" },
            { "LOCNotificationHeader", "Logro desbloqueado" },

            // --- CONFIGURACIÓN DE RUTAS Y TEMAS ---
            { "LOCSettingsLanguageLabel", "Idioma del Plugin:" },
            { "LOCSettingsSpoiler", "Censura (Difuminar logros secretos)" },
            { "LOCSettingsPaths", "Rutas de Archivos Locales (Achievements)" },
            { "LOCSettingsNote", "Introduce una ruta por línea. Soporta %appdata%, %public%, etc." },
            { "LOCSettingsTheme", "Estilo de Notificación Visual:" },
            { "LOCSettingsThemeNote", "Nota: Pon tus archivos .xaml personalizados en la carpeta 'Themes' dentro de la carpeta de datos del plugin." },
            { "LOCSettingsDuration", "Duración de notificación (segundos):" },
            
            // --- VENTANA DE ENTRADA MANUAL ---
            { "ManualInputTitle", "Importación Manual de Metadatos" },
            { "ManualInputInstructions", "1. El navegador se ha abierto en SteamDB.\n" +
                "2. Presiona F12, ve a la pestaña 'Console' o 'Consola' y pega el código fetch.\n" +
                "\t 2.1 Si te da un error y no te deja ejecutar el código, debes escribir el comando:\n" +
                "\t\tallow pasting\n" +
                "3. Haz clic en el botón 'Copiar' o 'Copy' al final del texto del navegador y pégalo en el recuadro de abajo" },
            { "ManualInputButton", "Procesar Datos" },
            { "ManualInputCopyButton", "Copiar"},
            { "ManualInputCopySuccess", "Copiado al portapapeles" },
            { "ManualInputCopyFailure", "Error al copiar" },
            { "ManualInputPasteHere", "Copia los metadatos web aquí:" },
            { "ManualInputInvalidContent", "El contenido copiado no es el esperado. Verifica que copiaste la respuesta completa de SteamDB." }
        };
    }
}
