using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CommonLibSL.Controls
{
    public class SLUserControl : UserControl
    {
        private bool _loaded = false;

        public SLUserControl()
        {
            if (!DesignerProperties.IsInDesignTool)
                this.Loaded += SLUserControl_Loaded;
        }

        void SLUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_loaded)
                OnLoad();
            _loaded = true;
        }

        /// <summary>
        /// 第一次加载时执行
        /// </summary>
        protected virtual void OnLoad()
        {

        }
    }
}
