using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CommLiby;
using CommonLibSL.Model;
using Models;

namespace CommonLibSL.Controls
{
    public delegate void CarCountChangedEventHandler(int carCount, int carOnlineCount);
    public partial class SLTreeView2 : SLUserControl
    {
        public event CarCountChangedEventHandler CarCountChanged;
        public event Action<SLCar> CarDoubleClicked;
        public event Action<SLCar> SelectedCarChanged;
        private bool _browseMode = false;
        private readonly SLTreeView2Data _rootUnitData;
        private readonly Dictionary<SLCar, SLTreeView2Data> _checkedCars = new Dictionary<SLCar, SLTreeView2Data>();

        /// <summary>
        /// 选中的车辆
        /// </summary>
        public SLCar SelectedCar { get; private set; }

        public SLTreeView2()
        {
            InitializeComponent();
            _rootUnitData = SLTreeView2Data.CreateRootTreeViewItem();
            InitEvent();
        }

        void InitEvent()
        {
            _rootUnitData.CarCountChanged += () =>
            {
                if (CarCountChanged != null)
                {
                    CarCountChanged(_rootUnitData.CarCount, _rootUnitData.OnlineCount);
                }
            };
            _rootUnitData.CheckedItemChanged += SLTreeView2Data_CheckedItemChanged;
            tv.SelectedItemChanged += tv_SelectedItemChanged;
            tv.MouseLeftButtonUp += tv_MouseLeftButtonUp;
        }

        private DateTime lastClickTime = DateTime.MinValue;
        /// <summary>
        /// 实现双击时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tv_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ((DateTimeHelper.Now - lastClickTime).TotalMilliseconds < 300) //双击
            {
                e.Handled = true;
                try
                {
                    if (SelectedCar != null && CarDoubleClicked != null)
                        CarDoubleClicked(SelectedCar);
                }
                catch (Exception ex)
                {

                }
            }

            lastClickTime = DateTimeHelper.Now;
        }

        /// <summary>
        /// 选中改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SLTreeView2Data_CheckedItemChanged(SLTreeView2Data item, SLCar car, bool isAdd)
        {
            if (isAdd)
            {
                if (!_checkedCars.ContainsKey(car))
                    _checkedCars.Add(car, item);
            }
            else
            {
                if (_checkedCars.ContainsKey(car))
                    _checkedCars.Remove(car);
            }
        }

