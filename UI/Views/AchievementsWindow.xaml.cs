using LocalAchievements.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LocalAchievements
{
    public partial class AchievementsWindow : Window
    {
        public bool SpoilerEnabled { get; set; }
        public string TextUnlocked { get; set; }
        public string TextLocked { get; set; }
        public string WindowTitle { get; set; }

        private ICollectionView view;
        private bool isAscending = true;
        private readonly ExophaseService exoService;
        private readonly Guid gameId;

        public class SortOption { public string Label { get; set; } public string PropertyName { get; set; } }

        public AchievementsWindow(string gameName, List<ExoAchievement> achievements, bool spoilerEnabled, ExophaseService service, Guid gameId)
        {
            InitializeComponent();

            WindowTitle = Localization.Get("MenuView"); // O "Logros" si prefieres

            SpoilerEnabled = spoilerEnabled;
            // Reutilizamos claves existentes de Localization.cs
            TextUnlocked = Localization.Get("StatusUnlocked");
            TextLocked = Localization.Get("StatusLocked");
            DataContext = this;

            this.exoService = service;
            this.gameId = gameId;

            // Título
            TitleText.Text = $"{gameName}";

            // Lógica de Ordenamiento
            ComboSort.ItemsSource = new List<SortOption>
            {
                // "FilterAll" ahora es "Desbloqueados" / "Unlocked" en tu Localization
                new SortOption { Label = Localization.Get("FilterAll"), PropertyName = "Default" },
                new SortOption { Label = Localization.Get("SortName"), PropertyName = "Name" },
                new SortOption { Label = Localization.Get("SortUnlocked"), PropertyName = "IsUnlocked" },
                new SortOption { Label = Localization.Get("SortRarity"), PropertyName = "Rarity" }
            };

            view = CollectionViewSource.GetDefaultView(achievements);
            view.Filter = FilterLogic;
            AchList.ItemsSource = view;

            // Selección inicial
            ComboSort.SelectedIndex = 0;
            ApplySort("Default");

            // Calcular Progreso Inicial
            UpdateProgress(achievements);

            if (this.exoService != null)
            {
                this.exoService.AchievementUpdated += OnAchievementUpdated;
            }

            this.Closed += (s, e) =>
            {
                if (this.exoService != null)
                    this.exoService.AchievementUpdated -= OnAchievementUpdated;
            };
        }

        private void UpdateProgress(List<ExoAchievement> list)
        {
            if (list == null) return;
            int total = list.Count;
            int unlocked = list.Count(x => x.IsUnlocked);
            int percent = total > 0 ? (int)((double)unlocked / total * 100) : 0;

            AchProgressBar.Maximum = total;
            AchProgressBar.Value = unlocked;
            ProgressTextLabel.Text = $"{unlocked}/{total} ({percent}%)";
        }

        private void OnAchievementUpdated(Guid updatedGameId, string apiName, bool newState)
        {
            if (updatedGameId == this.gameId) UpdateState(apiName, newState);
        }

        private bool FilterLogic(object item)
        {
            if (item is ExoAchievement ach)
            {
                if (!string.IsNullOrEmpty(TxtSearch.Text))
                {
                    if (ach.Name != null && ach.Name.IndexOf(TxtSearch.Text, StringComparison.OrdinalIgnoreCase) < 0) return false;
                }
            }
            return true;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => view.Refresh();

        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboSort.SelectedItem is SortOption opt) ApplySort(opt.PropertyName);
        }

        private void BtnOrder_Click(object sender, RoutedEventArgs e)
        {
            isAscending = !isAscending;
            BtnOrder.Content = isAscending ? "⬇" : "⬆";
            if (ComboSort.SelectedItem is SortOption opt) ApplySort(opt.PropertyName);
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Text = "";
            ComboSort.SelectedIndex = 0;
            isAscending = true;
            BtnOrder.Content = "⬇";
            view.Refresh();
        }

        private void ApplySort(string prop)
        {
            view.SortDescriptions.Clear();
            if (!string.IsNullOrEmpty(prop))
            {
                // --- AQUÍ ESTÁ EL ARREGLO ---
                if (prop == "Default")
                {
                    // Lógica para booleanos:
                    // True (1) > False (0).
                    // Descending: True primero (Desbloqueados arriba).
                    // Ascending: False primero (Bloqueados arriba).

                    var direction = isAscending ? ListSortDirection.Descending : ListSortDirection.Ascending;

                    // 1. Aplicamos el orden al grupo (Desbloqueados vs Bloqueados)
                    view.SortDescriptions.Add(new SortDescription("IsUnlocked", direction));

                    // 2. Dentro del grupo, mantenemos el orden del índice original (0, 1, 2...)
                    // Esto hace que la historia del juego siga teniendo sentido aunque inviertas los grupos.
                    view.SortDescriptions.Add(new SortDescription("Index", ListSortDirection.Ascending));
                }
                else
                {
                    // Lógica estándar para el resto
                    view.SortDescriptions.Add(new SortDescription(prop, isAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));
                }
            }
        }

        public void UpdateState(string incomingApiName, bool newState)
        {
            if (string.IsNullOrEmpty(incomingApiName)) return;
            string incomingNorm = Normalize(incomingApiName);

            Application.Current.Dispatcher.Invoke(() =>
            {
                var sourceList = AchList.ItemsSource as IEnumerable<ExoAchievement>;
                if (sourceList == null) return;

                var fullList = sourceList.ToList();
                bool changed = false;

                foreach (ExoAchievement item in fullList)
                {
                    string itemId = item.ApiName ?? item.Name;
                    if (string.Equals(Normalize(itemId), incomingNorm, StringComparison.OrdinalIgnoreCase))
                    {
                        if (item.IsUnlocked != newState)
                        {
                            item.IsUnlocked = newState;
                            item.UnlockedDate = newState ? (DateTime?)DateTime.Now : null;
                            changed = true;
                        }
                        break;
                    }
                }

                if (changed)
                {
                    view.Refresh();
                    UpdateProgress(fullList);
                }
            });
        }

        private string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return Regex.Replace(input, "[^a-zA-Z0-9]", "").ToLowerInvariant();
        }
    }
}