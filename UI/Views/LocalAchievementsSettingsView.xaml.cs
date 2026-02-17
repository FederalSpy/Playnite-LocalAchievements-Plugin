using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LocalAchievements
{
    public partial class LocalAchievementsSettingsView : UserControl
    {
        public LocalAchievementsSettingsView()
        {
            InitializeComponent();
            Loaded += LocalAchievementsSettingsView_Loaded;
        }

        private void LocalAchievementsSettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as LocalAchievementsSettingsViewModel)?.ReloadThemes();
        }

        private void ComboThemes_DropDownOpened(object sender, EventArgs e)
        {
            (DataContext as LocalAchievementsSettingsViewModel)?.ReloadThemes();
        }

        private void BtnBrowseSound_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LocalAchievementsSettingsViewModel;
            if (vm?.Settings == null)
            {
                return;
            }

            var dlg = new OpenFileDialog
            {
                Title = Localization.Get("LOCSelectSoundTitle"),
                Filter = $"{Localization.Get("LOCSoundPathLabel")}|*.mp3;*.wav"
            };

            if (dlg.ShowDialog() == true)
            {
                vm.Settings.SoundPath = dlg.FileName;
            }
        }

        private void BtnTestSound_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LocalAchievementsSettingsViewModel;
            if (vm?.Plugin == null)
            {
                return;
            }

            var notificationService = new NotificationService(vm.Plugin);
            notificationService.ShowNotification(
                Localization.Get("LOCTestTitle"),
                Localization.Get("LOCTestDesc"),
                null);
        }

        private void BtnBrowsePath_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LocalAchievementsSettingsViewModel;
            if (vm?.Settings?.AchievementPaths == null)
            {
                return;
            }

            var dialog = new OpenFileDialog
            {
                Title = Localization.Get("LOCSettingsPaths"),
                Filter = "Folder|*.folder",
                CheckFileExists = false,
                CheckPathExists = true,
                ValidateNames = false,
                FileName = "Select folder",
                DereferenceLinks = true
            };

            if (!string.IsNullOrWhiteSpace(vm.Settings.AchievementPaths.FirstOrDefault()))
            {
                var firstExisting = vm.Settings.AchievementPaths
                    .Select(Environment.ExpandEnvironmentVariables)
                    .FirstOrDefault(Directory.Exists);
                if (!string.IsNullOrWhiteSpace(firstExisting))
                {
                    dialog.InitialDirectory = firstExisting;
                }
            }

            if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.FileName))
            {
                return;
            }

            var selectedPath = dialog.FileName;
            if (File.Exists(selectedPath))
            {
                selectedPath = Path.GetDirectoryName(selectedPath);
            }
            else if (!Directory.Exists(selectedPath))
            {
                var parent = Path.GetDirectoryName(selectedPath);
                if (!string.IsNullOrWhiteSpace(parent) && Directory.Exists(parent))
                {
                    selectedPath = parent;
                }
            }

            if (string.IsNullOrWhiteSpace(selectedPath) || !Directory.Exists(selectedPath))
            {
                return;
            }

            bool exists = vm.Settings.AchievementPaths.Any(x =>
                string.Equals(x, selectedPath, StringComparison.OrdinalIgnoreCase));
            if (!exists)
            {
                vm.Settings.AchievementPaths.Add(selectedPath);
            }
        }

        private void BtnRemovePath_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LocalAchievementsSettingsViewModel;
            if (vm?.Settings?.AchievementPaths == null)
            {
                return;
            }

            var path = (sender as Button)?.Tag as string;
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var existing = vm.Settings.AchievementPaths.FirstOrDefault(x =>
                string.Equals(x, path, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                vm.Settings.AchievementPaths.Remove(existing);
            }
        }

        private void BtnResetPaths_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LocalAchievementsSettingsViewModel;
            if (vm == null)
            {
                return;
            }

            vm.ResetAchievementPathsToDefault();
        }
    }
}
