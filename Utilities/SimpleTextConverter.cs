using System;
using System.Globalization;
using System.Windows.Data;

namespace LocalAchievements
{
    public class SimpleTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Si el XAML pide una clave (ej: 'LOCSettingsPath'), intentamos traducir
            if (parameter is string key)
            {
                // Usa tu clase Localization. Si devuelve null, usa la clave como fallback.
                return Localization.Get(key) ?? key;
            }
            // Si no hay parámetro, devuelve el valor tal cual (ToString)
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}