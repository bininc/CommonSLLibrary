using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommLiby;
using Microsoft.Maps.MapControl;

namespace CommonLibSL.Map
{
    public enum DrawType { None = -1, DrawRect = 0, DrawPolygon = 1, DrawCircle = 2, DrawLine = 3, DrawCustomPoint = 4, DrawCalLength = 5, DrawCalArea = 6 };
    public delegate void EndDrawHandler(object sender, List<Location> locations, double result, object tag);

    public class MapDrawTool
    {
        private Microsoft.Maps.MapControl.Map map;
        private MapLayer lyr;

        private DrawType _dwType = DrawType.None;

        private DrawType DwType
        {
            get { return _dwType; }
            set
            {
                _dwType = value;
                map.Cursor = _dwType == DrawType.None ? Cursors.Arrow : Cursors.Stylus;
                if (_dwType == DrawType.None)
                {
                    if (_tag != null && !dicLines.ContainsKey(_tag))
                        currentLine.ForEach(x => lyr.Children.Remove(x));
                    if (EndDrawEvent != null)
                        EndDrawEvent(this, locationList, -1, _tag);
                }
            }
        }

        private List<Location> locationList = new List<Location>();
        private MapPolyline line = new MapPolyline();
        private MapPolygon poly = new MapPolygon();
        private Dictionary<object, List<UIElement>> dicLines = new Dictionary<object, List<UIElement>>();
        private List<UIElement> currentLine = new List<UIElement>();
        private object _tag;


        #region 工具小控件

        private Border _movePanel;

        private Border movePanel
        {
            get { return _movePanel; }
            set
            {
                _movePanel = value;
                if (_movePanel != null)
                {
                    _movePanel.BorderBrush = new SolidColorBrush(Colors.Red);
                }
            }
        }

        /// <summary>
        /// 更新位置
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="tip"></param>
        /// <param name="l"></param>
        void UpdateMovePanel(string msg, string tip, Location l)
        {
            if (movePanel == null) return;
            StackPanel sp = movePanel.Child as StackPanel;
            if (sp != null)
            {
                TextBlock txtMsg = sp.Children[0] as TextBlock;
                TextBlock txtTip = sp.Children[1] as TextBlock;
                if (txtMsg != null && msg != null)
                {
                    txtMsg.Text = msg;
                    txtMsg.Visibility = string.IsNullOrWhiteSpace(msg) ? Visibility.Collapsed : Visibility.Visible;
                }
                if (txtTip != null && tip != null)
                {
                    txtTip.Text = tip;
                    txtTip.Visibility = string.IsNullOrWhiteSpace(tip) ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            MapLayer.SetPosition(movePanel, l);
            Canvas.SetZIndex(currentLine.Last(), 2);
            Canvas.SetZIndex(sp, 1);
        }

        Border CreatePanel(string msg, string tip = "单击确定起点")
        {
            Border border = new Border();
            border.BorderThickness = new Thickness(1);
            border.BorderBrush = new SolidColorBrush(Colors.Gray);
            border.CornerRadius = new CornerRadius(2);
            border.Background = new SolidColorBrush(Colors.White);
            border.Padding = new Thickness(2);
            StackPanel _panel = new StackPanel();
            _panel.Orientation = Orientation.Vertical;
            TextBlock txtMsg = new TextBlock() { Text = msg, FontSize = 10 };
            txtMsg.Visibility = string.IsNullOrWhiteSpace(msg) ? Visibility.Collapsed : Visibility.Visible;
            _panel.Children.Add(txtMsg);
            TextBlock txtTip = new TextBlock()
            {
                Text = tip,
                Foreground = new SolidColorBrush(Colors.Gray),
                FontSize = 10
            };
            txtTip.Visibility = string.IsNullOrWhiteSpace(tip) ? Visibility.Collapsed : Visibility.Visible;
            _panel.Children.Add(txtTip);

            border.Child = _panel;
            return border;
        }

        Image CreatePoint()
        {
            Image img = new Image() { Source = new BitmapImage(new Uri("/ImageResource;component/Images/map/drawPoint.png", UriKind.Relative)), Stretch = Stretch.None, Width = 12, Height = 12 };
            return img;
        }

        Image CreateDeleteButton()
        {
            Image img = new Image() { Source = new BitmapImage(new Uri("/ImageResource;component/Images/map/delLine.png", UriKind.Relative)), Stretch = Stretch.None, Width = 12, Height = 12 };
            img.Tag = _tag;
            img.Cursor = Cursors.Hand;
            img.MouseLeftButtonDown += (x, y) =>
            {
                object key = ((Image)x).Tag;
                RemoveByTag(key);
            };
            return img;
        }

        #endregion

        public void Ini(Microsoft.Maps.MapControl.Map AMap, MapLayer ALayer)
        {
            map = AMap;
            map.MouseClick += new EventHandler<MapMouseEventArgs>(map_MouseClick);
            map.MouseRightButtonDown += new MouseButtonEventHandler(map_MouseRightButtonDown);
            map.MouseMove += new MouseEventHandler(map_MouseMove);
            map.MouseDoubleClick += map_MouseDoubleClick;
            lyr = ALayer;
        }

        public MapDrawTool()
        {

        }

        #region 事件控制

        private void map_MouseClick(object sender, MapMouseEventArgs e)
        {
            if (DwType != DrawType.None)
            {
                e.Handled = true;
                Location location = map.ViewportPointToLocation(e.ViewportPoint);
                StartDraw(location);
            }
        }

        void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (DwType != DrawType.None)
            {
                Location l = map.ViewportPointToLocation(e.GetPosition(map));
                DrawMove(l);
            }
        }

        void map_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Location location = map.ViewportPointToLocation(e.GetPosition(map));
            if (DwType != DrawType.None)
            {
                e.Handled = true;
                EndDraw();
            }
        }

