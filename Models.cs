using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Playnite.SDK.Data;

namespace LocalAchievements
{
    public class LocalUnlockInfo
    {
        public string ApiName { get; set; }
        public bool IsUnlocked { get; set; }
        public DateTime UnlockDate { get; set; }
        public int Index { get; set; } = int.MaxValue;
    }

    // AHORA IMPLEMENTA INotifyPropertyChanged PARA ACTUALIZACIÓN EN TIEMPO REAL
    public class ExoAchievement : INotifyPropertyChanged
    {
        private bool _isUnlocked;
        private DateTime? _unlockedDate;
        private int _index = int.MaxValue;

        public string Name { get; set; }
        public string ApiName { get; set; } // Clave técnica (ej: ACHIEVEMENT_1)
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public float Rarity { get; set; }
        public bool IsSecret { get; set; }

        public int Index
        {
            get => _index;
            set { if (_index != value) { _index = value; OnPropertyChanged(); } }
        }

        public bool IsUnlocked
        {
            get => _isUnlocked;
            set
            {
                if (_isUnlocked != value)
                {
                    _isUnlocked = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? UnlockedDate
        {
            get => _unlockedDate;
            set
            {
                if (_unlockedDate != value)
                {
                    _unlockedDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class AchievementChange
    {
        public ExoAchievement Exo { get; set; }
        public bool NewState { get; set; }
        public DateTime? UnlockedDate { get; set; }
    }

    public class SchemaResult
    {
        public System.Collections.Generic.Dictionary<string, string> Schema { get; set; } =
            new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

        public System.Collections.Generic.List<string> OrderedApiNames { get; set; } =
            new System.Collections.Generic.List<string>();
    }
}
