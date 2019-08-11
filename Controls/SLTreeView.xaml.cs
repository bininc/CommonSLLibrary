using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using CommonLibSL.Model;
using Models;

namespace CommonLibSL.Controls
{
    public delegate void DoubleClickCarEventHandler(SLCar car);

    public partial class SLTreeView : SLUserControl
    {
        public event CarCountChangedEventHandler CarCountChanged;
        public event DoubleClickCarEventHandler CarDoubleClicked;
        public event Action<SLCar> SelectedCarChanged;

        public static readonly DependencyProperty BrowseModeProperty = DependencyProperty.Register(
            "BrowseMode", typeof(bool), typeof(SLTreeView), new PropertyMetadata(default(bool)));
        /// <summary>
        /// 浏览模式
        /// </summary>
        public bool BrowseMode
        {
            get { return (bool)GetValue(BrowseModeProperty); }
            set { SetValue(BrowseModeProperty, value); }
        }

        private readonly Dictionary<SLCar, SLTreeViewItem> _checkedCars = new Dictionary<SLCar, SLTreeViewItem>();
        private SLTreeViewItem RootTreeViewItem;
        public SLTreeView()
        {
            InitializeComponent();
            InitEvent();
        }

        protected override void OnLoad()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += (o, args) =>
            {
                ScrollViewer sv = tv.GetScrollHost();
                if (sv != null)
                {
                    sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    timer.Stop();
                }
            };
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        void InitEvent()
        {
            RootTreeViewItem = SLTreeViewItem.CreateRootTreeViewItem();
            RootTreeViewItem.CarCountChanged += (x, y) =>
            {
                if (CarCountChanged != null)
                {
                    CarCountChanged(RootTreeViewItem.CarCount, RootTreeViewItem.CarOnlineCount);
                }
            };
            SLTreeViewItem.CheckedCarChanged += SLTreeViewData_CheckedCarChanged;
            SLTreeViewItem.DoubleClicked += SLTreeViewItem_DoubleClicked;

            tv.SetBinding(TreeView.ItemsSourceProperty, new Binding("Items"));
            tv.DataContext = RootTreeViewItem;//数据源设置
            tv.SelectedItemChanged += tv_SelectedItemChanged;
        }

        /// <summary>
        /// 选择改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SLTreeViewItem item = e.NewValue as SLTreeViewItem;
            SLCar car = null;
            if (item != null)
            {
                car = item.DataCar;
            }

            if (SelectedCarChanged != null)
            {
                SelectedCarChanged(car);
            }
        }

        /// <summary>
        /// 选中改变事件
        /// </summary>
        /// <param name="car"></param>
        /// <param name="isAdd"></param>
        void SLTreeViewData_CheckedCarChanged(SLTreeViewItem item, SLCar car, bool isAdd)
        {
            if (isAdd)
            {
                if (!_checkedCars.ContainsKey(car))
                {
                    _checkedCars.Add(car, item);
                }
            }
            else
            {
                if (_checkedCars.ContainsKey(car))
                {
                    _checkedCars.Remove(car);
                }
            }
        }

        /// <summary>
        /// 数据绑定方法
        /// </summary>
        public void Bind(List<UNIT> units, List<SLCar> cars, bool isOilWell = false)
        {
            if (units != null && cars != null)
            {
                //var t= cars.First(x => string.IsNullOrWhiteSpace(x.LICENSE)); //调试用 找错误数据

                List<SLCar> carlist = null;
                if (isOilWell)
                {
                    carlist = cars.Where(x => x.IS_POLL_OIL_SPOT == 1).ToList();
                }
                else
                {
                    carlist = cars.Where(x => x.IS_POLL_OIL_SPOT != 1).ToList();
                }
                List<UNIT> unitlist = units.ToList();
                //填充数据
                RootTreeViewItem.BrowseMode = BrowseMode;
                RootTreeViewItem.FillData(unitlist, carlist);
            }
        }

        /// <summary>
        /// 得到选中的车辆信息
        /// </summary>
        /// <returns></returns>
        public List<SLCar> CheckedCars
        {
            get
            {
                return new List<SLCar>(_checkedCars.Keys.ToArray());
            }
        }

        /// <summary>
        /// 得到选择的车辆
        /// </summary>
        public SLCar Selectedcar
        {
            get
            {
                SLTreeViewItem item = tv.SelectedItem as SLTreeViewItem;
                if (item == null)
                {
                    return null;
                }
                return item.DataCar;
            }
        }

        /// <summary>
        /// 展开车辆
        /// </summary>
        /// <param name="car"></param>
        public void ExpandCar(SLCar car)
        {
            SLTreeViewItem data = RootTreeViewItem.GeTreeViewItemByCar(car);
            if (data != null)
            {
                var datas = data.GeTreeViewItemParents(false);
                if (!datas.Any()) return;
                //datas.ForEach(x => x.IsExpanded = true);
                tv.ExpandPath(datas);
                tv.SelectItem(data);
                if (tv.SelectedItem == null || tv.SelectedItem != data)
                {
                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Tick += (sender, args) =>
                    {
                        tv.SelectItem(data);
                        if (tv.SelectedItem != null && tv.SelectedItem == data)
                        {
                            timer.Stop();
                            timer = null;
                        }
                    };
                    timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
                    timer.Start();
                }
            }
        }

        /// <summary>
        /// 双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SLTreeViewItem_DoubleClicked(object sender, EventArgs e)
        {
            SLTreeViewItem item = (SLTreeViewItem)sender;
            if (!item.IsUnitNode)
            {
                if (CarDoubleClicked != null)
                    CarDoubleClicked(item.DataCar);
            }
        }

        /// <summary>
        /// 选中当前选择的节点
        /// </summary>
        public void CheckedSelectedItem()
        {
            if (tv.SelectedItem != null)
            {
                SLTreeViewItem item = (SLTreeViewItem)tv.SelectedItem;
                if (item.IsUnitNode) return;

                foreach (KeyValuePair<SLCar, SLTreeViewItem> keyValuePair in _checkedCars.ToArray())
                {
                    if (keyValuePair.Value.Checked)
                        keyValuePair.Value.Checked = false;
                }

                item.Checked = true;
            }
        }

        /// <summary>
        /// 选中所有节点
        /// </summary>
        public void CheckedAllItem()
        {
            RootTreeViewItem.Checked = true;
        }

        /// <summary>
        /// 取消选中所有节点
        /// </summary>
        public void UnCheckedAllItem()
        {
            RootTreeViewItem.Checked = false;
        }
    }
}