        void map_MouseDoubleClick(object sender, MapMouseEventArgs e)
        {
            if (DwType != DrawType.None)
            {
                e.Handled = true;
                //Location location = map.ViewportPointToLocation(e.ViewportPoint);
                EndDraw();
            }
        }

        #endregion

        #region 画图相关

        void AddToMapLayer(UIElement element, Location location, PositionOrigin origin = default(PositionOrigin))
        {
            lyr.AddChild(element, location, origin);
            currentLine.Add(element);
        }

        void StartDraw(Location location)
        {
            if (DwType == DrawType.DrawLine || DwType == DrawType.DrawCalLength)
            {
                if (!locationList.Any())
                {
                    line = new MapPolyline();
                    line.Locations = new LocationCollection() { location };
                    line.StrokeThickness = 2;
                    line.Opacity = 1;
                    line.Stroke = new SolidColorBrush(Color.FromArgb(200, 253, 128, 68));
                    line.StrokeLineJoin = PenLineJoin.Round;
                    locationList.Add(location);
                    lyr.Children.Add(line);
                    currentLine.Add(line);

                    AddToMapLayer(CreatePanel(null, "起点"), location, PositionOrigin.TopLeft);
                }
                else
                {
                    locationList.Add(location);
                    if (DwType == DrawType.DrawCalLength)
                    {
                        var panel = CreatePanel(null, GetDistanceStringFromLocations(locationList));
                        AddToMapLayer(panel, location, PositionOrigin.TopLeft);
                    }

                    if (line.Locations.Count == locationList.Count)
                    {
                        line.Locations[line.Locations.Count - 1] = location;
                    }
                }

                AddToMapLayer(CreatePoint(), location, PositionOrigin.Center);
            }
            else if (DwType == DrawType.DrawPolygon || DwType == DrawType.DrawCalArea)
            {
                if (!locationList.Any())
                {
                    poly = new MapPolygon();
                    poly.Locations = new LocationCollection() { location };
                    poly.StrokeThickness = 2;
                    poly.Opacity = 0.5;
                    poly.Stroke = new SolidColorBrush(Colors.Red);
                    poly.Fill = new SolidColorBrush(Color.FromArgb(200, 253, 128, 68));
                    poly.StrokeLineJoin = PenLineJoin.Round;
                    locationList.Add(location);
                    lyr.Children.Add(poly);
                    currentLine.Add(poly);
                }
                else
                {
                    locationList.Add(location);
                    if (poly.Locations.Count == locationList.Count)
                    {
                        poly.Locations[poly.Locations.Count - 1] = location;
                    }
                }
                AddToMapLayer(CreatePoint(), location, PositionOrigin.Center);
            }
            else if (DwType == DrawType.DrawRect)
            {
                if (!locationList.Any())
                {
                    poly = new MapPolygon();
                    poly.Locations = new LocationCollection() { location, location, location, location };
                    poly.StrokeThickness = 2;
                    poly.Opacity = 0.5;
                    poly.Stroke = new SolidColorBrush(Colors.Red);
                    poly.Fill = new SolidColorBrush(Color.FromArgb(200, 253, 128, 68));
                    poly.StrokeLineJoin = PenLineJoin.Round;

                    locationList.Add(location);
                    lyr.Children.Add(poly);
                    currentLine.Add(poly);
                }
                else
                {
                    locationList.Add(location);

                    var del = CreateDeleteButton();
                    lyr.AddChild(del, location, new System.Windows.Point(-13, 6));
                    currentLine.Add(del);
                    dicLines.Add(_tag, currentLine);

                    if (EndDrawRect != null)
                        EndDrawRect(this, locationList, -1, _tag);
                    DwType = DrawType.None;
                }
            }
            else if (DwType == DrawType.DrawCustomPoint)
            {
                locationList.Add(location);

                UpdateMovePanel(null, "", location);
                AddToMapLayer(CreatePoint(), location, PositionOrigin.Center);
                var del = CreateDeleteButton();
                lyr.AddChild(del, location, new System.Windows.Point(-13, 6));
                currentLine.Add(del);
                dicLines.Add(_tag, currentLine);

                if (EndDrawCustomPoint != null)
                    EndDrawCustomPoint(this, locationList, -1, _tag);
                DwType = DrawType.None;
            }
        }

