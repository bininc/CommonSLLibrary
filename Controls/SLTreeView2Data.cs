using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommLiby;
using CommonLibSL.Model;
using Models;
using CommLiby.Cyhk;

namespace CommonLibSL.Controls
{
    public delegate void CheckedItemChangedEventHandler(SLTreeView2Data item, SLCar car, bool isAdd);
    public class SLTreeView2Data : ClassBase_Notify<SLTreeView2Data>
    {
        #region 图片资源
        private static readonly BitmapImage fuxuankuang = new BitmapImage(new Uri("/ImageResource;component/Images/fuxuankuang.png", UriKind.Relative));
        private static readonly BitmapImage fuxuankuangon = new BitmapImage(new Uri("/ImageResource;component/Images/fuxuankuang_on.png", UriKind.Relative));
        private static readonly BitmapImage caroff = new BitmapImage(new Uri("/ImageResource;component/Images/car/treeOff.png", UriKind.Relative));
        private static readonly BitmapImage carnogps = new BitmapImage(new Uri("/ImageResource;component/Images/car/treeNoGps.png", UriKind.Relative));
        private static readonly BitmapImage carstop = new BitmapImage(new Uri("/ImageResource;component/Images/car/treeStop.png", UriKind.Relative));
        private static readonly BitmapImage caron = new BitmapImage(new Uri("/ImageResource;component/Images/car/treeOn.png", UriKind.Relative));
        private static readonly BitmapImage caralarm = new BitmapImage(new Uri("/ImageResource;component/Images/car/treeAlarm.png", UriKind.Relative));

        private ImageSource _checkedImg = fuxuankuangon;
        private ImageSource _unCheckedImg = fuxuankuang;
        #endregion

        #region 变量区
        private CarState _state;
        private int _onlineCount;
        private SLCar _car;
        private bool _checked;
        private string _extensionText;
        private ImageSource _carStateImg;
        private Brush _foreground;

        #endregion

        /// <summary>
        /// 车辆在线数发生改变
        /// </summary>
        public event Action CarCountChanged;
        /// <summary>
        /// 选中的车辆发生改变
        /// </summary>
        public event CheckedItemChangedEventHandler CheckedItemChanged;


