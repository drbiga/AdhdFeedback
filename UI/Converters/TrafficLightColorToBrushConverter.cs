using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

using LightColor = UI.ViewModels.TrafficLightViewModel.LightColor;

namespace UI.Converters
{
    public class TrafficLightColorToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] = CurrentColor (TrafficLightColor enum)
            // values[1] = Brush to use if this light is active
            // values[2] = Brush to use if this light is off

            if (values[0] is not LightColor currentColor) return values[2];
            if (values[1] is not RadialGradientBrush onBrush) return values[2];
            if (values[2] is not RadialGradientBrush offBrush) return values[2];

            // parameter = string "Red", "Yellow", "Green" to indicate which light this is
            var lightName = parameter as string;

            return currentColor switch
            {
                LightColor.Red when lightName == "Red" => onBrush,
                LightColor.Yellow when lightName == "Yellow" => onBrush,
                LightColor.Green when lightName == "Green" => onBrush,
                _ => offBrush
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

}
