using System;
using System.Globalization;
using System.Windows.Data;

namespace ToolCreator.MVVM.Converter {
    internal class IntegerConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            int val;

            try {
                val = System.Convert.ToInt32(value);
            }
            catch (Exception) {
                val = 1;
            }

            return val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            int val;

            try {
                val = System.Convert.ToInt32(value);
            }
            catch (Exception) {
                val = 1;
            }

            return val;
        }
    }
}
