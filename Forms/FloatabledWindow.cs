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
using CommonLibSL.Model;
using CommonLibSL.Themes;

namespace CommonLibSL.Forms
{
    public class FloatabledWindow : FloatableWindow
    {

        /// <summary>
        /// OK按钮点击事件
        /// </summary>
        public event Action OkButtonClicked;

        /// <summary>
        /// Cancel按钮点击事件
        /// </summary>
        public event Action CancelBtttonClicked;

        private bool _isLoaded; //是否已经加载过了

        protected SLCar _car; //车辆信息

        public FloatabledWindow()
        {
            Loaded += FloatabledWindow_Loaded;
            FontFamily = new FontFamily("Microsoft YaHei");          
            this.Style = ThemeManger.GetStyle("FloatableWindowStyle");
        }

        private void FloatabledWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                if (_car != null)
                    Title += $" - {_car.LICENSE} [{_car.MAC}]";
                Onload();
                _isLoaded = true;
            }
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


        protected override void OnClosed(EventArgs e)
        {
            if (DialogResult == true)
                if (OkButtonClicked != null)
                    OkButtonClicked();
            if (DialogResult == false || DialogResult == null)
                if (CancelBtttonClicked != null)
                    CancelBtttonClicked();
            base.OnClosed(e);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
            base.OnMouseRightButtonDown(e);
        }
    }
}
