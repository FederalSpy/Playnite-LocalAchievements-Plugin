// Archivo: ExophaseSearch.xaml.cs
// Propósito: Code-behind para la vista de búsqueda de Exophase (UI logic mínima).
// Revisado: 2026-02-04 — encabezado autoañadido.
// Nota: mantener lógica UI en code-behind al mínimo; usar ViewModel si crece.
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Playnite.SDK;
using Playnite.SDK.Data;
using System.Linq;
using System.Text.RegularExpressions; // Necesario para limpiar el HTML

namespace LocalAchievements
{
    public partial class ExophaseSearch : Window
    {
        public string SelectedUrl { get; private set; } = string.Empty;
        private readonly IPlayniteAPI api;
        private const string UrlApiSearch = @"https://api.exophase.com/public/archive/games?q={0}&sort=added";

        public ExophaseSearch(IPlayniteAPI api, string defaultSearch)
        {
            InitializeComponent();
            this.api = api;
            SearchBox.Text = defaultSearch;
            // Ejecutar búsqueda automática al abrir si hay texto
            if (!string.IsNullOrWhiteSpace(defaultSearch)) this.Loaded += (s, e) => RunSearch(defaultSearch);
        }

        private void Search_Click(object sender, RoutedEventArgs e) => RunSearch(SearchBox.Text);

        private void RunSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return;
            StatusText.Text = "Consultando API...";
            ResultsList.ItemsSource = null;
            BtnSelect.IsEnabled = false;
            SearchBox.IsEnabled = false;

            try
            {
                using (var webView = api.WebViews.CreateOffscreenView())
                {
                    string url = string.Format(UrlApiSearch, Uri.EscapeDataString(query));
                    webView.NavigateAndWait(url);

                    // Usamos GetPageSource para ser más robustos
                    string json = webView.GetPageSource();

                    // LIMPIEZA: A veces viene envuelto en <pre>...</pre>
                    if (json.Contains("<pre"))
                    {
                        json = Regex.Replace(json, "<.*?>", "");
                        json = System.Net.WebUtility.HtmlDecode(json);
                    }

                    // --- CORRECCIÓN DEL ERROR ---
                    // Si Exophase no encuentra nada, devuelve "games": false
                    // Detectamos esto manualmente para evitar el crash.
                    if (json.Contains("\"games\":false") || json.Contains("\"games\": false"))
                    {
                        StatusText.Text = "Sin resultados.";
                        ResultsList.ItemsSource = new List<SearchResult>(); // Lista vacía
                    }
                    else
                    {
                        var respuesta = Serialization.FromJson<ExoApiResponse>(json);
                        var results = new List<SearchResult>();

                        if (respuesta?.Games?.List != null)
                        {
                            foreach (var item in respuesta.Games.List)
                            {
                                string img = item.Images?.O ?? item.Images?.L ?? item.Images?.M ?? item.Images?.S;
                                if (string.IsNullOrEmpty(img)) img = "https://www.exophase.com/assets/images/exophase-icon.png";

                                results.Add(new SearchResult
                                {
                                    Title = item.Title,
                                    Url = item.EndpointAwards,
                                    ImageUrl = img,
                                    Platform = item.Platforms?.FirstOrDefault()?.Name ?? "Desconocido"
                                });
                            }
                        }

                        if (results.Count == 0) StatusText.Text = "Sin resultados.";
                        else StatusText.Text = $"Encontrados {results.Count} juegos.";

                        ResultsList.ItemsSource = results;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                StatusText.Text = "Error.";
            }
            finally
            {
                SearchBox.IsEnabled = true;
            }
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (ResultsList.SelectedItem is SearchResult selected)
            {
                SelectedUrl = selected.Url;
                DialogResult = true;
                Close();
            }
        }

        private void ResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnSelect.IsEnabled = ResultsList.SelectedItem != null;
        }
    }

    public class SearchResult
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string Platform { get; set; }
    }

    public class ExoApiResponse
    {
        public bool Success { get; set; }
        public GamesResponse Games { get; set; }
    }

    public class GamesResponse
    {
        public List<ExoGame> List { get; set; }
    }

    public class ExoGame
    {
        public string Title { get; set; }
        [Playnite.SDK.Data.SerializationPropertyName("endpoint_awards")]
        public string EndpointAwards { get; set; }
        public ExoImages Images { get; set; }
        public List<ExoPlatform> Platforms { get; set; }
    }

    public class ExoImages
    {
        public string O { get; set; } // Original
        public string L { get; set; } // Large
        public string M { get; set; } // Medium
        public string S { get; set; } // Small
    }

    public class ExoPlatform
    {
        public string Name { get; set; }
    }
}