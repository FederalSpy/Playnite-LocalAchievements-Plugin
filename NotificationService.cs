using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Threading;

namespace LocalAchievements
{
    public class NotificationService
    {
        private readonly MyPlugin plugin;
        private static readonly List<Window> openToasts = new List<Window>();
        private static MediaPlayer mediaPlayer;

        public NotificationService(MyPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void ShowNotification(string title, string description, string iconUrl)
        {
            if (Application.Current == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                int duration = plugin.settings?.Settings?.NotificationDuration ?? 4;
                if (duration <= 0)
                {
                    duration = 4;
                }

                PlayNotificationSound();

                var toast = CreateNotificationWindow(title, description, iconUrl, duration);
                if (toast == null)
                {
                    return;
                }

                toast.Closed += (s, e) =>
                {
                    if (openToasts.Contains(toast))
                    {
                        openToasts.Remove(toast);
                        RepositionToasts();
                    }
                };

                SetInitialToastPosition(toast);
                openToasts.Add(toast);
                toast.Show();
                RepositionToasts();
            });
        }

        private Window CreateNotificationWindow(string title, string description, string iconUrl, int duration)
        {
            var themed = CreateThemedWindow(title, description, iconUrl, duration);
            if (themed != null)
            {
                return themed;
            }

            return new AchievementToast(title, description, iconUrl, duration);
        }

        private Window CreateThemedWindow(string title, string description, string iconUrl, int duration)
        {
            string selectedTheme = plugin.settings?.Settings?.SelectedTheme;
            if (string.IsNullOrWhiteSpace(selectedTheme))
            {
                return null;
            }

            string themePath = plugin.GetThemeFilePath(selectedTheme);
            if (!File.Exists(themePath))
            {
                return null;
            }

            try
            {
                using (var stream = File.OpenRead(themePath))
                {
                    if (!(XamlReader.Load(stream) is Window window))
                    {
                        return null;
                    }

                    window.DataContext = new NotificationViewModel
                    {
                        Header = Localization.Get("LOCNotificationHeader"),
                        Title = title,
                        Description = description,
                        ImageUrl = iconUrl
                    };

                    window.MouseLeftButtonUp += (s, e) => FadeAndClose(window);

                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(duration) };
                    timer.Tick += (s, e) =>
                    {
                        timer.Stop();
                        FadeAndClose(window);
                    };
                    window.Closed += (s, e) => timer.Stop();
                    timer.Start();

                    return window;
                }
            }
            catch
            {
                return null;
            }
        }

        private void PlayNotificationSound()
        {
            string soundPath = plugin.settings?.Settings?.SoundPath;
            if (string.IsNullOrWhiteSpace(soundPath))
            {
                var installDefault = Path.Combine(plugin.GetExtensionInstallPath(), "default_unlock.mp3");
                var userDataDefault = Path.Combine(plugin.GetPluginUserDataPath(), "default_unlock.mp3");
                soundPath = File.Exists(installDefault) ? installDefault : userDataDefault;
            }

            if (!File.Exists(soundPath))
            {
                return;
            }

            try
            {
                mediaPlayer?.Stop();
                mediaPlayer?.Close();

                mediaPlayer = new MediaPlayer();
                mediaPlayer.Open(new Uri(soundPath, UriKind.Absolute));
                mediaPlayer.Play();
            }
            catch
            {
            }
        }

        private void FadeAndClose(Window window)
        {
            if (window == null || !window.IsVisible)
            {
                return;
            }

            var fade = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(400)
            };

            fade.Completed += (s, e) =>
            {
                try { window.Close(); } catch { }
            };

            window.BeginAnimation(Window.OpacityProperty, fade);
        }
        private void AnimateWindowTop(Window window, double newTop)
        {
            if (window == null)
            {
                return;
            }

            if (double.IsNaN(window.Top) || Math.Abs(window.Top - newTop) < 1)
            {
                window.Top = newTop;
                return;
            }

            var anim = new DoubleAnimation
            {
                To = newTop,
                Duration = TimeSpan.FromMilliseconds(240),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            window.BeginAnimation(Window.TopProperty, anim);
        }

        private void SetInitialToastPosition(Window window)
        {
            if (window == null)
            {
                return;
            }

            var desktop = SystemParameters.WorkArea;
            double rightMargin = 20;

            double width = window.Width;
            if (double.IsNaN(width) || width <= 0)
            {
                width = 350;
            }

            double height = window.Height;
            if (double.IsNaN(height) || height <= 0)
            {
                height = 90;
            }

            window.Left = desktop.Right - width - rightMargin;
            window.Top = desktop.Bottom + Math.Max(24, height * 0.35);
        }

        private void RepositionToasts()
        {
            var desktop = SystemParameters.WorkArea;
            double bottomMargin = 20;
            double rightMargin = 20;
            double gap = 10;
            double currentY = desktop.Bottom - bottomMargin;

            foreach (var window in openToasts.ToList())
            {
                if (window == null || !window.IsVisible)
                {
                    continue;
                }

                double width = window.ActualWidth > 0 ? window.ActualWidth : window.Width;
                double height = window.ActualHeight > 0 ? window.ActualHeight : window.Height;

                if (double.IsNaN(width) || width <= 0)
                {
                    width = 350;
                }
                if (double.IsNaN(height) || height <= 0)
                {
                    height = 80;
                }

                currentY -= height;
                window.Left = desktop.Right - width - rightMargin;

                if (window is AchievementToast achievementToast)
                {
                    achievementToast.SetTargetTop(currentY);
                }
                else
                {
                    AnimateWindowTop(window, currentY);
                }

                currentY -= gap;
            }
        }
    }
}

