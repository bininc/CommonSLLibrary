using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommLiby;
using CommLiby.Cyhk;
using CommonLibSL.Model;
using Models;

namespace CommonLibSL.Controls
{
    public partial class SLTreeViewItem : TreeViewItem
    {
        #region 选中改变处理
        public static readonly DependencyProperty CheckedProperty = DependencyProperty.Register(
        "Checked", typeof(bool), typeof(SLTreeViewItem), new PropertyMetadata(default(bool), (x, y) =>
        {
            SLTreeViewItem treeViewItem = (SLTreeViewItem)x;

            //选中改变事件
            bool check = (bool)y.NewValue;
            treeViewItem.SlCheckBox.Foreground = new SolidColorBrush(check ? Color.FromArgb(255, 69, 139, 194) : Color.FromArgb(255, 85, 85, 85));
            //treeViewItem.Background = new SolidColorBrush(check ? Colors.White : Colors.Transparent);
            if (check)
            {
                //父级选中子级也选中      
                foreach (SLTreeViewItem item in treeViewItem.Items)
                    item.Checked = true;

                //所有子级选中父级也选中
                if (treeViewItem.ParentTreeViewItem != null && treeViewItem.ParentTreeViewItem.Items.All(a => ((SLTreeViewItem)a).Checked))
                    treeViewItem.ParentTreeViewItem.Checked = true;
            }

            if (!check)
            {
                //父级取消字节也取消
                if (treeViewItem.Tag == null)
                    foreach (SLTreeViewItem item in treeViewItem.Items)
                        item.Checked = false;
                else
                    treeViewItem.Tag = null;

                //所有子取消选中父级选中取消
                if (treeViewItem.ParentTreeViewItem != null && treeViewItem.ParentTreeViewItem.Checked)
                {
                    treeViewItem.ParentTreeViewItem.Tag = false;
                    treeViewItem.ParentTreeViewItem.Checked = false;
                }
            }

            if (!treeViewItem.IsUnitNode)   //车辆节点
            {
                if (CheckedCarChanged != null)    //调用事件
                    CheckedCarChanged(treeViewItem, treeViewItem.DataCar, check);
            }
        }));
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool Checked
        {
            get { return (bool)GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }

        
        protected override void OnSelected(RoutedEventArgs e)
        {
            SlCheckBox.Foreground = new SolidColorBrush(Color.FromArgb(255, 69, 139, 194));
            base.OnSelected(e);
        }

        protected override void OnUnselected(RoutedEventArgs e)
        {
            if(!Checked)
                SlCheckBox.Foreground = new SolidColorBrush(Color.FromArgb(255, 85, 85, 85));
            base.OnUnselected(e);
        }

        #endregion

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(SLTreeViewItem), new PropertyMetadata("SLTreeViewItem"));
        /// <summary>
        /// 名字
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty ExtensionTextProperty = DependencyProperty.Register(
            "ExtensionText", typeof(string), typeof(SLTreeViewItem), new PropertyMetadata(default(string)));
        /// <summary>
        /// 扩展(后缀)文本
        /// </summary>
        public string ExtensionText
        {
            get { return (string)GetValue(ExtensionTextProperty); }
            set { SetValue(ExtensionTextProperty, value); }
        }

        private SLCheckBox _slCheckBox;
        private TextBlock _textBlock;

        private static readonly BitmapImage fuxuankuang = new BitmapImage(new Uri("/ImageResource;component/Images/fuxuankuang.png", UriKind.Relative));
        private static readonly BitmapImage fuxuankuangon = new BitmapImage(new Uri("/ImageResource;component/Images/fuxuankuang_on.png", UriKind.Relative));
        private static readonly BitmapImage caroff = new BitmapImage(new Uri("/ImageResource;component/Images/car/treeOff.png", UriKind.Relative));
        private static readonly BitmapImage carnogps = new BitmapImage(new Uri("/ImageResource;component/Images/car/treeNoGps.png", UriKind.Relative));
        private static readonly BitmapImage carstop = new BitmapImage(new Uri("/ImageResource;component/Images/car/treeStop.png", UriKind.Relative));
        private static readonly BitmapImage caron = new BitmapImage(new Uri("/ImageResource;component/Images/car/treeOn.png", UriKind.Relative));
        private static readonly BitmapImage caralarm = new BitmapImage(new Uri("/ImageResource;component/Images/car/treeAlarm.png", UriKind.Relative));

        private SLCheckBox SlCheckBox
        {
            get
            {
                if (_slCheckBox == null)
                {
                    _slCheckBox = new SLCheckBox();
                    _slCheckBox.TriggerInCheckBox = true;
                    _slCheckBox.Foreground = new SolidColorBrush(Color.FromArgb(255, 85, 85, 85));
                    _slCheckBox.CheckImg = fuxuankuangon;
                    _slCheckBox.UnCheckImg = fuxuankuang;
                    if (BrowseMode)
                        _slCheckBox.Enable = false;

                    _slCheckBox.SetBinding(SLCheckBox.CheckedProperty, new Binding("Checked") { Mode = BindingMode.TwoWay });
                    _slCheckBox.SetBinding(SLCheckBox.TextProperty, new Binding("Text"));
                    _slCheckBox.DataContext = this;
                }
                return _slCheckBox;
            }
        }