        public bool Selected { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    OnPropertyChanged("Checked", true);
                    Foreground = new SolidColorBrush(value ? Color.FromArgb(255, 69, 139, 194) : Color.FromArgb(255, 85, 85, 85));

                    if (value)
                    {
                        if (!onlythis)
                            //父级选中子级也选中      
                            foreach (SLTreeView2Data item in Items)
                                item.Checked = true;

                        //所有子级选中父级也选中
                        if (ParentItem != null && !ParentItem.Checked && ParentItem.Items.All(a => a.Checked))
                        {
                            ParentItem.onlythis = true;
                            ParentItem.Checked = true;
                        }
                    }

                    if (!value)
                    {
                        if (!onlythis)
                            //父级取消字节也取消
                            foreach (SLTreeView2Data item in Items)
                                item.Checked = false;

                        //子节点有取消 父节点取消
                        if (ParentItem != null && ParentItem.Checked)
                        {
                            ParentItem.onlythis = true;
                            ParentItem.Checked = false;
                        }
                    }

                    if (ItemType == TreeViewItemType.Car) //车辆节点
                    {
                        CheckedItemChanged?.Invoke(this, Car, value);
                        GetRootData(this).CheckedItemChanged?.Invoke(this, Car, value);
                    }
                }
            }
        }

        public static SLTreeView2Data GetRootData(SLTreeView2Data data)
        {
            if (data == null) return null;
            if (data.ParentItem == null) return data;
            return GetRootData(data.ParentItem);
        }

        private bool _onlythis;
        private bool onlythis
        {
            get
            {
                bool r = _onlythis;
                if (_onlythis)
                    _onlythis = false;
                return r;
            }
            set { _onlythis = value; }
        }

        /// <summary>
        /// 复选框选中图片
        /// </summary>
        public ImageSource CheckedImg
        {
            get { return _checkedImg; }
            set { _checkedImg = value; }
        }
        /// <summary>
        /// 复选框未选中图片
        /// </summary>
        public ImageSource UnCheckedImg
        {
            get { return _unCheckedImg; }
            set { _unCheckedImg = value; }
        }

        /// <summary>
        /// 前景色
        /// </summary>
        public Brush Foreground
        {
            get
            {
                if (_foreground == null)
                    _foreground = new SolidColorBrush(Color.FromArgb(255, 85, 85, 85));
                return _foreground;
            }
            private set
            {
                if (_foreground != value)
                {
                    _foreground = value;
                    OnPropertyChanged("Foreground", true);
                }
            }
        }

        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 最后一次位置所在城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 车辆总数
        /// </summary>
        public int CarCount { get; set; }

        /// <summary>
        /// 车辆在线数
        /// </summary>
        public int OnlineCount
        {
            get { return _onlineCount; }
            set
            {
                int changeval = value - _onlineCount;
                if (changeval != 0) //值发生改变
                {
                    _onlineCount = value;

                    if (ItemType == TreeViewItemType.Unit)
                        UpdateExtensionText();

                    if (CarCountChanged != null)    //调用事件
                        CarCountChanged();

                    if (ParentItem != null)
                        ParentItem.OnlineCount += changeval;
                }
            }
        }

        /// <summary>
        /// 后缀时间字符串
        /// </summary>
        public string TimeStr { get; set; }
        /// <summary>
        /// 单位小尾巴格式化字符串
        /// </summary>
        private static string unitFormatStr = "({0}/{1})";
        /// <summary>
        /// 车辆小尾巴格式化字符串
        /// </summary>
        private static string carFormatStr = "({0}) {1}";

        /// <summary>
        /// 扩展(后缀)文本
        /// </summary>
        public string ExtensionText
        {
            get { return _extensionText; }
            private set
            {
                if (_extensionText != value)
                {
                    _extensionText = value;
                    OnPropertyChanged(nameof(ExtensionText), true);
                }
            }
        }

        /// <summary>
        /// 更新后缀文字方法
        /// </summary>
        private void UpdateExtensionText()
        {
            switch (ItemType)
            {
                case TreeViewItemType.Car:
                    string p1 = City;
                    string p2 = TimeStr;
                    if (!_car.IsInService())
                    {
                        p1 = "服务费已到期";
                        p2 = _car.STOP_CAR_DATE.ToString("[yyyy/MM/dd]");
                    }
                    ExtensionText = string.Format(carFormatStr, p1, p2);
                    break;
                case TreeViewItemType.Unit:
                    ExtensionText = !BrowseMode ? string.Format(unitFormatStr, CarCount, OnlineCount) : string.Format("({0})", CarCount);
                    break;
            }
        }

        /// <summary>
        /// 车辆状态
        /// </summary>
        public CarState State
        {
            get { return _state; }
            set
            {
                if (_state != value) //车辆状态发生改变
                {
                    if ((_state == CarState.NoStatus || _state == CarState.OffLine) && value != CarState.NoStatus && value != CarState.OffLine)
                    {//上线++
                        if (OnlineCount != 1)
                            OnlineCount = 1;
                    }
                    if (_state != CarState.NoStatus && _state != CarState.OffLine && (value == CarState.NoStatus || value == CarState.OffLine))
                    {//下线--
                        if (OnlineCount != 0)
                            OnlineCount = 0;
                    }
                    _state = value;


                    switch (value)
                    {
                        case CarState.OffLine:
                            CarStateImg = caroff;
                            TimeStr = _car.LastGpsData.GNSSTIME.ToString("[yyyy/MM/dd HH:mm]");
                            UpdateExtensionText();
                            break;
                        case CarState.NoGps:
                            CarStateImg = carnogps;
                            break;
                        case CarState.Stop:
                            CarStateImg = carstop;
                            break;
                        case CarState.Run:
                            CarStateImg = caron;
                            break;
                        case CarState.Alarm:
                            CarStateImg = caralarm;
                            break;
                        case CarState.NoStatus:
                            CarStateImg = null;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 车辆不同状态图标
        /// </summary>
        public ImageSource CarStateImg
        {
            get { return _carStateImg; }
            private set
            {
                if (_carStateImg != value)
                {
                    _carStateImg = value;
                    OnPropertyChanged("CarStateImg", true);
                }
            }
        }

        public TreeViewItemType ItemType { get; private set; }

        /// <summary>
        /// 车辆数据信息
        /// </summary>
        public SLCar Car
        {
            get { return _car; }
            set
            {
                _car = value;
                if (_car != null)
                {
                    if (!BrowseMode && !_car.IsCopy)
                    {
                        _car.PropertyChanged += _datacar_PropertyChanged;
                        if (_car.LastGpsData != null)
                        {
                            State = _car.CarState;
                            if (State == CarState.OffLine)
                                TimeStr = _car.LastGpsData.GNSSTIME.ToString("[yyyy/MM/dd HH:mm]");
                            else
                                TimeStr = "";

                            if (!gwcMode)
                                City = _car.LastGpsData.POSITION_CITY;
                            else
                            {
                                switch (_car.STATE)
                                {
                                    case 1:
                                        City = "任务中";
                                        break;
                                    default:
                                        City = "可调派";
                                        break;
                                }
                            }
                            UpdateExtensionText();
                        }
                    }

                }
            }
        }

        private void _datacar_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SLCar c = (SLCar)sender;

            if (e.PropertyName == nameof(SLCar.CarState))
                State = c.CarState; //车辆状态发生改变
            else if (e.PropertyName == nameof(SLCar.LastGpsData) + "." + nameof(NEWTRACK))
            {
                TimeStr = "";

                if (!gwcMode)
                    City = c.LastGpsData.POSITION_CITY;
                UpdateExtensionText();
            }

            if (gwcMode && e.PropertyName == nameof(SLCar.STATE))
            {
                switch (c.STATE)
                {
                    case 1:
                        City = "任务中";
                        break;
                    default:
                        City = "可调派";
                        break;
                }
                UpdateExtensionText();
            }
        }

        public UNIT Unit { get; set; }

        public ObservableCollection<SLTreeView2Data> Items { get; private set; }

        public SLTreeView2Data ParentItem { get; set; }

        /// <summary>
        /// 浏览模式
        /// </summary>
        public bool BrowseMode { get; set; }
        /// <summary>
        /// 公务车模式
        /// </summary>
        public bool gwcMode { get; set; }
        public SLTreeView2Data()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="itemType"></param>
        public SLTreeView2Data(TreeViewItemType itemType)
            : this()
        {
            ItemType = itemType;
            Items = new ObservableCollection<SLTreeView2Data>();
            if (itemType == TreeViewItemType.Car)
            {
                _carStateImg = caroff;
            }
        }

        /// <summary>
        /// 根节点单位
        /// </summary>
        public static SLTreeView2Data CreateRootTreeViewItem()
        {
            return new SLTreeView2Data(TreeViewItemType.Unit) { Name = "根单位", Unit = new UNIT() { UNITNAME = "根单位", UNITID = -1 } };
        }

        public static SLTreeView2Data GetRooTreeView2Data(SLTreeView2Data data = null)
        {
            if (data == null) return null;

            if (data.ParentItem != null)
                return GetRooTreeView2Data(data.ParentItem);
            else
                return data;
        }

        public SLTreeView2Data GetThisRooTreeView2Data()
        {
            return GetRooTreeView2Data(this);
        }

        /// <summary>
        /// 填充数据
        /// </summary>
        public void FillData(List<UNIT> units, IEnumerable<SLCar> cars)
        {
            Items.Clear();

            FillUnitCars(units, cars, this);

            if (CarCountChanged != null)    //调用事件
                CarCountChanged();
        }

        /// <summary>
        /// 计算单位上下级关系
        /// </summary>
        /// <param name="units"></param>
        /// <param name="cars"></param>
        /// <param name="unitData"></param>
        private void FillUnitCars(List<UNIT> units, IEnumerable<SLCar> cars, SLTreeView2Data unitData)
        {
            var tmpunits = units.Where(x => x.PID == unitData.Unit.UNITID).OrderBy(u => u.UNITNAME);    //找到下属子单位
            if (tmpunits.Any())
            {//有下级单位

                foreach (var u in tmpunits) //遍历下级单位
                {
                    bool hasChildrenUnit = units.Any(x => x.PID == u.UNITID);      //是否存在下属单位
                    var carlist = cars.Where(x => x.UNITID == u.UNITID).OrderBy(c => c.LICENSE); //查找该单位下的车辆
                    bool hasCar = carlist.Any();   //该单位下车辆数量

                    if (!hasChildrenUnit && !hasCar) continue;    //跳过没有下属的单位

                    SLTreeView2Data data = new SLTreeView2Data(TreeViewItemType.Unit)  //创建一个单位数据
                    {
                        Unit = u,
                        Name = u.UNITNAME,
                        ParentItem = unitData,
                        BrowseMode = BrowseMode,
                        gwcMode = gwcMode
                    };
                    if (BrowseMode)
                        data.CheckedImg = data.UnCheckedImg = null;

                    unitData.Items.Add(data);   //将单位添加到父单位下

                    if (hasChildrenUnit) //有下属单位
                        FillUnitCars(units, cars, data);  //递归寻找下级单位（单位优先显示）

                    if (hasCar) //有车辆
                    {
                        foreach (SLCar car in carlist)  //遍历下属车辆
                        {
                            SLTreeView2Data cardata = new SLTreeView2Data(TreeViewItemType.Car) //创建一个车辆数据
                            {
                                BrowseMode = BrowseMode,
                                gwcMode = gwcMode,
                                ParentItem = data,
                                Car = car,
                                Name = car.LICENSE
                            };

                            if (BrowseMode)
                                cardata.State = CarState.Run;

                            if (BrowseMode)
                                cardata.CheckedImg = cardata.UnCheckedImg = null;

                            data.Items.Add(cardata); //将车辆添加到 该单位下
                            data.CarCount += 1;  //计算车辆数
                        }
                    }

                    if (data.CarCount > 0)
                    {
                        data.UpdateExtensionText(); //更新串显示
                        unitData.CarCount += data.CarCount;   //父单位车辆数
                    }
                    else
                        unitData.Items.Remove(data);
                }
            }
        }


        /// <summary>
        /// 根据车辆找到TreeViewData
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        public SLTreeView2Data GetTreeViewDataByCar(SLCar car)
        {
            if (car == null) return null;

            bool isUnit = car.CARID == 0 && string.IsNullOrEmpty(car.MAC);

            SLTreeView2Data rItem = null;

            foreach (SLTreeView2Data x in Items)
            {
                if (!isUnit)
                {
                    if (x.Car != null && x.Car == car)
                    {
                        rItem = x;
                        break;
                    }
                }
                else
                {
                    if (x.Car == null && x.Name == car.LICENSE)
                    {
                        rItem = x;
                        break;
                    }
                }

                rItem = x.GetTreeViewDataByCar(car);
                if (rItem != null)
                    break;
            }
            return rItem;
        }

        /// <summary>
        /// 获得父级项
        /// </summary>
        /// <param name="addself"></param>
        /// <returns></returns>
        public List<SLTreeView2Data> GetTreeViewDataParents(bool addself = false)
        {
            List<SLTreeView2Data> list = new List<SLTreeView2Data>();
            if (ParentItem != null)
            {
                list.AddRange(ParentItem.GetTreeViewDataParents());
                list.Add(ParentItem);
            }
            if (addself) list.Add(this);
            return list;
        }


    }
}
