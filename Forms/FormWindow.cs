using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommonLibSL.Model;
using CommonLibSL.Themes;

namespace CommonLibSL.Forms
{
    public class FormWindow : ChildWindow
    {
        /// <summary>
        /// OK按钮点击事件
        /// </summary>
        public event Action OkButtonClicked;
        /// <summary>
        /// Cancel按钮点击事件
        /// </summary>
        public event Action CancelBtttonClicked;

        private bool _isLoaded;  //是否已经加载过了
        private string _oigTitle; //原标题

        protected SLCar _car; //车辆信息

        public FormWindow() : base()
        {
            Closed += FormWindow_Closed;
            Loaded += FormWindow_Loaded;
            FontFamily = new FontFamily("Microsoft YaHei");
            this.Style = ThemeManger.GetStyle("ChildWindowStyle");
        }

        void FormWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {               
                Onload();
                _oigTitle = Title.ToString();
                _isLoaded = true;
            }

            if (_car != null)
                Title = $"{_oigTitle} - {_car.LICENSE} [{_car.MAC}]";
            else
                Title = _oigTitle;
            OnShown();
        }

        /// <summary>
        /// 第一次加载
        /// </summary>
        protected virtual void Onload()
        {

        }

        /// <summary>
        /// 窗口每次显示时
        /// </summary>
        protected virtual void OnShown()
        {

        }

        void FormWindow_Closed(object sender, EventArgs e)
        {
            if (DialogResult == true)
                if (OkButtonClicked != null)
                    OkButtonClicked();
            if (DialogResult == false || DialogResult == null)
                if (CancelBtttonClicked != null)
                    CancelBtttonClicked();
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
            base.OnMouseRightButtonDown(e);
        }
    }
}
