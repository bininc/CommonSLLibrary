using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using CommLiby;

namespace CommonLibSL.Forms
{
    public partial class SLMessageBox : ChildWindow
    {
        #region 旧版弹窗
        //private static SLMessageBox _instance;

        //public static SLMessageBox Instance
        //{
        //    get
        //    {
        //        if (_instance == null)
        //            _instance = new SLMessageBox();
        //        return _instance;
        //    }
        //    private set { _instance = value; }
        //} 
        #endregion

        public static readonly Dictionary<string, Action[]> DicJsEventCallBack = new Dictionary<string, Action[]>();

        public static void Show(string title, string message, MessageBoxIcon messageIcon, MessageBoxButton messageButton, Action okEvent = null, Action cancelEvent = null)
        {
            short icon = (short)messageIcon;
            if (icon < 0) icon = 0;

            StringBuilder btn = new StringBuilder("确定");
            if (messageButton == MessageBoxButton.OKCancel)
                btn.AppendFormat(",{0}", "取消");

            string token = null;
            if (okEvent != null || cancelEvent != null)
            {
                Action[] eventActions = new Action[2];
                if (okEvent != null)
                    eventActions[0] = okEvent;
                if (cancelEvent != null)
                    eventActions[1] = cancelEvent;

                token = Common_liby.GetGuidString();
                DicJsEventCallBack.Add(token, eventActions);
            }

            Common_liby.InvokeMethodOnMainThread(() =>
                HtmlPage.Window.Invoke("lalert", title, message, icon, btn.ToString(), token));

            #region 旧版弹窗

            //if (_instance != null)
            //{
            //    _instance.Close();
            //    _instance = null;
            //}
            //Instance.Title = title;
            //Instance.TextMessage.Text = message;
            //Size fontSize = Common.MeasureString(message, Instance.TextMessage.FontFamily, Instance.TextMessage.FontSize);
            //double width = 10 + 50 + fontSize.Width + 22 + 10;
            //if (width < 177)
            //    width = 177;
            //Instance.Width = width;
            //double height = 28 + 13 + fontSize.Height < 30 ? 30 : fontSize.Height + 13 + 42;
            //if (height < 124)
            //    height = 124;
            //Instance.Height = height;
            //string imgurl = string.Format("/Images/icon_{0}.png", ((int)messageIcon).ToString("D2"));
            //Instance.ImageIcon.Source = new BitmapImage(new Uri(imgurl, UriKind.Relative));
            //if (messageButton == MessageBoxButton.OK)
            //{
            //    Instance.CancelButton.Visibility = Visibility.Collapsed;
            //    Instance.OKButton.Margin = Instance.CancelButton.Margin;
            //}
            //Instance.CancelButton.Focus();

            //Instance.Closed += (x, y) =>
            //{
            //    if (Instance.DialogResult == true)
            //    {
            //        Instance = null;
            //        if (okEvent != null)
            //            okEvent();
            //    }
            //    else
            //    {
            //        Instance = null;
            //        if (cancelEvent != null)
            //            cancelEvent();
            //    }
            //};
            //Instance.Show(); 

            #endregion
        }

        public static void ShowMsg(string message, Action okEvent = null, Action cancelEvent = null)
        {
            Show("提示", message, MessageBoxIcon.Pass, MessageBoxButton.OK, okEvent, cancelEvent);
        }

        public static void ShowWarn(string message, Action okEvent = null, Action cancelEvent = null)
        {
            Show("请注意", message, MessageBoxIcon.Warning, MessageBoxButton.OK, okEvent, cancelEvent);
        }

        public static void ShowError(string message, Action okEvent = null, Action cancelEvent = null)
        {
            Show("错误", message, MessageBoxIcon.Error, MessageBoxButton.OK, okEvent, cancelEvent);
        }

        public static void ShowConfirm(string message, Action okEvent = null, Action cancelEvent = null)
        {
            Show("请确认", message, MessageBoxIcon.Question, MessageBoxButton.OKCancel, okEvent, cancelEvent);
        }

        public static void Msg(string message, MessageBoxIcon messageIcon = MessageBoxIcon.None, int durtime = 3000)
        {
            short icon = (short)messageIcon;
            Common_liby.InvokeMethodOnMainThread(() =>
            {
                HtmlPage.Window.Invoke("msg", message, icon, durtime);
            });
        }

        public static void CloseMsg()
        {
            Common_liby.InvokeMethodOnMainThread(() =>
            {
                HtmlPage.Window.Invoke("closemsg");
            });
        }

        public SLMessageBox()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }

    public enum MessageBoxIcon
    {
        None = -1,
        Warning = 0,
        Pass = 1,
        Error = 2,
        Question = 3,
        Lock = 4,
        Cry = 5,
        Smile = 6,
        Loading = 16
    }
}

