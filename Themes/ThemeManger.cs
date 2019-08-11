using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CommonLibSL.Themes
{
    public static class ThemeManger
    {
        private static ResourceDictionary resources = null;
        static ThemeManger()
        {
            resources = new ResourceDictionary();
            resources.Source = new Uri("/CommonLibSL;component/Themes/Generic.xaml", UriKind.Relative);
        }

        public static Style GetStyle(string key)
        {
            Style style = resources[key] as Style;
            return style;
        }

    }
}