        private TextBlock OnlineCount
        {
            get
            {
                if (_textBlock == null)
                {
                    _textBlock = new TextBlock();
                    _textBlock.Margin = new Thickness(2, 0, 0, 0);
                    _textBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 140, 0));
                    _textBlock.SetBinding(TextBlock.TextProperty, new Binding("ExtensionText"));
                    _textBlock.DataContext = this;
                }
                return _textBlock;
            }
        }

        public static readonly DependencyProperty CarStausProperty = DependencyProperty.Register(
            "CarState", typeof(CarState), typeof(SLTreeViewItem), new PropertyMetadata(CarState.NoStatus, (x, y) =>
            {   //状态发生改变
                SLTreeViewItem item = (SLTreeViewItem)x;
                CarState status = (CarState)y.NewValue;
                switch (status)
                {
                    case CarState.OffLine:
                        item.SlCheckBox.ExtImg = caroff;
                        break;
                    case CarState.NoGps:
                        item.SlCheckBox.ExtImg = carnogps;
                        break;
                    case CarState.Stop:
                        item.SlCheckBox.ExtImg = carstop;
                        break;
                    case CarState.Run:
                        item.SlCheckBox.ExtImg = caron;
                        break;
                    case CarState.Alarm:
                        item.SlCheckBox.ExtImg = caralarm;
                        break;
                    case CarState.NoStatus:
                        item.SlCheckBox.ExtImg = null;
                        break;
                }

                if (!y.NewValue.Equals(y.OldValue)) //车辆状态发生改变
                {
                    CarState value = (CarState)y.NewValue;
                    if (item.ParentTreeViewItem != null) //自动计算车辆上线数
                    {
                        if (value != CarState.NoStatus && value != CarState.OffLine)
                        {
                            if (item.IsUnitNode || (!item.IsUnitNode && item.CarOnlineCount == 0))
                                item.CarOnlineCount++; //上线++
                        }
                        else
                        {
                            if (item.CarOnlineCount != 0)
                                item.CarOnlineCount--; //下线--
                        }
                    }
                }
            }));

        public CarState CarState
        {
            get { return (CarState)GetValue(CarStausProperty); }
            set { SetValue(CarStausProperty, value); }
        }

        public SLTreeViewItem()
        {
            InitializeComponent();
            Loaded += SLTreeViewItem_Loaded;
        }

        void SLTreeViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (Header == null)
                InitHead();
        }

        private DateTime lastClickTime = DateTime.MinValue;
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if ((DateTimeHelper.Now - lastClickTime).TotalMilliseconds < 300) //双击
            {
                e.Handled = true;
                if (DoubleClicked != null)
                    DoubleClicked(this, new EventArgs());
            }

            lastClickTime = DateTimeHelper.Now;
            base.OnMouseLeftButtonUp(e);
        }

        private void InitHead()
        {
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            sp.Children.Add(SlCheckBox);
            sp.Children.Add(OnlineCount);
            Header = sp;
        }

        #region 事件
        /// <summary>
        /// 车辆统计发生变化
        /// </summary>
        public event EventHandler CarCountChanged;
        /// <summary>
        /// 选中的车辆发生改变
        /// </summary>
        public static event CheckedCarChangedEventHandler CheckedCarChanged;
        /// <summary>
        /// 双击节点
        /// </summary>
        public static event EventHandler DoubleClicked;
        #endregion

        #region 根节点
        /// <summary>
        /// 根节点单位
        /// </summary>
        public static SLTreeViewItem CreateRootTreeViewItem() => new SLTreeViewItem() { Name = "根单位", DataUnit = new UNIT() { UNITNAME = "根单位", UNITID = -1 } };
        #endregion

        #region 字段
        /// <summary>
        /// 单位小尾巴格式化字符串
        /// </summary>
        private string unitFormatStr = "({0}/{1})";
        /// <summary>
        /// 车辆小尾巴格式化字符串
        /// </summary>
        private string carFormatStr = "({0}) {1}";

        private SLCar _dataCar;
        private int _carOnlineCount;

        #endregion

        #region 属性
        /// <summary>
        /// 浏览模式
        /// </summary>
        public bool BrowseMode { get; set; }
        /// <summary>
        /// 单位数据
        /// </summary>
        public UNIT DataUnit { get; set; }

        /// <summary>
        /// 车辆数据
        /// </summary>
        public SLCar DataCar
        {
            get { return _dataCar; }
            set
            {
                _dataCar = value;
                if (_dataCar != null && !BrowseMode)
                    _dataCar.PropertyChanged += _dataCar_PropertyChanged;
            }
        }
        //车辆属性改变事件
        void _dataCar_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SLCar car = (SLCar)sender;

            if (e.PropertyName == "CarState")    //车辆状态发生改变
            {
                CarState = car.CarState;
            }
            if (car.LastGpsData != null)
            {
                string timeStr = CarState == CarState.OffLine
                    ? car.LastGpsData.GNSSTIME.ToString("[yy/MM/dd HH:mm]")
                    : "";
                ExtensionText = string.Format(carFormatStr, car.LastGpsData.POSITION_CITY, timeStr);
            }
        }

        /// <summary>
        /// 父节点
        /// </summary>
        public SLTreeViewItem ParentTreeViewItem { get; set; }
        /// <summary>
        /// 是否是单位节点
        /// </summary>
        public bool IsUnitNode => DataUnit != null;
        /// <summary>
        /// 车辆数量
        /// </summary>
        public int CarCount { get; private set; }

        /// <summary>
        /// 车辆在线数量
        /// </summary>
        public int CarOnlineCount
        {
            get { return _carOnlineCount; }
            private set
            {
                int changeval = value - _carOnlineCount;
                if (changeval != 0)
                {
                    _carOnlineCount = value;
                    if (IsUnitNode)
                        ExtensionText = string.Format(unitFormatStr, CarCount, _carOnlineCount);

                    if (CarCountChanged != null)    //调用事件
                        CarCountChanged(this, new EventArgs());

                    if (ParentTreeViewItem != null)
                        ParentTreeViewItem.CarOnlineCount += changeval;
                }
            }
        }

        #endregion

        #region 方法
        /// <summary>
        /// 填充数据
        /// </summary>
        public void FillData(List<UNIT> units, List<SLCar> cars)
        {
            FillUnitCars(units, cars, this);

            if (CarCountChanged != null)    //调用事件
                CarCountChanged(this, new EventArgs());
        }

        private void FillUnitCars(List<UNIT> units, List<SLCar> cars, SLTreeViewItem treeItem)
        {
            try
            {
                var tmpunits = units.Where(x => x.PID == treeItem.DataUnit.UNITID).ToList();
                if (tmpunits.Any())
                {//有下级单位
                    tmpunits.Sort((a, b) => string.Compare(a.UNITNAME, b.UNITNAME, StringComparison.CurrentCulture));

                    foreach (var u in tmpunits)
                    {
                        units.Remove(u);
                        int childrenunitcount = units.Count(x => x.PID == u.UNITID);
                        var carlist = cars.Where(x => x.UNITID == u.UNITID).ToList();   //查找该单位下的车辆
                        int carCount = carlist.Count;

                        if (childrenunitcount == 0 && carCount == 0) continue;

                        SLTreeViewItem data = new SLTreeViewItem()
                        {
                            BrowseMode = BrowseMode,
                            DataUnit = u,
                            Text = u.UNITNAME,
                            ParentTreeViewItem = treeItem,
                            CarState = CarState.NoStatus
                        };

                        FillUnitCars(units, cars, data);  //递归寻找下级单位

                        if (carCount > 0) //有车辆
                        {
                            carlist.Sort((a, b) => String.Compare(a.LICENSE, b.LICENSE, StringComparison.CurrentCulture));
                            foreach (SLCar car in carlist)
                            {
                                cars.Remove(car);
                                SLTreeViewItem cardata = new SLTreeViewItem
                                {
                                    BrowseMode = BrowseMode,
                                    Text = car.LICENSE,
                                    ParentTreeViewItem = data,
                                    CarState = CarState.OffLine,
                                    CarCount = 1
                                };
                                cardata.DataCar = car;
                                data.Items.Add(cardata);
                            }
                        }
                        data.CarCount += carCount;
                        data.ExtensionText = string.Format(unitFormatStr, data.CarCount, data.CarOnlineCount);
                        treeItem.CarCount += data.CarCount;

                        if (data.CarCount != 0)
                            treeItem.Items.Add(data);
                        //treeItem.ExtensionText = string.Format(unitFormatStr, CarCount, CarOnlineCount);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// 通过车辆找到TreeViewData
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        public SLTreeViewItem GeTreeViewItemByCar(SLCar car)
        {
            if (car == null) return null;
            SLTreeViewItem rItem = null;

            if (HasItems)
            {
                foreach (SLTreeViewItem x in Items)
                {
                    if (x.DataCar != null && x.DataCar == car)
                    {
                        rItem = x;
                        break;
                    }
                    rItem = x.GeTreeViewItemByCar(car);
                    if (rItem != null)
                        break;
                }
                return rItem;
            }
            else
            {
                return rItem;
            }
        }

        /// <summary>
        /// 获得父级项
        /// </summary>
        /// <param name="addself"></param>
        /// <returns></returns>
        public List<SLTreeViewItem> GeTreeViewItemParents(bool addself = false)
        {
            List<SLTreeViewItem> list = new List<SLTreeViewItem>();
            if (ParentTreeViewItem != null)
            {
                list.AddRange(ParentTreeViewItem.GeTreeViewItemParents());
                list.Add(ParentTreeViewItem);
            }
            if (addself) list.Add(this);
            return list;
        }

        #endregion

    }

    public delegate void CheckedCarChangedEventHandler(SLTreeViewItem item, SLCar car, bool isAdd);
}
