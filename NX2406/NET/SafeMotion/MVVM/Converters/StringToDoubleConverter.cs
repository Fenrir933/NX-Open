using System;
using System.Globalization;
using System.Windows.Data;

namespace SafeMotion.MVVM.Converters {
    public class StringToDoubleConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (double.TryParse(value.ToString(), out double result)) {
                if (result > 0) return -result;
                if (result < -1000) return -1000;

                return result;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }
}
