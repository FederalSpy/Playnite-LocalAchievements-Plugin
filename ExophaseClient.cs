// Archivo: ExophaseClient.cs
// Propósito: Cliente HTTP/parsing para páginas de Exophase (scraping/parsing HTML).
// Revisado: 2026-02-04 — encabezado autoañadido.
// Nota: migrar a AngleSharp de forma consistente y centralizar parsers reutilizables.
using AngleSharp.Html.Parser;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LocalAchievements
{
    public class ExophaseClient
    {
        private readonly IPlayniteAPI api;

        public ExophaseClient(IPlayniteAPI api)
        {
            this.api = api;
        }

        public List<ExoAchievement> GetAchievements(string gameUrl, string language)
        {
            var results = new List<ExoAchievement>();

            string finalUrl = gameUrl;
            if (!finalUrl.EndsWith("/"))
            {
                finalUrl += "/";
            }

            if (language == "Español")
            {
                if (!finalUrl.ToLower().EndsWith("/es/"))
                {
                    finalUrl += "es/";
                }
            }

            var settings = new WebViewSettings
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
            };

            string html = Services.WebFetcher.GetPageSource(api, finalUrl, settings);

            var parser = new HtmlParser();
            var doc = parser.ParseDocument(html);

            var nodes = doc.QuerySelectorAll("ul.achievement li, ul.trophy li, ul.challenge li");

            foreach (var node in nodes)
            {
                try
                {
                    var ach = new ExoAchievement();

                    var nameNode = node.QuerySelector("a");
                    if (nameNode != null)
                        ach.Name = System.Net.WebUtility.HtmlDecode(nameNode.TextContent).Trim();

                    var descNode = node.QuerySelector("div.award-description p");
                    if (descNode != null)
                        ach.Description = System.Net.WebUtility.HtmlDecode(descNode.TextContent).Trim();

                    var imgNode = node.QuerySelector("img");
                    if (imgNode != null)
                        ach.ImageUrl = imgNode.GetAttribute("src");

                    // --- CORRECCIÓN 1: Parsear el float de Rarity ---
                    string rarityStr = node.GetAttribute("data-average");
                    if (float.TryParse(rarityStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float rarityVal))
                    {
                        ach.Rarity = rarityVal;
                    }

                    // --- CORRECCIÓN 2: IsSecret ya existe en el modelo ---
                    string classes = node.GetAttribute("class");
                    ach.IsSecret = classes != null && classes.Contains("secret");

                    if (!string.IsNullOrEmpty(ach.Name))
                        results.Add(ach);
                }
                catch
                {
                    // Ignorar errores individuales
                }
            }

            return results;
        }
    }
}
