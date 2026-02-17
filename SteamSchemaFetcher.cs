using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Playnite.SDK;
using Playnite.SDK.Data;
using AngleSharp.Html.Parser; // Asegúrate de tener AngleSharp instalado

namespace LocalAchievements
{
    public class SteamSchemaFetcher
    {
        private readonly IPlayniteAPI api;
        private static readonly ILogger logger = LogManager.GetLogger();

        public SteamSchemaFetcher(IPlayniteAPI api, string userDataPath = null)
        {
            this.api = api;
        }

        public SchemaResult GetSchemaWithLog(string appId)
        {
            var result = new SchemaResult();
            if (string.IsNullOrEmpty(appId) || appId == "0") return result;

            string rawInput = string.Empty;

            // Ejecutamos en el hilo UI para poder abrir ventanas
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    // 1. Abrir navegador del usuario (Plan B infalible)
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"https://steamdb.info/app/{appId}/stats/",
                        UseShellExecute = true
                    });

                    // 2. Abrir nuestra ventana de pegado manual
                    var window = new ManualMetadataWindow(appId);

                    // Centrar sobre Playnite
                    if (Application.Current.MainWindow != null)
                        window.Owner = Application.Current.MainWindow;

                    // Mostrar ventana y esperar a que el usuario pegue y acepte
                    if (window.ShowDialog() == true)
                    {
                        rawInput = window.PastedText;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error en flujo manual de SteamDB");
                    api.Dialogs.ShowErrorMessage("Error: " + ex.Message);
                }
            });

            // 3. Procesar el texto pegado (JSON o HTML)
            if (!string.IsNullOrWhiteSpace(rawInput))
            {
                ParseInput(rawInput, result);

                if (result.Schema == null || result.Schema.Count == 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        api.Dialogs.ShowMessage(
                            Localization.Get("ManualInputInvalidContent"),
                            Localization.Get("MenuSection"));
                    });
                }
            }

            return result;
        }

        private void ParseInput(string input, SchemaResult result)
        {
            if (result == null || string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            if (result.Schema == null)
            {
                result.Schema = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            if (result.OrderedApiNames == null)
            {
                result.OrderedApiNames = new List<string>();
            }

            string htmlToParse = input;

            // Detectar si el usuario pegó el JSON del comando fetch
            if (input.Trim().StartsWith("{"))
            {
                try
                {
                    var json = Serialization.FromJson<Dictionary<string, object>>(input);
                    if (json != null && json.ContainsKey("html") && json["html"] != null)
                    {
                        htmlToParse = json["html"].ToString();
                    }
                }
                catch { /* Si falla el JSON, asumimos que es texto plano y seguimos */ }
            }

            // Usar AngleSharp para extraer los datos
            var parser = new HtmlParser();
            var document = parser.ParseDocument(htmlToParse);

            var achNodes = document.QuerySelectorAll(".achievement");
            foreach (var node in achNodes)
            {
                string name = node.QuerySelector(".achievement_name")?.TextContent.Trim();
                string apiName = node.QuerySelector(".achievement_api")?.TextContent.Trim();

                if (!string.IsNullOrEmpty(apiName))
                {
                    // Guardar nombre visible
                    if (!string.IsNullOrEmpty(name))
                        result.Schema[apiName] = name;

                    // Guardar ID técnico en orden
                    if (!result.OrderedApiNames.Contains(apiName))
                        result.OrderedApiNames.Add(apiName);
                }
            }
        }
    }
}
