using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GestionReparaciones.converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        //Una clase para convertir de booleano a la visivilidad que queremos
        public bool Invert { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Comprueba si el valor es un booleano y lo asigna a `boolValue`. Si no lo es, toma false por defecto.
            bool boolValue = value is bool b && b;

            // Si la propiedad `Invert` esta activada, invierte el valor booleano.
            if (Invert)
                boolValue = !boolValue;

            // Si `boolValue` es true, devuelve `Visibility.Visible`, de lo contrario `Visibility.Collapsed`.
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        // Revierte la visibilidad
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Verifica si el valor recibido es del tipo `Visibility`
            if (value is Visibility visibility)
            {
                // Si la propiedad `Invert` esta activa, devuelve true cuando NO sea Visible.
                // Si `Invert` esta desactivada, devuelve true solo cuando sea Visible.
                return Invert ? visibility != Visibility.Visible : visibility == Visibility.Visible;
            }

            // Si el valor no es de tipo `Visibility`, devuelve false por defecto.
            return false;
        }
    }
}
