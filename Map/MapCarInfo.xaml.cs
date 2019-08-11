using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CommonLibSL.Map
{
    public partial class MapCarInfo : UserControl
    {
        public MapCarInfo()
        {
            InitializeComponent();
            Width = 300;
        }

        //public string CarName { get { return LblCarName.Text; } set { LblCarName.Text = value; } }
        //public string GnssTime { get { return LblGnssTime.Text; } set { LblGnssTime.Text = value; } }
        //public string Speed { get { return LblSpeed.Text; } set { LblSpeed.Text = value; } }
        //public string Position { get { return LblPosition.Text; } set { LblPosition.Text = value; } }
        //public string LockStr { get { return LblLockStr.Text; } set { LblLockStr.Text = value; } }
        //public string AreaStr { get { return LblAreaStr.Text; } set { LblAreaStr.Text = value; } }
        //public string TaskStr { get { return LblTaskStr.Text; } set { LblTaskStr.Text = value; } }

        //public void SetCarInfo(string carName, SLGpsData gpsData, SLLockGpsData lockData)
        //{
        //    CarName = carName;

        //    if (gpsData != null)
        //    {
        //        GnssTime = gpsData.GNSSTIME.ToString();
        //        Speed = gpsData.SPEED + " km/h";
        //        Position = gpsData.POSITION != null ? gpsData.POSITION.TrimStart() : "";
        //        AreaStr = gpsData.AREA_STR;
        //    }

        //    if (lockData != null)
        //    {
        //        if (string.IsNullOrWhiteSpace(lockData.MainLockState_Str))
        //            LockStr = "";
        //        else
        //        {
        //            StringBuilder sb = new StringBuilder("中控");
        //            sb.Append(lockData.MainLockState_Str);
        //            if (!string.IsNullOrWhiteSpace(lockData.Lock1_Str) && lockData.Lock1_Str == "开")
        //                sb.AppendFormat(" 进1{0}", lockData.Lock1_Str);
        //            if (!string.IsNullOrWhiteSpace(lockData.Lock2_Str) && lockData.Lock2_Str == "开")
        //                sb.AppendFormat(" 进2{0}", lockData.Lock2_Str);
        //            if (!string.IsNullOrWhiteSpace(lockData.Lock3_Str) && lockData.Lock3_Str == "开")
        //                sb.AppendFormat(" 进3{0}", lockData.Lock3_Str);
        //            if (!string.IsNullOrWhiteSpace(lockData.Lock4_Str) && lockData.Lock4_Str == "开")
        //                sb.AppendFormat(" 进4{0}", lockData.Lock4_Str);
        //            if (!string.IsNullOrWhiteSpace(lockData.Lock5_Str) && lockData.Lock5_Str == "开")
        //                sb.AppendFormat(" 左1{0}", lockData.Lock5_Str);
        //            if (!string.IsNullOrWhiteSpace(lockData.Lock6_Str) && lockData.Lock6_Str == "开")
        //                sb.AppendFormat(" 左2{0}", lockData.Lock6_Str);
        //            if (!string.IsNullOrWhiteSpace(lockData.Lock7_Str) && lockData.Lock7_Str == "开")
        //                sb.AppendFormat(" 左3{0}", lockData.Lock7_Str);
        //            if (!string.IsNullOrWhiteSpace(lockData.Lock8_Str) && lockData.Lock8_Str == "开")
        //                sb.AppendFormat(" 左4{0}", lockData.Lock8_Str);
        //            if (!string.IsNullOrWhiteSpace(lockData.Lock9_Str) && lockData.Lock9_Str == "开")
        //                sb.AppendFormat(" 右1{0}", lockData.Lock9_Str);
        //            if (!string.IsNullOrWhiteSpace(lockData.Lock10_Str) && lockData.Lock10_Str == "开")
        //                sb.AppendFormat(" 右2{0}", lockData.Lock10_Str);
        //            if (!string.IsNullOrWhiteSpace(lockData.Lock11_Str) && lockData.Lock11_Str == "开")
        //                sb.AppendFormat(" 右3{0}", lockData.Lock11_Str);
        //            if (!string.IsNullOrWhiteSpace(lockData.Lock12_Str) && lockData.Lock12_Str == "开")
        //                sb.AppendFormat(" 右4{0}", lockData.Lock12_Str);
        //            LockStr = sb.ToString();
        //        }
        //}
        //}

        public void Bind(MapCarInfoBindItem[] items)
        {
            if (items == null || items.Length == 0) return;
            WrapPanel.Children.Clear();
            double height = 0;
            for (var i = 0; i < items.Length; i++)
            {
                MapCarInfoItem item = null;
                if (i < items.Length - 2)
                    item = MapCarInfoItem.GetCarInfoItemByIndex(i);
                else if (i == items.Length - 2)
                    item = MapCarInfoItem.GetCarInfoItemByIndex(-2);
                else if (i == items.Length - 1)
                    item = MapCarInfoItem.GetCarInfoItemByIndex(-1);

                if (item != null)
                {
                    item.Binding(items[i]);
                    height += item.Height;
                    WrapPanel.Children.Add(item);
                }
            }
            Height = height;
        }
    }

    public class MapCarInfoBindItem : DependencyObject
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(MapCarInfoBindItem), new PropertyMetadata(default(string)));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(MapCarInfoBindItem), new PropertyMetadata(default(string)));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public MapCarInfoBindItem() { }

        public MapCarInfoBindItem(string title, string text)
        {
            Title = title;
            Text = text;
        }
    }

    public class MapCarInfoBindItems
    {
        private List<MapCarInfoBindItem> carInfoItems = new List<MapCarInfoBindItem>();

        public MapCarInfoBindItems(params MapCarInfoBindItem[] items)
        {
            carInfoItems.AddRange(items);
        }

        public MapCarInfoBindItem this[string title]
        {
            get
            {
                return carInfoItems?.FirstOrDefault(item => item.Title == title);
            }
        }

        public MapCarInfoBindItem this[int index]
        {
            get
            {
                return carInfoItems[index];
            }
            set
            {
                if (value != null)
                    carInfoItems[index] = value;
            }
        }

        public MapCarInfoBindItem[] ToArray()
        {
            return carInfoItems.ToArray();
        }
    }
}
