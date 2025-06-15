﻿using System;
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
            bool boolValue = value is bool b && b;

            if (Invert)
                boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return Invert ? visibility != Visibility.Visible : visibility == Visibility.Visible;
            }
            return false;
        }
    }
}
