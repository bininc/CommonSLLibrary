using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CommLiby;
using System.Linq;
using CrossAppLib;
using Point = System.Windows.Point;

namespace CommonLibSL.Controls
{
    public class SuperMenu
    {
        public readonly ContextMenu Menu = new ContextMenu();
        private List<SuperMenuItemData> _listItems;
        public SuperMenu()
        {
            Menu.Opened += Menu_Opened;
        }

        public SuperMenu(ICollection<SuperMenuItem> items) : this()
        {
            if (items != null)
            {
                foreach (SuperMenuItem item in items)
                {
                    Menu.Items.Add(item);
                }
            }
        }

        public SuperMenu(List<SuperMenuItemData> dicItems) : this()
        {
            if (dicItems != null)
            {
                _listItems = dicItems;
                foreach (var dicItem in dicItems)
                {
                    CreateSuperMenuItem(dicItem, Menu);
                }
            }
        }

        private void CreateSuperMenuItem(SuperMenuItemData data, ItemsControl parent)
        {
            if (data.Children != null)
            {
                SuperMenuItem groupMenuItem = new SuperMenuItem();
                groupMenuItem.SetBinding(SuperMenuItem.HeaderProperty, new Binding(nameof(data.Name)) { Mode = BindingMode.TwoWay });
                groupMenuItem.SetBinding(SuperMenuItem.VisibilityProperty, new Binding(nameof(data.Visibility)) { Mode = BindingMode.TwoWay });
                groupMenuItem.SetBinding(SuperMenuItem.IsEnabledProperty, new Binding(nameof(data.Enable)) { Mode = BindingMode.TwoWay });
                groupMenuItem.DataContext = data;
                parent.Items.Add(groupMenuItem);

                foreach (var item in data.Children)
                {
                    CreateSuperMenuItem(item, groupMenuItem);
                }
            }
            else if (data.ItemType == MenuItemType.Separator)
            {
                Separator sp = new Separator();
                sp.SetBinding(Separator.VisibilityProperty, new Binding(nameof(data.Visibility)) { Mode = BindingMode.TwoWay });
                sp.SetBinding(Separator.IsEnabledProperty, new Binding(nameof(data.Enable)) { Mode = BindingMode.TwoWay });
                sp.DataContext = data;
                parent.Items.Add(sp);
            }
            else
            {
                SuperMenuItem item = new SuperMenuItem();
                item.SetBinding(SuperMenuItem.HeaderProperty, new Binding(nameof(data.Name)) { Mode = BindingMode.TwoWay });
                item.SetBinding(SuperMenuItem.VisibilityProperty, new Binding(nameof(data.Visibility)) { Mode = BindingMode.TwoWay });
                item.SetBinding(SuperMenuItem.IsEnabledProperty, new Binding(nameof(data.Enable)) { Mode = BindingMode.TwoWay });
                item.Click += data.MenuItemClick;
                item.DataContext = data;
                parent.Items.Add(item);
            }
        }

        public List<SuperMenuItemData> Sources
        {
            get { return _listItems; }
            set
            {
                if (value != null)
                {
                    _listItems = value;
                    Menu.Items.Clear();
                    foreach (var dicItem in _listItems)
                    {
                        CreateSuperMenuItem(dicItem, Menu);
                    }
                }
            }
        }

        private void Menu_Opened(object sender, RoutedEventArgs e)
        {
            CheckAccess();
        }

        public void CheckAccess()
        {
            if (_listItems == null) return;
            foreach (var item in _listItems)
            {
                CheckItemAccess(item);
            }
        }

        public void CheckItemAccess(SuperMenuItemData item)
        {
            if (item.NoAccessUserGroups != null && item.NoAccessUserGroups.Any(i => i == IApp.loginUser.GROUPID))
            {
                item.Visibility = Visibility.Collapsed;
            }
            else
            {
                item.Visibility = Visibility.Visible;
            }

            if (item.Children != null)
            {
                foreach (var citem in item.Children)
                {
                    CheckItemAccess(citem);
                }
            }
        }

        /// <summary>
        /// 注册单击菜单
        /// </summary>
        /// <param name="element"></param>
        public void ReisterToControl(FrameworkElement element)
        {
            if (element != null)
            {
                element.MouseLeftButtonDown += element_MouseLeftButtonDown;
            }
        }

        void element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement btnCtrl = (FrameworkElement)sender;
            Point p = e.GetPosition(btnCtrl);
            Menu.HorizontalOffset = -p.X;
            Menu.VerticalOffset = btnCtrl.ActualHeight - p.Y;
            Menu.IsOpen = true;
        }

        /// <summary>
        /// 注册右键菜单
        /// </summary>
        /// <param name="element"></param>
        public void RegisterToRightControl(FrameworkElement element)
        {
            if (element != null)
            {
                element.MouseRightButtonDown += Element_MouseRightButtonDown;
                //ContextMenuService.SetContextMenu(element, Menu);
            }
        }

        private void Element_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Menu.IsOpen = true;
        }
    }
}
