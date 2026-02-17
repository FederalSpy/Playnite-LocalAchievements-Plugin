// Archivo: LocalAchievementsSettings.cs
// Propósito: Contiene la configuración serializable del plugin y colecciones de ajustes.
// Revisado: 2026-02-04 — encabezado autoañadido.
// Nota: revisar serialización y valores por defecto; documentar propiedades públicas.
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace LocalAchievements
{
    // --- 1. CLASE DE DATOS ---
    public class LocalAchievementsSettings : ObservableObject
    {
        private string achievementsLanguage = "Español";
        public string AchievementsLanguage { get => achievementsLanguage; set => SetValue(ref achievementsLanguage, value); }

        private bool spoilerSecrets = true;
        public bool SpoilerSecrets { get => spoilerSecrets; set => SetValue(ref spoilerSecrets, value); }

        private int notificationDuration = 4;
        public int NotificationDuration
        {
            get => notificationDuration;
            set
            {
                int clamped = value;
                if (clamped < 1) clamped = 1;
                if (clamped > 30) clamped = 30;
                SetValue(ref notificationDuration, clamped);
            }
        }

        // Propiedad para seleccionar el tema visual
        private string selectedTheme = "Default.xaml";
        public string SelectedTheme { get => selectedTheme; set => SetValue(ref selectedTheme, value); }

        private string soundPath = "";
        public string SoundPath { get => soundPath; set => SetValue(ref soundPath, value); }

        private ObservableCollection<string> achievementPaths = new ObservableCollection<string>();
        public ObservableCollection<string> AchievementPaths { get => achievementPaths; set => SetValue(ref achievementPaths, value); }

        [DontSerialize]
        public string AchievementPathsString
        {
            get => string.Join("\n", AchievementPaths);
            set
            {
                AchievementPaths.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    foreach (var line in value.Split('\n'))
                    {
                        if (!string.IsNullOrWhiteSpace(line)) AchievementPaths.Add(line.Trim());
                    }
                }
                OnPropertyChanged();
            }
        }
    }

    // --- 2. CLASE VIEWMODEL ---
    public class LocalAchievementsSettingsViewModel : ObservableObject, ISettings
    {
        private readonly MyPlugin plugin;

        public MyPlugin Plugin => plugin;
        public ObservableCollection<string> AvailableThemes { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> AvailableLanguages { get; } = new ObservableCollection<string> { "English", "Español" };

        private LocalAchievementsSettings editingClone { get; set; }

        private LocalAchievementsSettings settings;
        public LocalAchievementsSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public LocalAchievementsSettingsViewModel(MyPlugin plugin)
        {
            this.plugin = plugin;

            var savedSettings = plugin.LoadPluginSettings<LocalAchievementsSettings>();

            if (savedSettings != null)
            {
                Settings = savedSettings;
                EnsureSettingsDefaults();
            }
            else
            {
                Settings = new LocalAchievementsSettings();
                SetDefaultPaths();
            }

            ReloadThemes();
        }

        private void EnsureSettingsDefaults()
        {
            if (Settings.AchievementPaths == null)
            {
                Settings.AchievementPaths = new ObservableCollection<string>();
            }

            if (Settings.AchievementPaths.Count == 0)
            {
                SetDefaultPaths();
            }

            if (string.IsNullOrWhiteSpace(Settings.AchievementsLanguage))
            {
                Settings.AchievementsLanguage = "English";
            }

            if (string.IsNullOrWhiteSpace(Settings.SelectedTheme))
            {
                Settings.SelectedTheme = "Default.xaml";
            }
        }

        private void SetDefaultPaths()
        {
            Settings.AchievementPaths.Add("%PUBLIC%\\Documents\\Steam\\CODEX");
            Settings.AchievementPaths.Add("%appdata%\\Steam\\CODEX");
            Settings.AchievementPaths.Add("%PUBLIC%\\Documents\\Steam\\RUNE");
            Settings.AchievementPaths.Add("%appdata%\\Steam\\RUNE");
            Settings.AchievementPaths.Add("%appdata%\\Goldberg SteamEmu Saves");
            Settings.AchievementPaths.Add("%appdata%\\GSE Saves");
            Settings.AchievementPaths.Add("%appdata%\\SmartSteamEmu");
            Settings.AchievementPaths.Add("%DOCUMENTS%\\DARKSiDERS");
            Settings.AchievementPaths.Add("%ProgramData%\\Steam");
            Settings.AchievementPaths.Add("%localappdata%\\SKIDROW");
            Settings.AchievementPaths.Add("%DOCUMENTS%\\SKIDROW");
        }

        public void ResetAchievementPathsToDefault()
        {
            Settings.AchievementPaths.Clear();
            SetDefaultPaths();
            Settings.OnPropertyChanged(nameof(Settings.AchievementPathsString));
        }

        public void ReloadThemes()
        {
            AvailableThemes.Clear();
            AvailableThemes.Add("Default.xaml");

            try
            {
                foreach (var themesPath in plugin.GetThemeDirectories())
                {
                    var files = Directory.GetFiles(themesPath, "*.xaml", SearchOption.TopDirectoryOnly)
                        .Select(Path.GetFileName)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

                    foreach (var file in files)
                    {
                        if (!AvailableThemes.Contains(file, StringComparer.OrdinalIgnoreCase))
                        {
                            AvailableThemes.Add(file);
                        }
                    }
                }
            }
            catch
            {
            }

            if (string.IsNullOrWhiteSpace(Settings.SelectedTheme) ||
                !AvailableThemes.Contains(Settings.SelectedTheme, StringComparer.OrdinalIgnoreCase))
            {
                Settings.SelectedTheme = AvailableThemes.FirstOrDefault() ?? "Default.xaml";
            }
        }

        public void BeginEdit()
        {
            editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            Settings = editingClone;
        }

        public void EndEdit()
        {
            plugin.SavePluginSettings(Settings);
            Localization.CurrentLanguage = Settings.AchievementsLanguage;
            plugin.RestartGlobalWatching();
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}
