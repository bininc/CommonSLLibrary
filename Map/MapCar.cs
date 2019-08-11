using CommLiby;
using CommonLibSL.Model;
using Microsoft.Maps.MapControl;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommLiby.Cyhk;
using Models;

namespace CommonLibSL.Map
{
    public partial class MapCar : ContentControl
    {
        public event Action<MapCar, Location> LocationChanged;
        //图片资源
        private static readonly BitmapImage ImageRun = new BitmapImage(new Uri("/ImageResource;component/Images/car/run.png", UriKind.Relative));
        private static readonly BitmapImage ImageStop = new BitmapImage(new Uri("/ImageResource;component/Images/car/stop.png", UriKind.Relative));
        private static readonly BitmapImage ImageOff = new BitmapImage(new Uri("/ImageResource;component/Images/car/off.png", UriKind.Relative));
        private static readonly BitmapImage ImageNoGps = new BitmapImage(new Uri("/ImageResource;component/Images/car/nogps.png", UriKind.Relative));
        private static readonly BitmapImage ImageAlarm = new BitmapImage(new Uri("/ImageResource;component/Images/car/alarm.png", UriKind.Relative));

        private static readonly BitmapImage ImageOilWellOn = new BitmapImage(new Uri("/ImageResource;component/Images/car/oil_spot_on31.png", UriKind.Relative));
        private static readonly BitmapImage ImageOilWellOff = new BitmapImage(new Uri("/ImageResource;component/Images/car/oil_spot_off31.png", UriKind.Relative));
        private static readonly BitmapImage ImageOilWellAlarm = new BitmapImage(new Uri("/ImageResource;component/Images/car/oil_spot_alarm31.png", UriKind.Relative));

        //private static readonly BitmapImage ImageBg = new BitmapImage(new Uri("/ImageResource;component/Images/car/duihuakang_bg1.png", UriKind.Relative));
        private readonly Image _imgCar;
        private SLCar _car;
        private bool _gwcMode;
        public SLCar Car
        {
            get { return _car; }
            private set
            {
                _car = value;
                if (_car != null)
                {
                    CarType = _car.IS_POLL_OIL_SPOT == 1 ? CarType.OilWell : CarType.Car;
                    _car.PropertyChanged += _car_PropertyChanged;
                    _lblCarName.Dispatcher.BeginInvoke(() => _lblCarName.Content = _car.LICENSE);
                    CalLocation();
                    UpdateCarState();
                }
            }
        }

        public bool ActiveCarAlwaysInMap { get; set; } = true;
        public bool? ShowRunLine { get; set; } = false;
        public static bool ShowAllPoint { get; set; }
        public Location _location { get; private set; }
        private Location _lastLocation;
        private void CalLocation()
        {
            NEWTRACK gps = _car?.LastGpsData;
            //异常 经纬度过滤
            if (gps == null) return;
            if (gps.LOCATE != 1 || !GPSTool.CheckLaLo(gps.LATITUDE, gps.LONGITUDE)) return;

            if (_mapLayer?.MapAdjust == null)
                return;
            _location = _mapLayer.MapAdjust.GetLoLa(gps.LONGITUDE, gps.LATITUDE, gps.OFFSETX, gps.OFFSETY);
        }

        public static bool OptimizationDisplayLoction(Location lastGps, Location gps)
        {
            if (gps == null) return false;
            if (lastGps == null) return true;
            //if (!MapTools.CheckLaLo(lastGps.Latitude, lastGps.Longitude) || !MapTools.CheckLaLo(gps.Latitude, gps.Longitude)) return false;
            double lal = Math.Round(lastGps.Latitude, 5);
            double lol = Math.Round(lastGps.Longitude, 4);
            double la = Math.Round(gps.Latitude, 5);
            double lo = Math.Round(gps.Longitude, 4);
            if (lal == la && lol == lo)
            {
                //距离相近的点不显示
                return false;
            }

            return true;
        }

