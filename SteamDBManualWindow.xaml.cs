using System;
using System.Collections.Generic;
using System.Windows;
using Playnite.SDK;

namespace MyLocalAchievements
{
    public partial class SteamDBManualWindow : Window
    {
        private readonly IPlayniteAPI api;
        private readonly string url;
        private IWebView webView;

        // Propiedad pública para que el Fetcher pueda leer los resultados (Arregla CS1061)
        public List<string> ExtractedIds { get; private set; } = new List<string>();

        // Constructor con 2 argumentos (Arregla CS1729)
        public SteamDBManualWindow(IPlayniteAPI api, string url)
        {
            InitializeComponent();
            this.api = api;
            this.url = url;

            this.Loaded += (s, e) => {
                webView = api.WebViews.CreateView(850, 550);
                webView.Navigate(url);
                BrowserContainer.Content = webView.Content; // Incrusta el navegador en el XAML
            };
        }

        // El método que el botón del XAML busca (Arregla CS1061 en el .xaml)
        private void BtnCapture_Click(object sender, RoutedEventArgs e)
        {
            if (webView == null) return;

            string js = @"
                (function() {
                    let items = document.querySelectorAll('.achievement_api, .apiname, .text-muted');
                    return Array.from(items).map(el => el.innerText.trim()).join('|');
                })()";

            var task = webView.EvaluateScriptAsync(js);
            task.Wait();

            if (task.Result?.Result != null)
            {
                string raw = task.Result.Result.ToString();
                var parts = raw.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts)
                {
                    string clean = p.Trim();
                    if (clean.Length > 2 && !ExtractedIds.Contains(clean)) ExtractedIds.Add(clean);
                }
            }

            this.DialogResult = true;
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            webView?.Dispose();
            base.OnClosed(e);
        }
    }
}