        void DrawMove(Location location)
        {
            if (locationList.Any())
            {
                if (DwType == DrawType.DrawLine || DwType == DrawType.DrawCalLength || DwType == DrawType.DrawPolygon || DwType == DrawType.DrawCalArea)
                {
                    if (DwType == DrawType.DrawLine || DwType == DrawType.DrawCalLength)
                    {
                        if (locationList.Count == line.Locations.Count)
                        {
                            line.Locations.Add(location);
                        }
                        else if (locationList.Count < line.Locations.Count)
                        {
                            line.Locations[line.Locations.Count - 1] = location;
                        }
                    }
                    else
                    {
                        if (locationList.Count == poly.Locations.Count)
                        {
                            poly.Locations.Add(location);
                        }
                        else if (locationList.Count < poly.Locations.Count)
                        {
                            poly.Locations[poly.Locations.Count - 1] = location;
                        }
                    }

                    if (DwType == DrawType.DrawCalLength)
                    {
                        List<Location> tmp = new List<Location>(locationList.ToArray());
                        tmp.Add(location);
                        UpdateMovePanel("总长：" + GetDistanceStringFromLocations(tmp), "单击确定起点，双击结束", location);
                    }
                    else
                    {
                        UpdateMovePanel(null, "单击确定起点，双击结束", location);
                    }
                }
                else if (DwType == DrawType.DrawRect)
                {
                    if (poly.Locations.Count == 4)
                    {
                        Location l1 = new Location(location.Latitude, poly.Locations[0].Longitude);
                        Location l3 = new Location(poly.Locations[0].Latitude, location.Longitude);
                        poly.Locations[1] = l1;
                        poly.Locations[2] = location;
                        poly.Locations[3] = l3;
                    }
                }
            }
            else
            {
                UpdateMovePanel(null, null, location);
            }
        }

