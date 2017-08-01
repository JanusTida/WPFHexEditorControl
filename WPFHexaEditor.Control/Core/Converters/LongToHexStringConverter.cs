//////////////////////////////////////////////
// MIT License  - 2016-2017
// Author : Derek Tremblay (derektremblay666@gmail.com)
//////////////////////////////////////////////

using System;
using System.Globalization;
using System.Windows.Data;

namespace WPFHexaEditor.Core.Converters
{
    /// <summary>
    /// Used to convert long value to hexadecimal string like this 0xFFFFFFFF.
    /// </summary>
    public class LongToHexStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long longValue = 0;
            if (long.TryParse(value?.ToString() ?? "0",out longValue)) {
                if (longValue > -1)
                    return "0x" + longValue.ToString(ConstantReadOnly.HexLineInfoStringFormat, CultureInfo.InvariantCulture).ToUpper();
                else
                    return "0x00000000";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}