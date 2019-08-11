using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Messaging;
using CommLiby;
using CommonLibSL.Model;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;
using Microsoft.Maps.MapControl.Navigation;
using Models;

namespace CommonLibSL.Map
{
    public partial class SLMapControl : UserControl
    {
        /// <summary>
        /// 地图范围发生改变事件
        /// </summary>
        public event Action<SLMapControl> MapViewChangeEnd;
        /// <summary>
        /// 激活的车辆发生改变
        /// </summary>
        public event Action<SLCar> ActiveCarChanged;

        public SLMapControl()
        {
            InitializeComponent();
            InitMap();
            mapLayerCar.ActiveMapCarChanged += MapLayerCar_ActiveMapCarChanged;
            mapLayerCar.MapCarLocationChanged += MapCar_MapCarLocationChanged;
            mapLayerCarLine.MapAdjustChanged += MapLayerCarLine_MapAdjustChanged;
            mapLayerCustomPoint.MapAdjustChanged += MapLayerCustomPoint_MapAdjustChanged;
            mapLayerDraw.MapAdjustChanged += MapLayerDraw_MapAdjustChanged;
        }

        private void MapLayerCarLine_MapAdjustChanged(object sender, EventArgs e)
        {
            ClearLineInMap();
        }
        private void MapLayerCustomPoint_MapAdjustChanged(object sender, EventArgs e)
        {
            ClearMapPoints();
        }
        private void MapLayerDraw_MapAdjustChanged(object sender, EventArgs e)
        {
            MapDrawTool.Clear();
        }
        /// <summary>
        /// 手动改变了地图范围
        /// </summary>
        public bool UserChangedMapView { get; set; }
        /// <summary>
        /// 是否显示车辆轨迹
        /// </summary>
        public bool ShowCarLine { get; set; }
        /// <summary>
        /// 是否显示车牌号
        /// </summary>
        public bool ShowCarLicense
        {
            get { return CarLayer.IsShowCarLicense; }
            set { CarLayer.IsShowCarLicense = value; }
        }
        /// <summary>
        /// 车辆路线图层
        /// </summary>
        public MapLayer CarLineLayer { get { return mapLayerCarLine; } }
        /// <summary>
        /// 兴趣点图层
        /// </summary>
        public MapLayer CustomPointLayer { get { return mapLayerCustomPoint; } }
        /// <summary>
        /// 图形图层
        /// </summary>
        public MapLayer DrawLayer { get { return mapLayerDraw; } }
        /// <summary>
        /// 车辆图层
        /// </summary>
        public MapLayer CarLayer { get { return mapLayerCar; } }
        /// <summary>
        /// 当前地图坐标校准模式
        /// </summary>
        public MapAdjust CurrentMapAdjust => ((CustomMode)map.Mode).MapAdjust;

        /// <summary>
        /// 单辆车模式
        /// </summary>
        public bool OneCarMode { get; set; }
        /// <summary>
        /// 公务车模式
        /// </summary>
        public bool gwcMode { get; set; }

        #region 地图相关设置

        private readonly Dictionary<MapModes, CustomMode> _mapModeDic = new Dictionary<MapModes, CustomMode>();
        public CustomMode GetMapMode(MapModes mode = MapModes.AMapMode)
        {
            if (_mapModeDic.ContainsKey(mode))
            {
                return _mapModeDic[mode];
            }
            else
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                CustomMode mmode = assembly.CreateInstance("CommonLibSL.Map." + mode) as CustomMode;
                if (mmode != null)
                {
                    _mapModeDic.Add(mode, mmode);
                }
                return mmode;
            }
        }