        private void _car_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!IsInLayer) return;  //不在图层中不触发事件
            //if(_car.CarStatus.blindarea_status) return; //盲区补偿不更新            

            if (e.PropertyName.Equals(nameof(SLCar.CarState)))  //车辆状态发生改变
            {
                UpdateCarState();
            }
            if (e.PropertyName.Equals(nameof(SLCar.LastGpsData) + "." + nameof(NEWTRACK)))   //GPS状态改变
            {
                UpdateGridInfo(); //更新文字信息
                CalLocation();
                UpdateHeading();
                UpdateLocation();
                BringToFront(); //防止遮住激活车辆
            }

            if (_gwcMode && e.PropertyName.Equals(nameof(SLCar.STATE)))
            {
                UpdateGridInfo(); //更新文字信息
            }
        }

        public CarType CarType { get; set; }

        internal MapLayer _mapLayer;

        private readonly Border _borderName;
        private readonly Label _lblCarName;

        private MapCarInfo _mapCarInfo;
        private MapCarInfo _gridInfo
        {
            get
            {
                if (_mapCarInfo == null)
                {
                    _mapCarInfo = new MapCarInfo();
                    _mapCarInfo.Visibility = Visibility.Collapsed;
                    _mapCarInfo.Bind(carInfoItems.ToArray());
                    _mapCarInfo.MouseLeftButtonDown += _gridInfo_MouseLeftButtonDown;
                }
                return _mapCarInfo;
            }
        }

        private readonly MapCarInfoBindItems carInfoItems;

        internal MapCar(SLCar car, MapLayer mapLayer, object tag = null)
        {
            Width = 16;
            Height = 32;
            _imgCar = new Image() { Stretch = Stretch.Uniform };
            Content = _imgCar;

            _borderName = new Border();
            _borderName.Background = new SolidColorBrush(Color.FromArgb(0xC8, 0xE5, 0xE6, 0xE8));
            _borderName.CornerRadius = new CornerRadius(0);
            _borderName.Padding = new Thickness(2, 0, 2, 1);
            _borderName.Margin = new Thickness(0, 0, 0, 16);
            _borderName.BorderThickness = new Thickness(1);
            _borderName.BorderBrush = new SolidColorBrush(Colors.Gray);
            _lblCarName = new Label();
            _lblCarName.FontSize = 12;
            _lblCarName.Tag = "0";
            _borderName.MouseLeftButtonDown += _lblCarName_MouseLeftButtonDown;
            _borderName.Child = _lblCarName;


            if (tag != null && tag.ToString() == "gwc")
            {
                _gwcMode = true;
                carInfoItems = new MapCarInfoBindItems(
                        new MapCarInfoBindItem("车牌号", ""),
                        new MapCarInfoBindItem("任务状态", ""),
                        new MapCarInfoBindItem("乘 客", ""),
                        new MapCarInfoBindItem("起始地点", ""),
                        new MapCarInfoBindItem("起始时间", ""),
                        new MapCarInfoBindItem("结束地点", ""),
                        new MapCarInfoBindItem("结束时间", ""),
                        new MapCarInfoBindItem("驾驶员", "")
                    );
            }
            else
            {
                carInfoItems = new MapCarInfoBindItems(
                        new MapCarInfoBindItem("车牌号", ""),
                        new MapCarInfoBindItem("时 间", ""),
                        new MapCarInfoBindItem("速 度", ""),
                        new MapCarInfoBindItem("地 点", ""),
                        new MapCarInfoBindItem("定 位", ""),
                        new MapCarInfoBindItem("状 态", "")
                    );
            }

            IsActive = false;

            _mapLayer = mapLayer;
            _mapLayer.MapAdjustChanged += _mapLayer_MapAdjustChanged;
            Car = car;
        }

        #region 激活及点击相关
        /// <summary>
        /// 显示车辆信息面板
        /// </summary>
        public void OpenInfoPanel()
        {
            if (_location == null) return;
            Location loc = _location;
            if (_lastLocation != null)
                loc = _lastLocation;

            _gridInfo.Visibility = Visibility.Visible;
            UpdateGridInfo();
            if (!_mapLayer.Children.Contains(_gridInfo))
                _mapLayer.AddChild(_gridInfo, loc, new PositionOrigin(-0.03, 0.78));
            BringToFront(short.MaxValue - 1);
        }

        /// <summary>
        /// 关闭车辆信息面板
        /// </summary>
        public void CloseInfoPanel()
        {
            if (!_mapLayer.Children.Contains(_gridInfo)) return;

            _gridInfo.Visibility = Visibility.Collapsed;
            _mapLayer.Children.Remove(_gridInfo);
        }

        void _gridInfo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                CloseInfoPanel();
                e.Handled = false;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                IsActive = true;
                e.Handled = true;
            }
            else
            {
                OpenInfoPanel();
                e.Handled = true;
            }
            base.OnMouseLeftButtonDown(e);
        }

        void _lblCarName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                IsActive = true;
                e.Handled = true;
            }
            else
            {
                OpenInfoPanel();
                e.Handled = true;
            }
        }

        private int _oldBorderZindex;
        private int _oldZindex;
        private int _oldGridZindex;
        private bool _isActive;

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                _lblCarName.Foreground = new SolidColorBrush(_isActive ? Colors.Red : Color.FromArgb(0xff, 0x00, 0x78, 0xd7));
                if (_location == null) return;

                if (_isActive)
                {
                    IsShow = true;

                    if (_mapLayer.ActiveCar != this)
                    {
                        if (_mapLayer.ActiveCar != null)
                            _mapLayer.ActiveCar.IsActive = false;

                        BringToFront();
                        _mapLayer.ParentMap.Center = MapLayer.GetPosition(this);    //激活车辆显示地图中心
                    }
                    _mapLayer.ActiveCar = this;
                    _borderName.Visibility = Visibility.Visible;
                }
                else
                {
                    SendToBack();
                    _borderName.Visibility = Visibility.Collapsed;
                }
            }
        }
        private static int zindex = 1;
        /// <summary>
        /// 将车辆置于最顶端
        /// </summary>
        public void BringToFront(int value = -1)
        {
            if (!_isShow) return;

            Dispatcher.BeginInvoke(() =>
            {
                _oldBorderZindex = Canvas.GetZIndex(_borderName);
                _oldZindex = Canvas.GetZIndex(this);
                if (_mapCarInfo != null)
                    _oldGridZindex = Canvas.GetZIndex(_mapCarInfo);

                if (_isActive || (_mapCarInfo != null && _mapCarInfo.Visibility == Visibility.Visible)) value = short.MaxValue;
                else
                {
                    if (value == -1)
                    {
                        int z = Canvas.GetZIndex(this);
                        if (zindex < z)
                            zindex = z;
                        //if (zindex < value)
                        //    zindex = value;
                        value = ++zindex;

                        if (value >= short.MaxValue)
                            value = 1;

                        zindex = value;
                    }
                    else if (value < 0 || value > short.MaxValue) return;
                }

                Canvas.SetZIndex(_borderName, value);
                Canvas.SetZIndex(this, value);
                if (_mapCarInfo != null)
                    Canvas.SetZIndex(_mapCarInfo, value);
            });
        }
        /// <summary>
        /// 将车辆置于最后端
        /// </summary>
        public void SendToBack(int value = -1)
        {
            if (!_isShow) return;
            if (value == -1)
            {
                Canvas.SetZIndex(_borderName, _oldBorderZindex);
                Canvas.SetZIndex(this, _oldZindex);
                if (_mapCarInfo != null)
                    Canvas.SetZIndex(_mapCarInfo, _oldGridZindex);
            }
            else if (value >= 0 && value < short.MaxValue)
            {
                Canvas.SetZIndex(_borderName, value);
                Canvas.SetZIndex(this, value);
                if (_mapCarInfo != null)
                    Canvas.SetZIndex(_mapCarInfo, value);
            }
        }
        #endregion

        #region 相关状态更新

        private void _mapLayer_MapAdjustChanged(object sender, EventArgs e)
        {   //地图校准变化 刷新自身位置
            CalLocation();  //重新计算位置
            UpdateLocation();
        }

        /// <summary>
        /// 更新方向
        /// </summary>
        void UpdateHeading()
        {
            if (!_isShow) return;
            if (_car.LastGpsData == null || _car.LastGpsData.LOCATE != 1 || !GPSTool.CheckLaLo(_car.LastGpsData.LATITUDE, _car.LastGpsData.LONGITUDE)) return;

            Dispatcher.BeginInvoke(() =>
            {
                RotateTransform rt = RenderTransform as RotateTransform;
                if (rt == null)
                {
                    rt = new RotateTransform();
                    rt.CenterX = Width / 2.0;
                    rt.CenterY = Height / 2.0;
                }
                if (CarType != CarType.OilWell)
                {
                    if (_car.LastGpsData != null)
                        rt.Angle = _car.LastGpsData.HEADING;
                }
                RenderTransform = rt;
            });
        }
        /// <summary>
        /// 更新位置
        /// </summary>
        void UpdateLocation()
        {
            if (_location == null) return;  //不更新无效位置
            if (!ShowAllPoint && !OptimizationDisplayLoction(_lastLocation, _location)) return;
            _lastLocation = _location;

            Dispatcher.BeginInvoke(() =>
            {
                if (_isShow)
                {
                    MapLayer.SetPosition(this, _location);
                    MapLayer.SetPosition(_borderName, _location);
                    if (IsActive && _borderName.Visibility == Visibility.Collapsed)
                        _borderName.Visibility = Visibility.Visible;
                }

                if (_mapCarInfo != null && _mapCarInfo.Visibility == Visibility.Visible)
                {
                    MapLayer.SetPosition(_mapCarInfo, _location);
                }

                if (_isActive)   //激活状态地图中心跟踪移动
                {
                    if (ActiveCarAlwaysInMap /*&& !CarIsInView()*/)
                    {
                        _mapLayer.ParentMap.Center = _location;
                    }
                }
            });

            LocationChanged(this, _location);
        }

        /// <summary>
        /// 更新车辆状态
        /// </summary>
        void UpdateCarState()
        {
            if (!_isShow || _car == null) return;

            _imgCar.Dispatcher.BeginInvoke(() =>
            {
                switch (_car.CarState)
                {
                    case CarState.NoStatus:
                        break;
                    case CarState.OffLine:
                        _imgCar.Source = CarType == CarType.OilWell ? ImageOilWellOff : ImageOff;
                        break;
                    case CarState.NoGps:
                        _imgCar.Source = CarType == CarType.OilWell ? ImageOilWellOn : ImageNoGps;
                        break;
                    case CarState.Stop:
                        _imgCar.Source = CarType == CarType.OilWell ? ImageOilWellOn : ImageStop;
                        break;
                    case CarState.Run:
                        _imgCar.Source = CarType == CarType.OilWell ? ImageOilWellOn : ImageRun;
                        break;
                    case CarState.Alarm:
                        _imgCar.Source = CarType == CarType.OilWell ? ImageOilWellAlarm : ImageAlarm;
                        break;
                }
            });
        }

        /// <summary>
        /// 更新车辆信息
        /// </summary>
        void UpdateGridInfo()
        {
            if (_car?.LastGpsData == null) return;
            Dispatcher.BeginInvoke(() =>
            {
                if (_car?.LastGpsData == null) return;
                _lblCarName.Content = _car.LICENSE;
                if (_mapCarInfo == null || _mapCarInfo.Visibility == Visibility.Collapsed) return;

                if (_gwcMode)
                {
                    carInfoItems["车牌号"].Text = $"{_car.LICENSE} {_car.CAR_TYPE_STR}";
                    carInfoItems["任务状态"].Text = _car.STATE == 1 ? "任务中" : "可调派";
                    carInfoItems["乘 客"].Text = $"{_car.PASSENGER} {_car.PASSENGER_PHONE}";
                    carInfoItems["起始地点"].Text = _car.BEGIN_ADDRESS;
                    carInfoItems["起始时间"].Text = _car.BEGIN_TIME.ToFormatDateTimeStr();
                    carInfoItems["结束地点"].Text = _car.END_ADDRESS;
                    carInfoItems["结束时间"].Text = _car.END_TIME.ToFormatDateTimeStr();
                    carInfoItems["驾驶员"].Text = $"{_car.DRIVER_NAME} {_car.DRIVER_PHONE}";
                }
                else
                {
                    string carLisStr = _car.LICENSE;
                    if (!string.IsNullOrWhiteSpace(_car.Unit?.UNITNAME))
                        carLisStr += $"({_car.Unit.UNITNAME})";
                    carInfoItems["车牌号"].Text = carLisStr;
                    carInfoItems["时 间"].Text = _car.LastGpsData.GNSSTIME.ToFormatDateTimeStr();
                    carInfoItems["速 度"].Text = $"{_car.LastGpsData.SPEED} Km/h 方向: {_car.LastGpsData.HEADING_STR} 里程: {(_car.LastGpsData.OBD_MILEAGE == 0 ? _car.LastGpsData.MILEAGE_K : _car.LastGpsData.OBD_MILEAGE)}";
                    carInfoItems["地 点"].Text = _car.LastGpsData.POSITION?.TrimStart() ?? "";
                    carInfoItems["定 位"].Text = "[" + _car.LastGpsData.LOCATE_STR + "] 模式: " + _car.LastGpsData.LOCATEMODE_STR + " " + _car.LastGpsData.ASSIST_LOCATEMODE_STR;
                    carInfoItems["状 态"].Text = "[" + DescriptionAttribute.GetText(_car.CarState) + "] " + _car.CarStatus;
                }
            });
        }
        #endregion

        #region 车辆显示与隐藏
        private bool _isShow = false;
        /// <summary>
        /// 是否在地图中显示
        /// </summary>
        public bool IsShow
        {
            get { return _isShow; }
            set
            {
                if (value == _isShow) return;

                if (_location == null)
                    CalLocation();

                if (value && !_isShow && _location != null)
                {
                    _mapLayer.AddChild(this, _location, PositionOrigin.Center);
                    _mapLayer.AddCarLicense(_borderName, _location, PositionOrigin.BottomCenter);
                    _isShow = true;
                }
                else if (!value && _isShow)
                {
                    _mapLayer.RemoveCarLicense(_borderName);
                    _mapLayer.Children.Remove(this);
                    if (_mapCarInfo != null && _mapCarInfo.Visibility == Visibility.Visible)
                        _mapLayer.Children.Remove(_mapCarInfo);
                    _isShow = false;
                }

                UpdateCarState();
                UpdateLocation();
                UpdateHeading();
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (!_mapLayer.IsShowCarLicense && !IsActive)    //鼠标放到车图标上自动显示车牌号
                _borderName.Visibility = Visibility.Visible;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!_mapLayer.IsShowCarLicense && !IsActive)    //鼠标离开隐藏车牌号
                _borderName.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 添加车辆在图层
        /// </summary>
        public void Add()
        {
            IsInLayer = true;
        }
        /// <summary>
        /// 车辆是否在图层中
        /// </summary>
        public bool IsInLayer { get; private set; }

        /// <summary>
        /// 移除车辆从图层
        /// </summary>
        public void Remove(bool fast = false)
        {
            IsInLayer = false;

            if (!fast)
            {
                if (_isShow)
                {
                    _mapLayer.RemoveCarLicense(_borderName);
                    _mapLayer.Children.Remove(this);
                }

                if (_mapCarInfo != null && _mapCarInfo.Visibility == Visibility.Visible)
                    _mapLayer.Children.Remove(_mapCarInfo);
            }

            _isShow = false;
        }
        #endregion
    }

    public enum CarType
    {
        Car,
        OilWell
    }
}
