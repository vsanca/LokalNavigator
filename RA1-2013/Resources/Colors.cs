using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RA1_2013.Resources
{
    class Colors
    {
        public static readonly Color c1 = Color.FromRgb(42, 24, 70);
        public static readonly Color c2 = Color.FromRgb(127, 71, 210);
        public static readonly Color c3 = Color.FromRgb(81, 45, 134);
        public static readonly Color c4 = Color.FromRgb(89, 49, 147);
        public static readonly Color c5 = Color.FromRgb(66, 36, 108);

        public static readonly SolidColorBrush scb1 = new SolidColorBrush(c1);
        public static readonly SolidColorBrush DialogFrame = new SolidColorBrush(c4);
        public static readonly SolidColorBrush scb3 = new SolidColorBrush(c2);
        public static readonly SolidColorBrush MainFrame = new SolidColorBrush(c3);
        public static readonly SolidColorBrush DialogFrameAlt = new SolidColorBrush(c5);
    }
}
