using System;
using LocalAchievements;

namespace LocalAchievements.Services
{
    public class AchievementChange
    {
        public ExoAchievement Exo { get; set; }
        public bool NewState { get; set; }
        // CORRECCIÓN: DateTime? permite nulos
        public DateTime? UnlockedDate { get; set; }
    }
}