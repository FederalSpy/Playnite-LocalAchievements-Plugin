using System.Threading;
using Playnite.SDK;

namespace LocalAchievements.Services
{
    /// <summary>
    /// Helper to centralize offscreen webview usage and retries.
    /// This avoids duplicating navigation/retry logic across multiple services.
    /// </summary>
    public static class WebFetcher
    {
        /// <summary>
        /// Navigates to <paramref name="url"/> using an offscreen WebView and returns the page source.
        /// Retries a few times with a small delay to allow dynamically rendered content to appear.
        /// </summary>
        public static string GetPageSource(IPlayniteAPI api, string url, WebViewSettings settings = null, int attempts = 4, int delayMs = 1000)
        {
            try
            {
                using (var webView = api.WebViews.CreateOffscreenView(settings))
                {
                    webView.NavigateAndWait(url);
                    string html = "";
                    for (int i = 0; i < attempts; i++)
                    {
                        Thread.Sleep(delayMs);
                        html = webView.GetPageSource();
                        if (!string.IsNullOrEmpty(html)) break;
                    }
                    return html ?? string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
