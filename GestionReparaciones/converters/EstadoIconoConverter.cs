using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using FontAwesome.Sharp;

namespace GestionReparaciones.converters
{
    public class EstadoIconoConverter : IValueConverter
    {
        // Metodo Conver convierte un valor booleano a un icono FontAwesome.
        // Si el valor es true, devuelve el icono de check y si no devuelve una X.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b) ? IconChar.Check : IconChar.Times;

        // No hace falta por ahora pero lo dejo por si mas adelante lo utilizo
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}


