using Prueba.viewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Prueba.helpers
{
    public class ComboBoxSelectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Usamos el parámetro para obtener el valor a comparar
            string? targetValue = parameter as string;

            if (value != null)
            {
                // Normalizamos las cadenas para eliminar las diferencias diacríticas (acentos, tildes)
                string normalizedValue = value.ToString().Normalize(NormalizationForm.FormD);
                string normalizedTargetValue = targetValue.Normalize(NormalizationForm.FormD);

                // Comparamos sin importar las tildes y sin tener en cuenta mayúsculas/minúsculas
                if (string.Compare(normalizedValue, normalizedTargetValue, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;  // No necesitamos implementar conversión hacia atrás
        }
    }
}