        /// <summary>
        /// 选择改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SLTreeView2Data item = e.NewValue as SLTreeView2Data;
            if (item != null)
            {
                if (SelectedCar == item.Car) return;

                SelectedCar = item.Car;
                lastClickTime = DateTime.MinValue;
                if (SelectedCarChanged != null)
                {
                    SelectedCarChanged(SelectedCar);
                }
            }
        }


        /// <summary>
        /// 浏览模式
        /// </summary>
        public bool BrowseMode
        {
            get { return _browseMode; }
            set
            {
                _browseMode = value;
                if (!value)
                {
                    //ToolTipService.SetToolTip(tv, "车辆图标:\r\n灰色:不在线\r\n银色:不定位\r\n黄色:停车中\r\n绿色:行驶中\r\n红色:报警中");
                    lblToolTip.Text = "车辆图标说明\r\n灰色：不在线\r\n银色：不定位\r\n黄色：停车中\r\n绿色：行驶中\r\n红色：报警中";
                    lblToolTip.Visibility = Visibility.Visible;
                }
                else
                {
                    //ToolTipService.SetToolTip(tv, string.Empty);
                    //lblToolTip.Text = "";
                    lblToolTip.Opacity = 0;
                    lblToolTip.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// 公务车模式
        /// </summary>
        public bool gwcMode { get; set; }


        protected override void OnLoad()
        {
            if (!_browseMode) BrowseMode = false;

            //隐藏滚动条
            //DispatcherTimer timer = new DispatcherTimer();
            //timer.Tick += (o, args) =>
            //{
            //    ScrollViewer sv = tv.GetScrollHost();
            //    if (sv != null)
            //    {
            //        //sv.Background = new SolidColorBrush(Colors.Transparent); //解决鼠标放在空白地方不滚动的Bug
            //        sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            //        timer.Stop();
            //    }
            //};
            //timer.Interval = new TimeSpan(0, 0, 1);
            //timer.Start();
        }

        /// <summary>
        /// 数据绑定方法
        /// </summary>
        public void Bind(List<UNIT> units, List<SLCar> cars, bool isOilWell = false)
        {
            if (units != null && cars != null)
            {
                _checkedCars.Clear();
                SelectedCar = null;
                //var t= cars.First(x => string.IsNullOrWhiteSpace(x.LICENSE)); //调试用 找错误数据
                IEnumerable<SLCar> carlist = null;
                if (isOilWell)
                {
                    carlist = cars.Where(x => x.IS_POLL_OIL_SPOT == 1);
                }
                else
                {
                    carlist = cars.Where(x => x.IS_POLL_OIL_SPOT != 1);
                }
                List<UNIT> unitlist = units;

                _rootUnitData.BrowseMode = BrowseMode;  //浏览模式赋值
                _rootUnitData.gwcMode = gwcMode;
                //DateTime time = DateTime.Now;
                _rootUnitData.FillData(unitlist, carlist);
                //double mis = (DateTime.Now - time).TotalMilliseconds;
                Common_liby.InvokeMethodOnMainThread(() =>
                {
                    tv.Items.Clear();
                    tv.ItemsSource = _rootUnitData.Items; //数据源设置
                    tv.Loaded += (sender, e) =>
                    {
                        if (_rootUnitData.Items.Count == 1) //子节点只有一个
                        {
                            tv.ExpandToDepth(1);    //展开子节点
                        }
                    };
                });
            }
        }

        private ScrollViewer _tvScrollViewer;
        /// <summary>
        /// 展开车辆
        /// </summary>
        /// <param name="car"></param>
        public void ExpandCar(SLCar car)
        {
            if (_tvScrollViewer == null)
                _tvScrollViewer = tv.GetScrollHost();
            SLTreeView2Data data = _rootUnitData.GetTreeViewDataByCar(car);
            if (data != null)
            {
                var datas = data.GetTreeViewDataParents(false);
                if (datas.Any())
                    tv.ExpandPath(datas);

                tv.SelectItem(data);
                if (tv.SelectedItem == null || tv.SelectedItem != data)
                {
                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Tick += (sender, args) =>
                    {
                        if (tv.SelectedItem != null && tv.SelectedItem == data)
                        {
                            _tvScrollViewer.ScrollIntoView(tv.GetSelectedContainer());
                            timer.Stop();
                            timer = null;
                        }
                        else
                            tv.SelectItem(data);
                    };
                    timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
                    timer.Start();
                }
                else
                {
                    _tvScrollViewer.ScrollIntoView(tv.GetSelectedContainer());
                }
            }
        }

        /// <summary>
        /// 选中当前选择的节点
        /// </summary>
        public void CheckedSelectedItem()
        {
            if (tv.SelectedItem != null)
            {
                UnCheckedAllItem();
                SLTreeView2Data item = (SLTreeView2Data)tv.SelectedItem;
                //if (item.IsUnitNode) return;
                item.Checked = true;
            }
        }

        /// <summary>
        /// 选中所有节点
        /// </summary>
        public void CheckedAllItem()
        {
            _rootUnitData.Checked = true;
        }

        /// <summary>
        /// 取消选中所有节点
        /// </summary>
        public void UnCheckedAllItem()
        {
            if (_rootUnitData.Checked)
                _rootUnitData.Checked = false;
            else
                foreach (SLTreeView2Data data in _checkedCars.Values.ToList())
                {
                    if (data.Checked)
                        data.Checked = false;
                }
        }

        /// <summary>
        /// 得到选中的车辆信息
        /// </summary>
        /// <returns></returns>
        public List<SLCar> CheckedCars
        {
            get { return _checkedCars.Keys.ToList(); }
        }
    }

    public class TreeView2 : TreeView
    {
        #region 鼠标滚轮Bug修复(包括在Chrome浏览器下)

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            MouseWheelTool.Instance.MouseWheel += InvokeScrollBar;
        }

        private bool Focused;

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            Focused = true;
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            Focused = false;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            Focused = true;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            Focused = false;
        }

        private void InvokeScrollBar(int mouseDelta)
        {
            if (!Focused || mouseDelta == 0) return;

            short mouseNum = (short)(mouseDelta / 120);
            ScrollAmount scrollAmount = ScrollAmount.NoAmount;
            if (mouseNum < 0)
            {
                //向下滑动
                if (mouseNum >= -2)
                    scrollAmount = ScrollAmount.SmallIncrement;
                else
                    scrollAmount = ScrollAmount.LargeIncrement;
            }
            else if (mouseNum > 0)
            {
                //向上滑动
                if (mouseNum <= 2)
                    scrollAmount = ScrollAmount.SmallDecrement;
                else
                    scrollAmount = ScrollAmount.LargeDecrement;
            }

            if (scrollAmount == ScrollAmount.NoAmount) return;

            //修复滚轮滚动Bug

            //取得AutomationPeer 
            AutomationPeer automationPeer = FrameworkElementAutomationPeer.FromElement(this);
            if (automationPeer == null)
                automationPeer = FrameworkElementAutomationPeer.CreatePeerForElement(this);
            //得到scroll provider 
            IScrollProvider scrollProvider = automationPeer.GetPattern(PatternInterface.Scroll) as IScrollProvider;
            if (scrollProvider != null)
            {
                if (scrollProvider.VerticallyScrollable)
                {
                    scrollProvider.Scroll(ScrollAmount.NoAmount, scrollAmount);
                }
            }

        }
        #endregion
    }
}
