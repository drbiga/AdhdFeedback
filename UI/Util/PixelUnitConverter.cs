using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace UI.Util
{
    internal class PixelUnitConverter
    {
        public static double PixelsToDipsX(double pixels, Visual visual)
        {
            var dpi = VisualTreeHelper.GetDpi(visual);
            return pixels * (96.0 / dpi.DpiScaleX / 96.0);
        }

        public static double PixelsToDipsY(double pixels, Visual visual)
        {
            var dpi = VisualTreeHelper.GetDpi(visual);
            return pixels * (96.0 / dpi.DpiScaleY / 96.0);
        }

        public static double DipsToPixelsX(double dips, Visual visual)
        {
            var dpi = VisualTreeHelper.GetDpi(visual);
            return dips * dpi.DpiScaleX;
        }

        public static double DipsToPixelsY(double dips, Visual visual)
        {
            var dpi = VisualTreeHelper.GetDpi(visual);
            return dips * dpi.DpiScaleY;
        }
    }
}
