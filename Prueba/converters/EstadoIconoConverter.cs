using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using FontAwesome.Sharp;

namespace Prueba.converters
{
    public class EstadoIconoConverter : IValueConverter
    {
        //Cambiar el icono segun el estado
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b) ? IconChar.Check : IconChar.Times;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}


