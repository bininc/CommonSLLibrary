using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CommonLibSL.Controls;

namespace CommonLibSL.Map
{
    public partial class MapCarInfoItem : SLUserControl
    {
        private static readonly BitmapImage bg_top =
            new BitmapImage(new Uri("/ImageResource;component/Images/duihuakang_bg_top.png", UriKind.Relative));
        private static readonly BitmapImage bg_bottom = new BitmapImage(new Uri("/ImageResource;component/Images/duihuakang_bg_bottom.png", UriKind.Relative));
        private static readonly BitmapImage bg_arrow = new BitmapImage(new Uri("/ImageResource;component/Images/duihuakang_bg_arrow.png", UriKind.Relative));
        private static readonly BitmapImage bg_even = new BitmapImage(new Uri("/ImageResource;component/Images/duihuakang_bg_even.png", UriKind.Relative));
        private static readonly BitmapImage bg_odd= new BitmapImage(new Uri("/ImageResource;component/Images/duihuakang_bg_odd.png", UriKind.Relative));

        private MapCarInfoItem()
        {
            InitializeComponent();
        }

        public static MapCarInfoItem GetCarInfoItemByIndex(int index)
        {
            if (index < -2) return null;
            MapCarInfoItem item = new MapCarInfoItem();
            item.Width = 300;
            if (index == 0)
            {
                item.Height = 28;
                item.BgImage.ImageSource = bg_top;
                item.LblTitle.Margin = new Thickness(17, 4, 0, 0);
                item.LblText.Margin = new Thickness(4, 5, 8, 0);

            }
            else if (index == -1)
            {   //最后一个
                item.Height = 30;
                item.BgImage.ImageSource = bg_bottom;
                item.LblTitle.Margin = new Thickness(17, 1, 0, 7);
                item.LblText.Margin = new Thickness(4, 1, 8, 7);
            }
            else if (index == -2)
            {   //倒数第二个 箭头
                item.Height = 25;
                item.BgImage.ImageSource = bg_arrow;
                item.LblTitle.Margin = new Thickness(17, 1, 0, 0);
                item.LblText.Margin = new Thickness(4, 1, 8, 0);
            }
            else
            {
                item.Height = 25;
                item.LblTitle.Margin = new Thickness(17, 1, 0, 0);
                item.LblText.Margin = new Thickness(4, 1, 8, 0);
                if ((index + 1) % 2 == 0) //因为索引从0开始 所以需要加1
                {   //偶数
                    item.BgImage.ImageSource = bg_even;
                }
                else
                {   //奇数
                    item.BgImage.ImageSource = bg_odd;
                }
            }

            return item;
        }

        public string Title
        {
            get
            {
                return LblTitle.Text;
            }
            set
            {
                LblTitle.Text = value;
            }
        }

        public string Text
        {
            get
            {
                return LblText.Text;
            }
            set
            {
                LblText.Text = value;
            }
        }

        public void Binding(MapCarInfoBindItem item)
        {
            if (item == null) return;
            LblTitle.SetBinding(TextBlock.TextProperty, new Binding("Title"));
            LblTitle.DataContext = item;
            LblText.SetBinding(TextBlock.TextProperty, new Binding("Text"));
            LblText.DataContext = item;
            TextToolTip.SetBinding(TextBlock.TextProperty, new Binding("Text"));
            TextToolTip.DataContext = item;
        }
    }
}