        private bool _mouseLeftDown = false;
        /// <summary>
        /// 初始化地图
        /// </summary>
        void InitMap()
        {
            map.CopyrightVisibility = Visibility.Collapsed;
            map.LogoVisibility = Visibility.Collapsed;
            map.MapForeground.TemplateApplied += delegate (object sender, EventArgs args)
            {
                map.MapForeground.NavigationBar.TemplateApplied += delegate (object obj, EventArgs e)
                {
                    //得到地图的导航条
                    NavigationBar navBar = map.MapForeground.NavigationBar;
                    navBar.HorizontalPanel.Children.Clear();

                    #region 纵向导航条设置
                    UIElementCollection children = map.MapForeground.NavigationBar.VerticalPanel.Children;
                    RepeatButton rb = (children[2] as RepeatButton);
                    if (rb != null)
                        ToolTipService.SetToolTip(rb, "放大");
                    rb = (children[4] as RepeatButton);
                    if (rb != null)
                        ToolTipService.SetToolTip(rb, "缩小");

                    //分割线
                    navBar.VerticalPanel.Children.Add(new CommandSeparator());
                    //使用自定义的MoveMapCommand命令添加移动按钮到工具条
                    CommandButton btnMoveLeft = new CommandButton(new MoveMapCommand(MoveDirection.左), "左移", "左移地图");
                    navBar.VerticalPanel.Children.Add(btnMoveLeft);
                    CommandButton btnMoveRight = new CommandButton(new MoveMapCommand(MoveDirection.右), "右移", "右移地图");
                    navBar.VerticalPanel.Children.Add(btnMoveRight);
                    CommandButton btnMoveUp = new CommandButton(new MoveMapCommand(MoveDirection.上), "上移", "上移地图");
                    navBar.VerticalPanel.Children.Add(btnMoveUp);
                    CommandButton btnMoveDown = new CommandButton(new MoveMapCommand(MoveDirection.下), "下移", "下移地图");
                    navBar.VerticalPanel.Children.Add(btnMoveDown);

                    //分割线
                    navBar.VerticalPanel.Children.Add(new CommandSeparator());
                    //使用ZoomMapCommand命令添加缩放按钮到工具条
                    CommandButton btnZoomIn = new CommandButton(new ZoomMapCommand(true), "放大", "放大地图");
                    navBar.VerticalPanel.Children.Add(btnZoomIn);
                    CommandButton btnZoomOut = new CommandButton(new ZoomMapCommand(false), "缩小", "缩小地图");
                    navBar.VerticalPanel.Children.Add(btnZoomOut);
                    #endregion

                    #region 横向导航条设置
                    foreach (MapModes value in Enum.GetValues(typeof(MapModes)))
                    {
                        string desc = DescriptionAttribute.GetText(value);
                        //使用ChangeMapModeButton添加地图模式按钮到工具条
                        ChangeMapModeButton btn = new ChangeMapModeButton(GetMapMode(value), desc, desc + "模式");
                        btn.Tag = value;
                        btn.Click += Btn_Click;
                        navBar.HorizontalPanel.Children.Add(btn);
                    }
                    #endregion
                };
            };
            CustomMode defaultMode = GetMapMode();   //默认地图模式
            SetMapMode(defaultMode);

            roadMode.Checked += roadMode_Checked;
            aerialMode.Checked += aerialMode_Checked;
            map.Center = new Location().ChinaLocation();  //默认中国范围

            //地图工具初始化
            MapDrawTool.Ini(map, mapLayerDraw);
            map.ViewChangeStart += Map_ViewChangeStart;
            map.ViewChangeEnd += map_ViewChangeEnd;
            map.MouseWheel += (sender, e) =>
            {   //鼠标滚轮一定改变了地图视图
                if (!UserChangedMapView) UserChangedMapView = true;
            };
            map.MouseMove += (sender, e) =>
            {
                if (_mouseLeftDown)
                    if (!UserChangedMapView)
                        UserChangedMapView = true;
            };
            map.MouseLeftButtonDown += (sender, e) =>
            {
                _mouseLeftDown = true;
            };
            map.MouseLeftButtonUp += (sender, e) =>
            {
                _mouseLeftDown = false;
            };

            map.KeyDown += (sender, e) => { OnKeyDown(e); };
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            ChangeMapModeButton btn = (ChangeMapModeButton)sender;
            MapModes mm = (MapModes)btn.Tag;
            CustomMode cm = GetMapMode(mm);
            cm.IsAerialMode = IsAerialMode;
            SetMapMode(cm);
        }

        void aerialMode_Checked(object sender, RoutedEventArgs e)
        {
            IsAerialMode = true;
        }

        void roadMode_Checked(object sender, RoutedEventArgs e)
        {
            IsAerialMode = false;
        }

        /// <summary>
        /// 设置地图中心
        /// </summary>
        public void SetMapCenter(double la, double lo, double maplevel = -1, bool gcj02 = false)
        {
            Location location = new Location(la, lo);
            SetMapCenter(location, maplevel, gcj02);
        }

        /// <summary>
        /// 设置地图中心
        /// </summary>
        public void SetMapCenter(Location location, double maplevel = -1, bool gcj02 = false)
        {
            if (!gcj02)
                location = GetLocation(location.Longitude, location.Latitude);

            map.Dispatcher.BeginInvoke(new Action(() =>
            {
                map.Center = location;
                if (maplevel > 0)
                    map.ZoomLevel = maplevel;
            }));
        }

        /// <summary>
        /// 活得地图中心 顺序依次 la lo maplevel
        /// </summary>
        /// <returns></returns>
        public string[] GetMapCenter()
        {
            string[] resArray = new string[3];
            resArray[0] = map.Center.Latitude.ToString();
            resArray[1] = map.Center.Longitude.ToString();
            resArray[2] = map.ZoomLevel.ToString();
            return resArray;
        }

        /// <summary>
        /// 设置地图模式
        /// </summary>
        /// <param name="mode"></param>
        public void SetMapMode(MapModes mode)
        {
            CustomMode mmode = GetMapMode(mode);
            SetMapMode(mmode);
        }

        /// <summary>
        /// 设置地图模式
        /// </summary>
        /// <param name="mmode"></param>
        public void SetMapMode(CustomMode mmode)
        {
            if (mmode == null) return;
            if (map.Mode != mmode)
            {
                map.Mode = mmode;             
            }            
            mapLayerCar.MapAdjust = mmode.MapAdjust;
            mapLayerCarLine.MapAdjust = mmode.MapAdjust;
            mapLayerCustomPoint.MapAdjust = mmode.MapAdjust;
            mapLayerDraw.MapAdjust = mmode.MapAdjust;
        }

        /// <summary>
        /// 是否是卫星模式
        /// </summary>
        public bool IsAerialMode
        {
            get { return _isAerialMode; }
            set
            {
                if (value != _isAerialMode)
                {
                    _isAerialMode = value;
                    roadMode.IsChecked = !_isAerialMode;
                    aerialMode.IsChecked = _isAerialMode;
                    CustomMode mode = map.Mode as CustomMode;
                    if (mode != null)
                        mode.IsAerialMode = _isAerialMode;
                }
            }
        }

        /// <summary>
        /// 地图缩放级别
        /// </summary>
        public double ZoomLevel
        {
            get { return map.ZoomLevel; }
            set { map.ZoomLevel = value; }
        }

        #endregion

