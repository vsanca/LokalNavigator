using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace RA1_2013
{
    public class HelpProvider
    {
        public static string GetHelpKey(DependencyObject obj)
        {
            if (obj != null)
                return obj.GetValue(HelpKeyProperty) as string;
            else
                return "index";
        }

        public static void SetHelpKey(DependencyObject obj, string value)
        {
            obj.SetValue(HelpKeyProperty, value);
        }

        public static readonly DependencyProperty HelpKeyProperty =
            DependencyProperty.RegisterAttached("HelpKey", typeof(string), typeof(HelpProvider), new PropertyMetadata("index", HelpKey));
        private static void HelpKey(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //NOOP
        }

        public static void ShowHelp(string key, Window originator)
        {
            HelpViewer hh = new HelpViewer(key, originator);
            hh.Show();
        }
    }
}
