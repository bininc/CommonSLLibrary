using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Overlays;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Maps.MapControl.Navigation;

namespace CommonLibSL.Map
{
    public class OfflineMap : Microsoft.Maps.MapControl.Map
    {
        public OfflineMap()
            : base()
        {
            this.LoadingError += (sender, e) =>
            {
                //移除 Unable to contact server 的错误消息 
                try
                {
                    (VisualTreeHelper.GetChild(this, 0) as MapLayer).Children.Remove(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this, 0), 5) as LoadingErrorMessage);
                }
                catch { }
            };
            base.LoadingError += (sender, e) =>
            {
                base.RootLayer.Children.RemoveAt(5);
            };
            this.CredentialsProvider = new ApplicationIdCredentialsProvider { ApplicationId = "AlQcYtZfsGz-9ypuUBot2EbVFUqgPnjh37H87MSgtLLhFY_BDbAJ-anvFv1yAYvP" };

            MouseWheelTool.Instance.MouseWheel += InvokeZoomLevel;
        }

        //public override event EventHandler<LoadingErrorEventArgs> LoadingError;

        #region 鼠标滚轮Bug修复(包括在Chrome浏览器下)
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

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            Focused = true;
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            Focused = true;
        }

        private void InvokeZoomLevel(int mouseDelta)
        {
            if (!Focused || mouseDelta == 0) return;

            bool zoomIn = mouseDelta > 0;
            int count = Math.Abs(mouseDelta) / 120;

            ZoomMapCommand cmd = new ZoomMapCommand(zoomIn);
            for (int i = 0; i < count; i++)
            {
                cmd.Execute(this);
            }
        }
    }
    #endregion
}

