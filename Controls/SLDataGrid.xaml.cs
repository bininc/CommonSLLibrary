using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CommLiby;

namespace CommonLibSL.Controls
{
    public partial class SLDataGrid : SLUserControl
    {
        private List<GridBindColumn> _bindColumns;

        private DateTime lastClickTime = DateTime.MinValue;
        public event Action<object> SLDataGridDoubleClicked;
        public event Action<object> SLDataGridClicked;
        private object lastSelItem;
        private DataGridColumn lastSelColnum;

        public SLDataGrid()
        {
            InitializeComponent();
            DataGrid.SelectionMode = DataGridSelectionMode.Single;
            DataGrid.MouseLeftButtonUp += DataGrid_MouseLeftButtonUp;
            DataGrid.KeyDown += (sender, e) => { OnKeyDown(e); };
            DataGrid.MouseEnter += DataGrid_MouseEnter;
            DataGrid.MouseLeave += DataGrid_MouseLeave;
        }

        private void DataGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            OnMouseLeave(e);
        }

        private void DataGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            OnMouseEnter(e);
        }

        /// <summary>
        /// 激活选中项
        /// </summary>
        /// <param name="item"></param>
        public void ActiveSelectedItem(object item)
        {
            if (item == null) return;

            if (DataGrid.SelectedItem == null)
            {
                DataGrid.SelectedItem = item;
            }

            if (!DataGrid.SelectedItem.Equals(item))
            {
                DataGrid.SelectedItem = item;
            }

            if (DataGrid.SelectedIndex < 0)
            {
                DispatcherTimer dTimer = new DispatcherTimer();
                dTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                dTimer.Tick += (s, e) =>
                {
                    if (DataGrid.SelectedIndex >= 0)
                    {
                        DataGrid.ScrollIntoView(item, DataGrid.CurrentColumn);
                        dTimer.Stop();
                        dTimer = null;
                    }
                };
            }
            else
                DataGrid.ScrollIntoView(item, DataGrid.CurrentColumn);
        }

        /// <summary>
        /// DataGrid点击事件
        /// </summary>
        void DataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataGrid.SelectedItem == null) return;

            if (lastSelItem != DataGrid.SelectedItem || lastSelColnum != DataGrid.CurrentColumn)
            {
                lastClickTime = DateTime.MinValue;
                lastSelItem = DataGrid.SelectedItem;
                lastSelColnum = DataGrid.CurrentColumn;
            }

            if ((DateTimeHelper.Now - lastClickTime).TotalMilliseconds < 300) //双击
            {
                int colindex = DataGrid.CurrentColumn.DisplayIndex;
                GridBindColumn col;
                try
                {
                    col = _bindColumns.First(x => x.DisplayIndex == colindex); //得到当前列 
                }
                catch
                {
                    col = null;
                }
                if (col != null && col.Children != null && col.Children.Any())
                {
                    for (int i = colindex + 1; i < colindex + col.Children.Count + 1; i++)
                    {
                        if (DataGrid.Columns[i].Visibility == Visibility.Collapsed)
                            DataGrid.Columns[i].Visibility = Visibility.Visible;
                        else if (DataGrid.Columns[i].Visibility == Visibility.Visible)
                            DataGrid.Columns[i].Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    if (SLDataGridDoubleClicked != null)
                    {
                        SLDataGridDoubleClicked(DataGrid.SelectedItem);
                    }
                }
                e.Handled = true;
            }
            else
            {   //单击
                if (SLDataGridClicked != null)
                    SLDataGridClicked(DataGrid.SelectedItem);
            }

            lastClickTime = DateTimeHelper.Now;
        }

        /// <summary>
        /// 绑定数据源
        /// </summary>
        /// <param name="source"></param>
        public void Bind(ICollection source, List<GridBindColumn> bindColumn)
        {
            if (bindColumn == null) return;

            bindColumn.Sort();  //进行排序
            _bindColumns = bindColumn;

            DataGrid.Columns.Clear();   //清除原有列

            int d_index = 0;
            bindColumn.ForEach(x => //循环绑定列
            {
                DataGrid.Columns.Add(CreateDataGridColumn(x));  //绑定主列
                x.SetDisplayIndex(d_index++);
                if (x.Children != null && x.Children.Any())
                {
                    x.Children.ForEach(y =>
                    {   //绑定附属列
                        DataGrid.Columns.Add(CreateDataGridColumn(y));
                        y.SetDisplayIndex(d_index++);
                    });
                }
            });

            DataGrid.ItemsSource = source;
        }

        /// <summary>
        /// 创建DataGridColumn列
        /// </summary>
        /// <param name="bindColumn"></param>
        private DataGridColumn CreateDataGridColumn(GridBindColumn bindColumn)
        {
            //创建列对象
            DataGridColumn col;
            if (bindColumn.ColumnTemplate == null)
            {
                DataGridTextColumn textColumn = new DataGridTextColumn();
                textColumn.Header = bindColumn.ColumnName;
                Binding binding = new Binding(bindColumn.DataName);
                if (bindColumn.Converter != null)
                    binding.Converter = bindColumn.Converter;
                if (bindColumn.ConverterParameter != null)
                    binding.ConverterParameter = bindColumn.ConverterParameter;
                textColumn.Binding = binding;
                textColumn.Foreground = bindColumn.Foreground;

                col = textColumn;
            }
            else
            {
                DataGridTemplateColumn templateColumn = new DataGridTemplateColumn();
                templateColumn.Header = bindColumn.ColumnName;
                templateColumn.CellTemplate = bindColumn.ColumnTemplate;
                col = templateColumn;
            }

            col.Width = DataGridLength.Auto; //自动列宽
            if (bindColumn.IsHide)
                col.Visibility = Visibility.Collapsed;
            else
                col.Visibility = bindColumn.DefaultShow ? Visibility.Visible : Visibility.Collapsed;  //控制列显示
            bindColumn.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(bindColumn.IsHide))
                {
                    if (bindColumn.IsHide)
                        col.Visibility = Visibility.Collapsed;
                    else
                        col.Visibility = bindColumn.DefaultShow ? Visibility.Visible : Visibility.Collapsed;  //控制列显示
                }
            };
            col.IsReadOnly = true;  //只读
            return col;
        }

        #region 鼠标滚轮Bug修复(包括在Chrome浏览器下)

        protected override void OnLoad()
        {
            base.OnLoad();
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
            AutomationPeer automationPeer = FrameworkElementAutomationPeer.FromElement(DataGrid);
            if (automationPeer == null)
                automationPeer = FrameworkElementAutomationPeer.CreatePeerForElement(DataGrid);
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

    /// <summary>
    /// 绑定对象
    /// </summary>
    public class GridBindColumn : ClassBase_Notify<GridBindColumn>, IComparable<GridBindColumn>
    {
        //默认显示
        private bool _defultShow = true;
        private List<GridBindColumn> _children = null;
        private bool _isHide;

        /// <summary>
        /// 显示的顺序
        /// </summary>
        public float DisplayOrder { get; set; }
        /// <summary>
        /// 显示的索引顺序
        /// </summary>
        public int DisplayIndex { get; private set; }
        /// <summary>
        /// 列标题
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// 数据列字段
        /// </summary>
        public string DataName { get; set; }

        /// <summary>
        /// 前景色
        /// </summary>
        public Brush Foreground { get; set; }

        /// <summary>
        /// 默认是否显示
        /// </summary>
        public bool DefaultShow
        {
            get { return _defultShow; }
            set { _defultShow = value; }
        }

        /// <summary>
        /// 列模板 空的话为Text类型
        /// </summary>
        public DataTemplate ColumnTemplate { get; set; }
        /// <summary>
        /// 数据转换
        /// </summary>
        public IValueConverter Converter { get; set; }
        /// <summary>
        /// 数据转换参数
        /// </summary>
        public object ConverterParameter { get; set; }

        /// <summary>
        /// 关联列
        /// </summary>
        public List<GridBindColumn> Children
        {
            get
            {
                if (_children == null)
                    _children = new List<GridBindColumn>();
                return _children;
            }
            set { _children = value; }
        }

        /// <summary>
        /// 是否隐藏列
        /// </summary>
        public bool IsHide
        {
            get { return _isHide; }
            set
            {
                if (_isHide != value)
                {
                    _isHide = value;
                    OnPropertyChanged(nameof(IsHide));
                }
            }
        }

        public int CompareTo(GridBindColumn other)
        {
            if (DisplayOrder > other.DisplayOrder)
                return 1;
            if (DisplayOrder < other.DisplayOrder)
                return -1;
            return 0;
        }

        public void SetDisplayIndex(int index)
        {
            DisplayIndex = index;
        }
    }
}
