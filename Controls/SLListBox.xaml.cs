using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CommonLibSL.Controls
{
    public partial class SLListBox : SLUserControl
    {
        public event SelectionChangedEventHandler SelectionChanged;
        public SLListBox()
        {
            InitializeComponent();
            LayoutRoot.SelectionChanged += SelectionChanged;
        }

        public SelectionMode SelectionMode
        {
            get { return LayoutRoot.SelectionMode; }
            set { LayoutRoot.SelectionMode = value; }
        }

        public IList SelectedItems
        {
            get { return LayoutRoot.SelectedItems; }
        }

        public IEnumerable ItemsSource
        {
            get { return LayoutRoot.ItemsSource; }
            set { LayoutRoot.ItemsSource = value; }
        }

        //
        // 摘要:
        //     获取用于生成控件内容的集合。
        //
        // 返回结果:
        //     如果存在用于生成控件内容的集合，则为该集合；否则为 null。默认值为空集合。
        public ItemCollection Items { get { return LayoutRoot.Items; } }

    }
}