        void EndDraw()
        {
            if ((DwType == DrawType.DrawLine || DwType == DrawType.DrawCalLength) && locationList.Any() && (line != null))
            {
                if (line.Locations.Count > locationList.Count)
                    line.Locations.RemoveAt(line.Locations.Count - 1);

                Location lastLocation = locationList.Last();
                var del = CreateDeleteButton();
                lyr.AddChild(del, lastLocation, new System.Windows.Point(-13, 6));
                currentLine.Add(del);
                dicLines.Add(_tag, currentLine);

                if (DwType == DrawType.DrawCalLength)
                {
                    double len = GetDistanceFromLocations(locationList);
                    UpdateMovePanel("总长：" + GetDistanceString(len), "", lastLocation);

                    if (EndCalLength != null)
                        EndCalLength(this, locationList, len, _tag);
                }
                else
                {
                    if (EndDrawLine != null)
                        EndDrawLine(this, locationList, -1, _tag);
                }
            }
            if ((DwType == DrawType.DrawPolygon || DwType == DrawType.DrawCalArea) && locationList.Any() && (poly != null))
            {
                if (poly.Locations.Count > locationList.Count)
                    poly.Locations.RemoveAt(poly.Locations.Count - 1);

                Location lastLocation = locationList.Last();
                var del = CreateDeleteButton();
                lyr.AddChild(del, lastLocation, new System.Windows.Point(-13, 6));
                currentLine.Add(del);
                dicLines.Add(_tag, currentLine);

                if (DwType == DrawType.DrawCalArea)
                {
                    double area = GetAreaFromLocations(locationList);
                    UpdateMovePanel("面积：" + GetAreaString(area), "", lastLocation);

                    if (EndCalArea != null)
                        EndCalArea(this, locationList, area, _tag);
                }
                else
                {
                    UpdateMovePanel(null, "", lastLocation);
                    if (EndDrawPolygon != null)
                    {
                        EndDrawPolygon(this, locationList, -1, _tag);
                    }
                }
            }
            DwType = DrawType.None;
        }

        #endregion

        #region 开始画图形

        public void BeginDraw(DrawType drawType, object tag)
        {
            if (DwType != DrawType.None) return;
            if (tag == null) return;

            _tag = tag;
            DwType = drawType;
            locationList.Clear();

            movePanel = CreatePanel(null);
            lyr.AddChild(movePanel, new Location(0, 0));
            currentLine = new List<UIElement>();
            currentLine.Add(movePanel);
        }

        public void BeginDrawLine(object tag)
        {
            BeginDraw(DrawType.DrawLine, tag);
        }

        public void BeginDrawCalcLen(object tag)
        {
            BeginDraw(DrawType.DrawCalLength, tag);
        }

        public void BeginDrawCustomPoint(string msg, string tip, object tag)
        {
            BeginDraw(DrawType.DrawCustomPoint, tag);
            UpdateMovePanel(msg, tip, new Location(0, 0));
        }

        public void BeginDrawPolygon(string msg, string tip, object tag)
        {
            BeginDraw(DrawType.DrawPolygon, tag);
            UpdateMovePanel(msg, tip, new Location(0, 0));
        }

        public void BeginDrawCalcArea(object tag)
        {
            BeginDraw(DrawType.DrawCalArea, tag);
        }

        public void BeginDrawRect(object tag)
        {
            BeginDraw(DrawType.DrawRect, tag);
        }

        public void DrawPolygon(LocationCollection locations, string msg)
        {
            currentLine = new List<UIElement>();
            Location lastLocation = locations.Last();
            _tag = Common_liby.GetGuidString();

            poly = new MapPolygon();
            poly.Locations = locations;
            poly.StrokeThickness = 2;
            poly.Opacity = 0.5;
            poly.Stroke = new SolidColorBrush(Colors.Red);
            poly.Fill = new SolidColorBrush(Color.FromArgb(200, 253, 128, 68));
            poly.StrokeLineJoin = PenLineJoin.Round;
            lyr.Children.Add(poly);
            currentLine.Add(poly);

            foreach (Location location in locations)
                AddToMapLayer(CreatePoint(), location, PositionOrigin.Center);

            movePanel = CreatePanel(msg, "");
            AddToMapLayer(movePanel, lastLocation);

            var del = CreateDeleteButton();
            lyr.AddChild(del, lastLocation, new System.Windows.Point(-13, 6));
            currentLine.Add(del);
            dicLines.Add(_tag, currentLine);
        }