        #region 车辆地图显示相关
        private bool basy = true;
        /// <summary>
        /// 将车显示到地图
        /// </summary>
        /// <param name="clearLCache">是否清楚位置缓存</param>
        /// <param name="cars">需要显示在地图上的车辆</param>
        public void ShowCarInMap(bool clearLCache = false, params SLCar[] cars)
        {
            if (cars.Length == 0) return;
            basy = true;
            if (clearLCache)
            {
                lock (lastPoints)
                {
                    lastPoints.Clear();
                }
                updateBusy = false;
            }

            bool addNew = false;
            if (OneCarMode)
            {
                if (cars[0].IsInService())  //判断服务费是否到期
                {
                    cars[0].IsInView = true;
                    MapCar mapCar = mapLayerCar.GetMapCar(cars[0], true, gwcMode ? "gwc" : "");
                    mapCar.Add();
                    mapCar.IsActive = true;
                    mapCar.ActiveCarAlwaysInMap = ActiveCarAlwaysInMap;
                    UpdateCarInView(mapCar);
                }
            }
            else
            {
                foreach (SLCar car in cars)
                {
                    if (car.IsInService())  //判断服务费是否到期
                    {
                        car.IsInView = true;
                        MapCar mapCar = mapLayerCar.GetMapCar(car, true, gwcMode ? "gwc" : "");
                        if (mapCar.IsInLayer) continue;

                        mapCar.Add();
                        if (cars.Length == 1)
                            UpdateCarInView(mapCar);
                        else
                        {
                            if (!addNew)
                                addNew = true;
                        }
                    }
                }
            }

            if (addNew)
                UpdateCarInView();

            basy = false;
        }

        /// <summary>
        /// 将车从地图上隐藏
        /// </summary>
        /// <param name="cars"></param>
        public void HideCarInMap(SLCar car)
        {
            if (car == null) return;
            car.IsInView = false;
            MapCar mapCar = mapLayerCar.GetMapCar(car);
            if (mapCar != null)
            {
                mapCar.Remove();
                lock (lastPoints)
                {
                    if (lastPoints.ContainsKey(mapCar))
                        lastPoints.Remove(mapCar);
                }
            }
        }

        /// <summary>
        /// 清除地图上的车辆
        /// </summary>
        public void ClearMapCars(bool clearData)
        {
            if (clearData)
            {
                foreach (MapCar mapCar in mapLayerCar.CreatedMapCars)
                {
                    mapCar.Car.IsInView = false;
                    mapCar.Remove(true);
                }
            }

            mapLayerCar.Children.Clear();
            lock (lastPoints)
            {
                lastPoints.Clear();
            }
            updateBusy = false;
            GC.Collect();
        }

        /// <summary>
        /// 将车辆显示到地图中心
        /// </summary>
        /// <param name="car"></param>
        public void SetCarToMapCenter(SLCar car)
        {
            if (car == null || car.LastGpsData == null) return;

            MapCar mcar = mapLayerCar.GetMapCar(car);
            if (mcar != null)
            {
                NEWTRACK gps = car.LastGpsData;
                if (gps.LOCATE != 1 || !GPSTool.CheckLaLo(gps.LATITUDE, gps.LONGITUDE)) return;
                SetMapCenter(gps.LATITUDE, gps.LONGITUDE);
                mcar.IsShow = true;
                mcar.BringToFront();
            }
        }

        /// <summary>
        /// 添加mapcarinfo到地图上
        /// </summary>
        /// <param name="carInfo">地图车辆信息</param>
        /// <param name="location">位置信息</param>
        public void AddMapCarInfo(MapCarInfo carInfo, Location location, int offsetx = 10, int offsety = 3, bool gcj02 = false)
        {
            if (carInfo == null || location == null) return;

            System.Windows.Point offset = new System.Windows.Point(offsetx, -carInfo.Height + offsety);

            if (!gcj02)
            {
                location = GetLocation(location.Longitude, location.Latitude);
            }
            if (!CarLayer.Children.Contains(carInfo))
                CarLayer.AddChild(carInfo, location, offset);
            else
            {
                MapLayer.SetPosition(carInfo, location);
                //MapLayer.SetPositionOrigin(carInfo,PositionOrigin.BottomLeft);
            }
        }

        /// <summary>
        /// 从地图上移除MapCarInfo
        /// </summary>
        /// <param name="carInfo">地图车辆信息</param>
        public void RemoveCarInfo(MapCarInfo carInfo)
        {
            if (carInfo == null) return;
            if (CarLayer.Children.Contains(carInfo))
            {
                CarLayer.Children.Remove(carInfo);
            }
        }

        #endregion

        #region 车辆激活相关
        /// <summary>
        /// 在地图上激活车辆
        /// </summary>
        public void ActiveCarInMap(SLCar car)
        {
            MapCar mapCar = GetMapCarByCar(car);
            if (mapCar != null)
            {
                mapCar.IsActive = true;
                mapCar.ActiveCarAlwaysInMap = ActiveCarAlwaysInMap;
            }
        }

        /// <summary>
        /// 取消激活车辆
        /// </summary>
        public void CancelActive()
        {
            if (mapLayerCar.ActiveCar != null)
            {
                mapLayerCar.ActiveCar.IsActive = false;
                mapLayerCar.ActiveCar = null;
            }
        }

        private void MapLayerCar_ActiveMapCarChanged(MapCar obj)
        {
            if (ActiveCarChanged != null)
            {
                if (obj != null)
                    ActiveCarChanged(obj.Car);
                else
                    ActiveCarChanged(null);
            }
        }

        /// <summary>
        /// 当前激活的车辆
        /// </summary>
        public SLCar ActivedCar
        {
            get
            {
                if (mapLayerCar.ActiveCar != null)
                {
                    return mapLayerCar.ActiveCar.Car;
                }
                return null;
            }
        }

        public bool ActiveCarAlwaysInMap
        {
            get { return _activeCarAlwaysInMap; }
            set
            {
                {
                    _activeCarAlwaysInMap = value;
                    if (mapLayerCar.ActiveCar != null)
                        mapLayerCar.ActiveCar.ActiveCarAlwaysInMap = _activeCarAlwaysInMap;
                }
            }
        }

        #endregion

        #region 车辆行驶轨迹相关

