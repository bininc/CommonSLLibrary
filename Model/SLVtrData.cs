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

namespace CommonLibSL.Model
{
    public class SLVtrData
    {
        public string DataStr { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime Time { get; set; }
        public SLCar Car { get; set; }
    }
}