        #endregion

        public void Clear()
        {
            lyr.Children.Clear();
            DwType = DrawType.None;
            GC.Collect();
        }

        public void RemoveByTag(object tag)
        {
            if (tag != null && dicLines.ContainsKey(tag))
            {
                var list = dicLines[tag];
                foreach (UIElement uiElement in list)
                {
                    lyr.Children.Remove(uiElement);
                }
                dicLines.Remove(tag);
                GC.Collect();
            }
        }

        public event EndDrawHandler EndDrawEvent;
        public event EndDrawHandler EndDrawLine;
        public event EndDrawHandler EndCalLength;
        public event EndDrawHandler EndDrawRect;
        public event EndDrawHandler EndDrawPolygon;
        public event EndDrawHandler EndCalArea;
        public event EndDrawHandler EndDrawCustomPoint;

        #region 相关算法
        public const double EARTH_RADIUS = 6378.137;//地球半径

        private static double rad(double d)
        {
            return d * Math.PI / 180.0;
        }

        public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double radLat1 = rad(lat1);
            double radLat2 = rad(lat2);
            double a = radLat1 - radLat2;
            double b = rad(lng1) - rad(lng2);

            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            return s;
        }

        public static double GetDistanceFromLocations(List<Location> locations)
        {
            double len = 0;
            for (int i = 0; i < locations.Count - 1; i++)
            {
                len += GetDistance(locations[i].Latitude, locations[i].Longitude, locations[i + 1].Latitude, locations[i + 1].Longitude);
            }
            return len * 1000;
        }

        public static string GetDistanceString(double distance)
        {
            string lengthstr = distance > 1000 ? (Math.Round(distance / 1000)).ToString("F2") + "公里" : distance.ToString("F2") + "米";
            return lengthstr;
        }

        public static string GetDistanceStringFromLocations(List<Location> locations)
        {
            double distnce = GetDistanceFromLocations(locations);
            return GetDistanceString(distnce);
        }