        private void MapCar_MapCarLocationChanged(MapCar mapCar, Location arg2)
        {
            if (ShowCarLine)
            {
                mapCar.ShowRunLine = null;
            }
            else
            {
                if (mapCar.ShowRunLine == null)
                    mapCar.ShowRunLine = false;
            }

            Common_liby.InvokeMethodOnMainThread(() =>
            {
                if (mapCar.ShowRunLine != false)
                    AddLineToMap(mapCar.Car.CARID, mapCar.Car.LastGpsData);
                UpdateCarInView(mapCar);
            });
        }

        //地图中的线
        private Dictionary<uint, MapPolyline> lineInMapDic = new Dictionary<uint, MapPolyline>();
        //地图中的点
        private Dictionary<uint, List<Image>> pointInMapDic = new Dictionary<uint, List<Image>>();

        bool showAllPoint;
        /// <summary>
        /// 显示所以轨迹点（不处理位置相近点）
        /// </summary>
        public bool ShowAllPoint
        {
            get
            {
                return showAllPoint;
            }
            set
            {
                showAllPoint = value;
                MapCar.ShowAllPoint = value;
            }
        }
        /// <summary>
        /// 向地图中添加车辆路线
        /// </summary>
        public void AddLineToMap(uint id = 0, NEWTRACK gps = null)
        {
            if (id == 0 || gps == null) return;

            if (!GPSTool.CheckLaLo(gps.LATITUDE, gps.LONGITUDE)) return;
            Location location = GetLocation(gps.LONGITUDE, gps.LATITUDE);

            MapPolyline line = null;
            if (lineInMapDic.ContainsKey(id))
            {
                line = lineInMapDic[id];
                Location tmpl = line.Locations.Last();
                if (!ShowAllPoint && !MapCar.OptimizationDisplayLoction(tmpl, location))
                {
                    return;
                }
                line.Locations.Add(location);
            }
            else
            {
                line = new MapPolyline();
                line.Locations = new LocationCollection() { location };
                line.Opacity = 1;
                line.StrokeStartLineCap = PenLineCap.Round;
                line.StrokeLineJoin = PenLineJoin.Round;
                line.StrokeEndLineCap = PenLineCap.Round;
                line.StrokeThickness = 3;
                line.Stroke = new SolidColorBrush(Color.FromArgb(255, 51, 133, 255));

                lineInMapDic.Add(id, line);
                mapLayerCarLine.Children.Add(line);
            }

            string str = $"时间：{gps.GNSSTIME.ToFormatDateTimeStr()}\r\n速度:{gps.SPEED}KM/H\r\n{gps.POSITION}";
            Image img = new Image();
            BitmapImage image = new BitmapImage(new Uri("/ImageResource;component/Images/map/track_main_road_arrow.png", UriKind.Relative));
            img.Source = image;
            img.Stretch = Stretch.None;
            img.Width = 10;
            img.Height = 10;
            //img.IsHitTestVisible = true;
            ToolTipService.SetToolTip(img, str);
            RotateTransform rt = img.RenderTransform as RotateTransform;
            if (rt == null) rt = new RotateTransform();
            rt.Angle = gps.HEADING;
            rt.CenterX = img.Width / 2.0;
            rt.CenterY = img.Height / 2.0;
            img.RenderTransform = rt;
            img.Tag = gps;

            List<Image> imgs = null;
            if (pointInMapDic.ContainsKey(id))
            {
                imgs = pointInMapDic[id];
                imgs.Add(img);
            }
            else
            {
                imgs = new List<Image>();
                imgs.Add(img);
                pointInMapDic.Add(id, imgs);
            }

            img.Visibility = Visibility.Collapsed;
            if (showRunPoint)
            {
                System.Windows.Point p;
                if (LocationIsInMapView(location, out p))
                {
                    img.Visibility = Visibility.Visible;
                    mapLayerCarLine.AddChild(img, location, PositionOrigin.Center);
                }
            }
        }

        /// <summary>
        /// 清除图中车辆路线
        /// </summary>
        public void ClearLineInMap(bool stopCarLine = false)
        {
            if (stopCarLine)
                ShowCarLine = false;
            lineInMapDic.Clear();
            pointInMapDic.Clear();
            mapLayerCarLine.Children.Clear();
            GC.Collect();
        }

        bool showRunPoint;
        /// <summary>
        /// 显示车辆路线中的点
        /// </summary>
        public bool ShowRunPoint
        {
            get
            {
                return showRunPoint;
            }
            set
            {
                showRunPoint = value;
                UpdateMapPoint();
            }
        }

