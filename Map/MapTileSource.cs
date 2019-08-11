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
using Microsoft.Maps.MapControl;
using System.Text.RegularExpressions;
using System.Windows.Browser;

namespace CommonLibSL.Map
{
    public class MapTileSource : TileSource
    {
        public MapTileSource(string uriFormat)            
        {
            this.UriFormat = uriFormat;
        }

        public new string UriFormat
        {
            get
            {
                return base.UriFormat;
            }
            set
            {
                if (base.UriFormat == value)
                    return;
                value= Regex.Replace(value, "{UriScheme}", PageUriScheme, RegexOptions.IgnoreCase);
                base.UriFormat = value;               
            }
        }

        public static string PageUriScheme
        {
            get
            {
                return !HtmlPage.IsEnabled ? "HTTP" : HtmlPage.Document.DocumentUri.Scheme;
            }
        }
    }
}
