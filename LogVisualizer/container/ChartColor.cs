using System.Collections.Generic;
using System.Windows.Media;

namespace LogVisualizer.data {
    internal static class ChartColor {

        public static List<SolidColorBrush> colorList = new List<SolidColorBrush>() {
            new SolidColorBrush(Color.FromArgb(255,78,121,166)),
            new SolidColorBrush(Color.FromArgb(255,225,86,89)),
            new SolidColorBrush(Color.FromArgb(255,242,142,44)),
            new SolidColorBrush(Color.FromArgb(255,118,183,177)),
        };
    }
}