        /// <summary>
        /// 智能显示地图上的点
        /// </summary>
        private void UpdateMapPoint()
        {
            IEnumerable<Image> imgs;
            if (!showRunPoint)
            {
                foreach (List<Image> item in pointInMapDic.Values)
                {
                    imgs = item.Where(m => m.Visibility == Visibility.Visible);
                    foreach (Image image in imgs)
                    {
                        mapLayerCarLine.Children.Remove(image);
                        image.Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                foreach (var item in pointInMapDic)
                {
                    imgs = item.Value.Where(m => m.Visibility == Visibility.Collapsed);
                    foreach (Image image in imgs)
                    {
                        NEWTRACK gps = image.Tag as NEWTRACK;
                        if (gps != null)
                        {
                            Location location = GetLocation(gps.LONGITUDE, gps.LATITUDE);
                            System.Windows.Point p;
                            if (LocationIsInMapView(location, out p))
                            {
                                image.Visibility = Visibility.Visible;
                                mapLayerCarLine.AddChild(image, location, PositionOrigin.Center);
                            }
                        }
                    }

                    imgs = item.Value.Where(m => m.Visibility == Visibility.Visible);
                    foreach (Image image in imgs)
                    {
                        NEWTRACK gps = image.Tag as NEWTRACK;
                        if (gps != null)
                        {
                            Location location = GetLocation(gps.LONGITUDE, gps.LATITUDE);
                            System.Windows.Point p;
                            if (!LocationIsInMapView(location, out p))
                            {
                                mapLayerCarLine.Children.Remove(image);
                                image.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region 地图工具相关
        private MapDrawTool _mapDrawTool;
        public MapDrawTool MapDrawTool
        {
            get
            {
                if (_mapDrawTool == null)
                {
                    _mapDrawTool = new MapDrawTool();
                    _mapDrawTool.EndDrawEvent += _mapDrawTool_EndDrawEvent;
                    _mapDrawTool.EndCalArea += _mapDrawTool_EndCalArea;
                    _mapDrawTool.EndCalLength += MapDrawTool_EndCalLength;
                    _mapDrawTool.EndDrawCustomPoint += MapDrawTool_EndDrawCustomPoint;
                    _mapDrawTool.EndDrawLine += MapDrawTool_EndDrawLine;
                    _mapDrawTool.EndDrawPolygon += MapDrawTool_EndDrawPolygon;
                    _mapDrawTool.EndDrawRect += MapDrawTool_EndDrawRect;
                }
                return _mapDrawTool;
            }
            set { _mapDrawTool = value; }
        }

        Dictionary<object, EndDrawHandler> dicHandler = new Dictionary<object, EndDrawHandler>();

        /// <summary>
        /// 测量距离
        /// </summary>
        public void CalLength(EndDrawHandler endDrawHandler)
        {
            object tag = CreateMapDrawTag(endDrawHandler);
            MapDrawTool.BeginDrawCalcLen(tag);
        }
        /// <summary>
        /// 清除屏幕上的图形
        /// </summary>
        public void ClearDraw()
        {
            MapDrawTool.Clear();
        }
        /// <summary>
        /// 测量面积
        /// </summary>
        public void CalArea(EndDrawHandler endDrawHandler)
        {
            MapDrawTool.BeginDrawCalcArea(CreateMapDrawTag(endDrawHandler));
        }

        /// <summary>
        /// 画一个点
        /// </summary>
        /// <param name="endDrawHandler"></param>
        public void DrawPoint(string msg, string tip, EndDrawHandler endDrawHandler)
        {
            MapDrawTool.BeginDrawCustomPoint(msg, tip, CreateMapDrawTag(endDrawHandler));
        }

        /// <summary>
        /// 画一个多边形
        /// </summary>
        public void DrawPolygon(string msg, string tip, EndDrawHandler endDrawHandler)
        {
            MapDrawTool.BeginDrawPolygon(msg, tip, CreateMapDrawTag(endDrawHandler));
        }

        /// <summary>
        /// 根据tag删除图形
        /// </summary>
        /// <param name="tag"></param>
        public void RemoveDrawByTag(object tag)
        {
            MapDrawTool.RemoveByTag(tag);
        }

        void _mapDrawTool_EndCalArea(object sender, List<Location> locations, double result, object tag)
        {

        }
        void MapDrawTool_EndCalLength(object sender, List<Location> locations, double result, object tag)
        {

        }
        void MapDrawTool_EndDrawRect(object sender, List<Location> locations, double result, object tag)
        {

        }
        void MapDrawTool_EndDrawPolygon(object sender, List<Location> locations, double result, object tag)
        {

        }
        void MapDrawTool_EndDrawLine(object sender, List<Location> locations, double result, object tag)
        {

        }
        void MapDrawTool_EndDrawCustomPoint(object sender, List<Location> locations, double result, object tag)
        {

        }

        void _mapDrawTool_EndDrawEvent(object sender, List<Location> locations, double result, object tag)
        {
            InvokeEndDrawHandler(this, locations, result, tag);
        }
        void InvokeEndDrawHandler(object sender, List<Location> locations, double result, object tag)
        {
            if (tag == null) return;
            if (dicHandler.ContainsKey(tag))
            {
                if (dicHandler[tag] != null)
                    dicHandler[tag].Invoke(sender, locations, result, tag);
                dicHandler.Remove(tag);
            }
        }

        public object CreateMapDrawTag(EndDrawHandler endDrawHandler = null)
        {
            object tag = Common_liby.GetGuidString();
            dicHandler.Add(tag, endDrawHandler);
            return tag;
        }

        public void DrawPolygonOnMap(string msg, LocationCollection locations)
        {
            if (locations == null || !locations.Any()) return;

            Location lastLocation = locations.Last();
            Common_liby.InvokeMethodOnMainThread(() =>
            {
                MapDrawTool.DrawPolygon(locations, msg);
            });
            SetMapCenter(lastLocation.Latitude, lastLocation.Longitude, 15, true);
        }

        #endregion

        #region 兴趣点/定位点相关

        readonly Dictionary<uint, List<UIElement>> _dicInMapCustomPoints = new Dictionary<uint, List<UIElement>>();
        /// <summary>
        /// 添加兴趣点
        /// </summary>
        public void AddCustomPoint(MAPCUSTOMPOINT point, bool centerOnMap = true)
        {
            if (point == null) return;
            if (point.ID == 0)
            {
                RemoveCustomPoint(point.ID);
            }
            if (_dicInMapCustomPoints.ContainsKey(point.ID)) return;

            string name = point.NAME;
            string imgurl; //= point.Mapcustomtype.IMAGENAME;

            if (string.IsNullOrWhiteSpace(name))
                name = "兴趣点";

            double lo = point.LO;
            double la = point.LA;
            if (point.MEMO == "new")
            {
                GeoLatLng geo = GPSTool.wgs84togcj02(lo, la);
                lo = geo.longitude;
                la = geo.latitude;
            }
            Location location = new Location(la, lo);

            imgurl = "/ImageResource;component/Images/map/point.png";
            Image img = new Image() { Source = new BitmapImage(new Uri(imgurl, UriKind.Relative)), Stretch = Stretch.None };
            mapLayerCustomPoint.AddChild(img, location, PositionOrigin.BottomCenter);

            List<UIElement> pointList = new List<UIElement>();
            Border border = new Border()
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 110, 200, 66)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(1)
            };
            TextBlock lbl = new TextBlock() { Text = name, Foreground = new SolidColorBrush(Colors.White), FontSize = 11, Margin = new Thickness(0) };
            border.Child = lbl;

            Image delimg = new Image() { Source = new BitmapImage(new Uri("/ImageResource;component/Images/map/delPoint.png", UriKind.Relative)), Stretch = Stretch.None, Width = 12, Height = 12 };
            pointList.Add(border);
            pointList.Add(img);
            pointList.Add(delimg);
            _dicInMapCustomPoints.Add(point.ID, pointList);
            delimg.Tag = point.ID;
            delimg.Cursor = Cursors.Hand;
            delimg.MouseLeftButtonDown += (x, y) =>
            {
                uint key = (uint)((Image)x).Tag;
                RemoveCustomPoint(key);
            };
            border.SizeChanged += (object sender, SizeChangedEventArgs e) =>
            {
                MapLayer.SetPositionOffset(border, new System.Windows.Point(-border.ActualWidth / 2, -44));
                MapLayer.SetPositionOffset(delimg, new System.Windows.Point(border.ActualWidth / 2, -38));
            };
            mapLayerCustomPoint.AddChild(border, location);
            mapLayerCustomPoint.AddChild(delimg, location);

            if (centerOnMap)
                map.Center = location;
        }
        /// <summary>
        /// 移除兴趣点
        /// </summary>
        /// <param name="id"></param>
        public void RemoveCustomPoint(uint id)
        {
            uint key = id;
            if (_dicInMapCustomPoints.ContainsKey(key))
            {
                List<UIElement> list = _dicInMapCustomPoints[key];
                Common_liby.InvokeMethodOnMainThread(() =>
                {
                    list.ForEach(z => mapLayerCustomPoint.Children.Remove(z));
                });
                _dicInMapCustomPoints.Remove(key);
                GC.Collect();
            }
        }

        private readonly Dictionary<string, UIElement[]> _locationList = new Dictionary<string, UIElement[]>();

        /// <summary>
        /// 添加定位点(WGS84)
        /// </summary>
        public string AddLocationPoint(double la, double lo, string name = null, string imgurl = "/ImageResource;component/Images/map/locationPoint.png", bool addDelBtn = true, bool forceAdd = false)
        {
            Location location = new Location(la, lo);
            return AddLocationPoint(location, name, imgurl, addDelBtn, null, null, null, forceAdd);
        }

        /// <summary>
        /// 添加定位点(WGS84)
        /// </summary>
        public string AddLocationPoint(Location location, string name = null, string imgurl = "/Images/map/locationPoint.png", bool addDelBtn = true, Action<object> clickAction = null, object tag = null, object tooltip = null, bool forceAdd = false)
        {
            if (location == null || imgurl == null) return null;

            string key = null;
            Common_liby.InvokeMethodOnMainThread(() =>
            {
                Image img = new Image() { Source = new BitmapImage(new Uri(imgurl, UriKind.Relative)), Stretch = Stretch.None };
                key = AddLocationPoint(location, name, img, PositionOrigin.BottomCenter, addDelBtn, clickAction, tag,
                    tooltip, forceAdd);
            }, false);
            return key;
        }

        /// <summary>
        /// 添加定位点(WGS84)
        /// </summary>
        public string AddLocationPoint(double la, double lo, Image img, PositionOrigin origin)
        {
            Location location = new Location(la, lo);
            return AddLocationPoint(location, "", img, origin, false, null, null, null, true);
        }


        /// <summary>
        /// 添加定位点(WGS84)
        /// </summary>
        public string AddLocationPoint(Location location, string name,
            Image img, PositionOrigin origin, bool addDelBtn = false, Action<object> clickAction = null,
            object tag = null, object tooltip = null, bool forceAdd = false)
        {
            if (location == null || img == null) return null;

            GeoLatLng trueGeo = GPSTool.wgs84togcj02(location.Longitude, location.Latitude);
            Location trueLocation = new Location(trueGeo.latitude, trueGeo.longitude);
            string locationkey = location.Latitude * 1000000 + "_" + location.Longitude * 1000000;
            if (forceAdd)
                locationkey += "_" + DateTime.Now.Ticks;
            if (_locationList.ContainsKey(locationkey))
            {
                map.Dispatcher.BeginInvoke(new Action(() =>
                {
                    map.Center = trueLocation;
                }));
                return locationkey; //相同点阻止添加
            }

            if (name == null)
                name = "经度:" + location.Longitude + "，纬度:" + location.Latitude;

            Common_liby.InvokeMethodOnMainThread(() =>
            {
                UIElement[] pointList = new UIElement[3];
                img.Tag = tag;
                if (clickAction != null)
                {
                    img.MouseLeftButtonDown += (sender, args) =>
                    {
                        clickAction(img.Tag);
                    };
                }
                if (tooltip != null)
                    ToolTipService.SetToolTip(img, tooltip);

                pointList[0] = img;
                mapLayerCustomPoint.AddChild(img, trueLocation, origin);

                Border border = null;
                Image delimg = null;
                if (!string.IsNullOrEmpty(name))
                {
                    TextBlock lbl = new TextBlock() { Text = name, Foreground = new SolidColorBrush(Colors.White), FontSize = 11, Margin = new Thickness(0) };
                    border = new Border()
                    {
                        Background = new SolidColorBrush(Color.FromArgb(255, 245, 67, 54)),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(3),
                        Padding = new Thickness(1),
                        Child = lbl
                    };
                    border.SizeChanged += (object sender, SizeChangedEventArgs e) =>
                    {
                        MapLayer.SetPositionOffset(border, new System.Windows.Point((-border.ActualWidth / 2), -44));
                        if (delimg != null)
                            MapLayer.SetPositionOffset(delimg, new System.Windows.Point(border.ActualWidth / 2, -38));
                    };
                    if (tooltip != null)
                        ToolTipService.SetToolTip(border, tooltip);
                    pointList[1] = border;
                    mapLayerCustomPoint.AddChild(border, trueLocation);
                }

                if (addDelBtn && border != null)
                {
                    delimg = new Image() { Source = new BitmapImage(new Uri("/ImageResource;component/Images/map/delLine.png", UriKind.Relative)), Stretch = Stretch.None, Width = 12, Height = 12 };
                    delimg.Cursor = Cursors.Hand;
                    delimg.Tag = locationkey;
                    delimg.MouseLeftButtonDown += (x, y) =>
                    {
                        string key = (string)((Image)x).Tag;
                        if (_locationList.ContainsKey(key))
                        {
                            var list = _locationList[key];
                            foreach (UIElement element in list)
                            {
                                mapLayerCustomPoint.Children.Remove(element);
                            }
                            _locationList.Remove(key);
                        }
                    };
                    pointList[2] = delimg;
                    mapLayerCustomPoint.AddChild(delimg, trueLocation);
                }
                _locationList.Add(locationkey, pointList);
                map.Center = trueLocation;
            }, false);
            return locationkey;
        }

        /// <summary>
        /// 移动定位点
        /// </summary>
        /// <param name="key">定位点key</param>
        /// <param name="location">新的位置(WSG84)</param>
        /// <param name="tooltip">提示文字</param>
        /// <returns></returns>
        public bool UpdateLocationPoint(string key, Location location, object tooltip = null, object tag = null)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            if (location == null) return false;
            if (!GPSTool.CheckLaLo(location.Latitude, location.Longitude)) return false;
            if (!_locationList.ContainsKey(key)) return false;

            UIElement[] eles = _locationList[key];
            Image img = (Image)eles[0];
            UIElement border = eles[1];
            UIElement delimg = eles[2];
            if (tag != null)
                img.Tag = tag;

            GeoLatLng trueGeo = GPSTool.wgs84togcj02(location.Longitude, location.Latitude);
            Location trueLocation = new Location(trueGeo.latitude, trueGeo.longitude);

            img.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (tooltip != null)
                    ToolTipService.SetToolTip(img, tooltip);
                MapLayer.SetPosition(img, trueLocation);
                if (border != null)
                {
                    if (tooltip != null)
                        ToolTipService.SetToolTip(border, tooltip);
                    MapLayer.SetPosition(border, trueLocation);
                }
                if (delimg != null)
                    MapLayer.SetPosition(delimg, trueLocation);
            }));
            return true;
        }

        /// <summary>
        /// 移动定位点
        /// </summary>
        /// <param name="key">定位点key</param>
        /// <param name="la">新位置纬度</param>
        /// <param name="lo">新位置经度</param>
        /// <param name="tooltip">提示文字</param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool UpdateLocationPoint(string key, double la, double lo, object tooltip = null, object tag = null)
        {
            Location location = new Location(la, lo);
            return UpdateLocationPoint(key, location, tooltip, tag);
        }

        /// <summary>
        /// 移除定位点
        /// </summary>
        /// <param name="key">定位点Key</param>
        public void RemoveLocationPoint(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (!_locationList.ContainsKey(key)) return;
            UIElement[] eles = _locationList[key];
            for (int i = 0; i < eles.Length; i++)
            {
                UIElement ele = eles[i];
                Common_liby.InvokeMethodOnMainThread(() =>
                {
                    mapLayerCustomPoint.Children.Remove(ele);
                });
            }
            _locationList.Remove(key);
            GC.Collect();
        }

        /// <summary>
        /// 清楚地图上所有的点
        /// </summary>
        public void ClearMapPoints()
        {
            _locationList.Clear();
            _dicInMapCustomPoints.Clear();
            mapLayerCustomPoint.Children.Clear();
            GC.Collect();
        }
        #endregion

        #region 优化车辆显示相关

        /// <summary>
        /// 经纬度是否在地图中
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool IsInMapView(Location location, bool gcj02 = false)
        {
            System.Windows.Point point = default(System.Windows.Point);
            if (!gcj02)
                location = GetLocation(location.Longitude, location.Latitude);

            bool suc = map.TryLocationToViewportPoint(location, out point);
            if (suc)
            {
                Size size = map.ViewportSize;
                if (point.X > 0 && point.Y > 0 && point.X < size.Width && point.Y < size.Height)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 车辆是否在可见视野中
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        bool CarIsInView(MapCar mapCar, out System.Windows.Point point)
        {
            point = default(System.Windows.Point);
            if (mapCar == null || mapCar._location == null) return false;
            return LocationIsInMapView(mapCar._location, out point);
        }

        private bool stopUpdateView = false;    //停止更新车辆地图
        private bool updateBusy = false; //更新正忙
        private bool _isAerialMode;
        Dictionary<MapCar, System.Windows.Point> lastPoints = new Dictionary<MapCar, System.Windows.Point>();
        private bool _activeCarAlwaysInMap = true;

        /// <summary>
        /// 更新车辆在地图中的位置显示
        /// </summary>
        /// <param name="mcar"></param>
        void UpdateCarInView(MapCar mcar = null)
        {
            if (stopUpdateView) return;

            if (updateBusy) return;
            updateBusy = true;

            lock (lastPoints)
            {
                bool mutiupdate = mcar == null;
                System.Windows.Point point;
                if (mutiupdate)
                {
                    lastPoints.Clear();
                    foreach (MapCar _mcar in mapLayerCar.CreatedMapCars)
                    {
                        if (!_mcar.IsInLayer) continue; //不在图层中的不处理

                        if (CarIsInView(_mcar, out point))
                        {
                            if (map.ZoomLevel > 8) //大于8级全部显示
                            {
                                _mcar.IsShow = true;
                            }
                            else if (lastPoints.Values.Any(p => Common_liby.CalTowPointLength(new CommLiby.Point(p.X, p.Y), new CommLiby.Point(point.X, point.Y)) <= 8)) //小于8级批量 距离小的不更新
                            {
                                if (!_mcar.IsActive)
                                    _mcar.IsShow = false;
                            }
                            else
                            {
                                lastPoints.Add(_mcar, point);
                                _mcar.IsShow = true;
                            }
                        }
                        else
                        {
                            if (!_mcar.IsActive)
                                _mcar.IsShow = false;
                        }
                    }
                }
                else
                {
                    if (mcar.IsInLayer)
                    {
                        //单个更新 只判断是否在地图中
                        if (!CarIsInView(mcar, out point))
                        {
                            if (!mcar.IsActive)
                                if (mcar.IsShow)
                                    mcar.IsShow = false;
                        }
                        else
                        {
                            if (lastPoints.ContainsKey(mcar))
                                lastPoints.Remove(mcar);

                            if (map.ZoomLevel > 8) //大于8级全部显示
                            {
                                mcar.IsShow = true;
                            }
                            else if (lastPoints.Values.Any(p => Common_liby.CalTowPointLength(new CommLiby.Point(p.X, p.Y), new CommLiby.Point(point.X, point.Y)) <= 8))
                            //小于8级批量 距离小的不更新
                            {
                                if (!mcar.IsActive)
                                    mcar.IsShow = false;
                            }
                            else
                            {
                                lastPoints.Add(mcar, point);
                                mcar.IsShow = true;
                            }
                        }
                    }
                }
            }

            updateBusy = false;
        }

        private void Map_ViewChangeStart(object sender, MapEventArgs e)
        {
            stopUpdateView = true;
        }

        /// <summary>
        /// 地图视图已经发生改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void map_ViewChangeEnd(object sender, MapEventArgs e)
        {
            stopUpdateView = false;
            MapViewChangeEnd?.Invoke(this);
            UpdateCarInView();
            UpdateMapPoint();
        }

        #endregion

        #region 公用方法
        /// <summary>
        /// 根据车辆找到地图中车辆对象
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        public MapCar GetMapCarByCar(SLCar car)
        {
            if (car == null) return null;
            MapCar mapCar = CarLayer.GetMapCar(car);
            return mapCar;
        }

        /// <summary>
        /// 计算当前地图模式坐标
        /// </summary>
        /// <returns></returns>
        public Location GetLocation(double lo, double la, short offx, short offy)
        {
            return GetLocation(lo, la);
        }
        /// <summary>
        /// 将wgs84坐标转换为当前地图坐标
        /// </summary>
        /// <param name="lo">经度</param>
        /// <param name="la">纬度</param>
        /// <returns></returns>
        public Location GetLocation(double lo, double la)
        {
            Location location = CurrentMapAdjust.GetLoLa(lo, la);
            return location;
        }

        /// <summary>
        /// 判断坐标是否在可见视图中
        /// </summary>
        /// <param name="location"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        bool LocationIsInMapView(Location location, out System.Windows.Point point)
        {
            point = default(System.Windows.Point);
            if (location == null) return false;

            bool suc = map.TryLocationToViewportPoint(location, out point);
            if (suc)
            {
                Size size = map.ViewportSize;
                if (point.X > 0 && point.Y > 0 && point.X < size.Width && point.Y < size.Height)
                    return true;
            }
            return false;
        }
        #endregion
    }

    #region 地图菜单设置
    /// <summary>
    /// 地图移动方向
    /// </summary>
    public enum MoveDirection
    { 上, 下, 左, 右 }

    /// <summary>
    /// 工具条命令：移动地图
    /// </summary>
    class MoveMapCommand : NavigationBarCommandBase
    {
        private MoveDirection direction;

        public MoveMapCommand(MoveDirection direction)
        {
            this.direction = direction;
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="map">被执行该命令的工具条控制的地图</param>
        public override void Execute(MapBase map)
        {
            //定义移动位移量，初始均为0
            int deltaX = 0;
            int deltaY = 0;

            //根据移动方向向移动位移赋值
            switch (direction)
            {
                case MoveDirection.上:
                    deltaY = -50;
                    break;
                case MoveDirection.下:
                    deltaY = 50;
                    break;
                case MoveDirection.左:
                    deltaX = -50;
                    break;
                case MoveDirection.右:
                    deltaX = 50;
                    break;
                default:
                    break;
            }

            //首先获取当前视图的坐标
            System.Windows.Point viewportPoint;
            if (map.TryLocationToViewportPoint(map.Center, out viewportPoint))
            {
                //将坐标与移动位移量进行相加，对地图视野中心进行调整
                viewportPoint.X += deltaX;
                viewportPoint.Y += deltaY;

                //将调整后的坐标再设置回地图
                Location newCenter;
                if (map.TryViewportPointToLocation(viewportPoint, out newCenter))
                {
                    //前台控件关掉了动画效果，这里再使用时再加上，用完后还原
                    AnimationLevel al = map.AnimationLevel;
                    map.AnimationLevel = AnimationLevel.Full;
                    map.Center = newCenter;
                    map.AnimationLevel = al;
                }
            }
        }
    }
    #endregion
}