        public static double GetAreaFromLocations(List<Location> locations)
        {
            locations.Add(locations[0]);
            int Count = locations.Count;
            if (Count > 3)
            {
                double mtotalArea = 0;
                if ((locations[0].Longitude != locations[Count - 1].Longitude) || (locations[0].Latitude != locations[Count - 1].Latitude))
                {
                    return 0;
                }
                //一个顶角的三个点
                var LowX = 0.0;
                var LowY = 0.0;
                var MiddleX = 0.0;
                var MiddleY = 0.0;
                var HighX = 0.0;
                var HighY = 0.0;

                var AM = 0.0;
                var BM = 0.0;
                var CM = 0.0;

                var AL = 0.0;
                var BL = 0.0;
                var CL = 0.0;

                var AH = 0.0;
                var BH = 0.0;
                var CH = 0.0;

                var CoefficientL = 0.0;
                var CoefficientH = 0.0;

                var ALtangent = 0.0;
                var BLtangent = 0.0;
                var CLtangent = 0.0;

                var AHtangent = 0.0;
                var BHtangent = 0.0;
                var CHtangent = 0.0;

                var ANormalLine = 0.0;
                var BNormalLine = 0.0;
                var CNormalLine = 0.0;
                var OrientationValue = 0.0;
                var AngleCos = 0.0;

                var Sum1 = 0.0;
                var Sum2 = 0.0;
                var Count2 = 0;
                var Count1 = 0;


                var Sum = 0.0;
                var Radius = 6370000;
                //求每个顶点的顶角的度数，并相加
                for (int i = 0; i < Count - 1; i++)
                {
                    if (i == 0)
                    {
                        LowX = locations[Count - 2].Latitude * Math.PI / 180;
                        LowY = locations[Count - 2].Longitude * Math.PI / 180;
                        MiddleX = locations[0].Latitude * Math.PI / 180;
                        MiddleY = locations[0].Longitude * Math.PI / 180;
                        HighX = locations[1].Latitude * Math.PI / 180;
                        HighY = locations[1].Longitude * Math.PI / 180;
                    }
                    else
                    {
                        LowX = locations[i - 1].Latitude * Math.PI / 180;
                        LowY = locations[i - 1].Longitude * Math.PI / 180;
                        MiddleX = locations[i].Latitude * Math.PI / 180;
                        MiddleY = locations[i].Longitude * Math.PI / 180;
                        HighX = locations[i + 1].Latitude * Math.PI / 180;
                        HighY = locations[i + 1].Longitude * Math.PI / 180;
                    }

                    AM = Math.Cos(MiddleY) * Math.Cos(MiddleX);
                    BM = Math.Cos(MiddleY) * Math.Sin(MiddleX);
                    CM = Math.Sin(MiddleY);
                    AL = Math.Cos(LowY) * Math.Cos(LowX);
                    BL = Math.Cos(LowY) * Math.Sin(LowX);
                    CL = Math.Sin(LowY);
                    AH = Math.Cos(HighY) * Math.Cos(HighX);
                    BH = Math.Cos(HighY) * Math.Sin(HighX);
                    CH = Math.Sin(HighY);


                    CoefficientL = (AM * AM + BM * BM + CM * CM) / (AM * AL + BM * BL + CM * CL);
                    CoefficientH = (AM * AM + BM * BM + CM * CM) / (AM * AH + BM * BH + CM * CH);

                    ALtangent = CoefficientL * AL - AM;
                    BLtangent = CoefficientL * BL - BM;
                    CLtangent = CoefficientL * CL - CM;
                    AHtangent = CoefficientH * AH - AM;
                    BHtangent = CoefficientH * BH - BM;
                    CHtangent = CoefficientH * CH - CM;

                    //求角的cos值
                    AngleCos = (AHtangent * ALtangent + BHtangent * BLtangent + CHtangent * CLtangent) / (Math.Sqrt(AHtangent * AHtangent + BHtangent * BHtangent + CHtangent * CHtangent) * Math.Sqrt(ALtangent * ALtangent + BLtangent * BLtangent + CLtangent * CLtangent));
                    //求角度
                    AngleCos = Math.Acos(AngleCos);

                    ANormalLine = BHtangent * CLtangent - CHtangent * BLtangent;
                    BNormalLine = 0 - (AHtangent * CLtangent - CHtangent * ALtangent);
                    CNormalLine = AHtangent * BLtangent - BHtangent * ALtangent;

                    if (AM != 0)
                        OrientationValue = ANormalLine / AM;
                    else if (BM != 0)
                        OrientationValue = BNormalLine / BM;
                    else
                        OrientationValue = CNormalLine / CM;

                    if (OrientationValue > 0)
                    {
                        Sum1 += AngleCos;
                        Count1++;

                    }
                    else
                    {
                        Sum2 += AngleCos;
                        Count2++;
                    }

                }
                if (Sum1 > Sum2)
                {
                    Sum = Sum1 + (2 * Math.PI * Count2 - Sum2);
                }
                else
                {
                    Sum = (2 * Math.PI * Count1 - Sum1) + Sum2;
                }
                //球面面积计算公式：R^2*(S-(n-2)*Pi)
                mtotalArea = (Sum - (Count - 3) * Math.PI) * Radius * Radius;
                return mtotalArea;
            }
            return 0;
        }

        public static string GetAreaString(double area)
        {
            if (area > 1000000)
                return (area / 1000000).ToString("F2") + "平方公里";
            else
                return area.ToString("F2") + "平方米";
        }

        public static string GetAreaStringFromLocations(List<Location> locations)
        {
            double area = GetAreaFromLocations(locations);
            return GetAreaString(area);
        }
        #endregion
    }
}
