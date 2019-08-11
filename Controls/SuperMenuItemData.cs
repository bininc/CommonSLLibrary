using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using CommLiby;

namespace CommonLibSL.Controls
{
    public class SuperMenuItemData : ClassBase_Notify<SuperMenuItemData>
    {
        string name;
        /// <summary>
        /// 菜单名称
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (this.name == value)
                {
                    return;
                }

                this.name = value;
                OnPropertyChanged(nameof(this.Name));
            }
        }
        /// <summary>
        /// 菜单类型
        /// </summary>
        public MenuItemType ItemType { get; set; }
        /// <summary>
        /// 菜单点击事件
        /// </summary>
        public RoutedEventHandler MenuItemClick;
        /// <summary>
        /// 没有权限的用户组
        /// </summary>
        public int[] NoAccessUserGroups { get; set; }

        /// <summary>
        /// 子菜单项
        /// </summary>
        public SuperMenuItemData[] Children { get; set; }

        /// <summary>
        /// 标签标记
        /// </summary>
        public string Tag { get; set; }


        bool enable = true;
        /// <summary>
        /// 是否启动
        /// </summary>
        public bool Enable
        {
            get
            {
                return this.enable;
            }
            set
            {
                if (this.enable == value)
                {
                    return;
                }

                this.enable = value;
                OnPropertyChanged(nameof(this.Enable));
            }
        }


        Visibility visibility = Visibility.Visible;
        /// <summary>
        /// 是否可见
        /// </summary>
        public Visibility Visibility
        {
            get
            {
                return this.visibility;
            }
            set
            {
                if (this.visibility == value)
                {
                    return;
                }

                this.visibility = value;
                OnPropertyChanged(nameof(this.Visibility));
            }
        }

        public SuperMenuItemData()
        {

        }

        public SuperMenuItemData(string name, RoutedEventHandler menuItemClick, params int[] noAccessGroups)
        {
            Name = name;
            MenuItemClick = menuItemClick;
            NoAccessUserGroups = noAccessGroups;
        }

    }
    public enum MenuItemType
    {
        MenuItem,
        Separator
    }
}
