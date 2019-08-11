using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Data;
using System.Windows.Input;
using CommLiby;

namespace CommonLibSL.Controls
{
    public partial class SLTimePicker : SLUserControl
    {
        HtmlElement divIndicatorName;
        HtmlElement txtIndicatorNameElements;
        private bool _iswindowless;
        public SLTimePicker()
        {
            InitializeComponent();
            if (!DesignerProperties.IsInDesignTool)
            {
                TextBox.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(TxtBox_MouseLeftButtonDown), true);
                TextBox.TextChanged += TextBox_TextChanged;
                Image.MouseLeftButtonDown += TextBox_MouseLeftButtonDown;
            }
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_iswindowless && string.IsNullOrWhiteSpace(txtIndicatorNameElements.GetProperty("value").ToString()))
            {
                txtIndicatorNameElements.SetProperty("value", TextBox.Text);
            }
        }

        private void TxtBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;

            if (!string.IsNullOrWhiteSpace(txtIndicatorNameElements.GetProperty("value").ToString()))
                TextBox.Text = "";

            System.Windows.Point posRoot = e.GetPosition(null);
            System.Windows.Point posRela = e.GetPosition(this.LayoutRoot);
            double left = posRoot.X - posRela.X + 4;//+2
            double top = posRoot.Y - posRela.Y + 1;//+2

            divIndicatorName.SetStyleAttribute("display", "block");
            divIndicatorName.SetStyleAttribute("left", string.Format("{0}px", left));
            divIndicatorName.SetStyleAttribute("top", string.Format("{0}px", top));
            divIndicatorName.SetStyleAttribute("position", "absolute");
            txtIndicatorNameElements.Focus();

            string browser = HtmlPage.Document.GetElementById("browser").GetProperty("value").ToString();
            double version = Convert.ToDouble(HtmlPage.Document.GetElementById("version").GetProperty("value"));
            if (browser == "IE" && version <= 8)
            {
                HtmlPage.Window.Invoke("showTimePicker" + Name);
            }
        }
        void TextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_iswindowless)
            {
                TxtBox_MouseLeftButtonDown(TextBox, e);
            }
            else
            {
                if (!TimePicker.IsDropDownOpen)
                    TimePicker.IsDropDownOpen = true;
            }
        }

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
            "Time", typeof(DateTime), typeof(SLTimePicker), new PropertyMetadata(default(DateTime), (o, args) =>
            {
                SLTimePicker tp = (SLTimePicker)o;
                DateTime date = (DateTime)args.NewValue;
                tp.TextBox.Text = date.ToString(tp.TimeFormat);
            }));

        private string _timeFormat = "tt hh:mm:ss";

        public DateTime Time
        {
            get { return (DateTime)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public string TimeFormat
        {
            get { return _timeFormat; }
            set { _timeFormat = value; }
        }

        protected override void OnLoad()
        {
            System.Windows.Interop.SilverlightHost host = Application.Current.Host;
            System.Windows.Interop.Settings setting = host.Settings;
            _iswindowless = setting.Windowless;

            if (_iswindowless)
            {
                CreateHtmlElement();
                this.SizeChanged += SLDatePicker_SizeChanged;
                this.TimePicker.Visibility = Visibility.Collapsed;
            }
            else
            {
                TextBox.MouseLeftButtonDown += TextBox_MouseLeftButtonDown;
            }
        }

        private void SLDatePicker_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            divIndicatorName.SetStyleAttribute("width", e.NewSize.Width - 40 + "px");
            divIndicatorName.SetStyleAttribute("height", e.NewSize.Height + "px");
        }

        /// <summary>
        /// 创建一个input 元素作为输入设备
        /// </summary>
        void CreateHtmlElement()
        {
            divIndicatorName = HtmlPage.Document.CreateElement("div");

            txtIndicatorNameElements = HtmlPage.Document.CreateElement("input");
            txtIndicatorNameElements.SetStyleAttribute("width", "100%");
            txtIndicatorNameElements.SetStyleAttribute("height", "100%");
            txtIndicatorNameElements.SetStyleAttribute("font-size", FontSize + "px");
            txtIndicatorNameElements.SetStyleAttribute("font-family", FontFamily.ToString());
            txtIndicatorNameElements.CssClass = "inputcss";
            if (!string.IsNullOrWhiteSpace(Name))
            {
                divIndicatorName.Id = "div_" + Name;
                txtIndicatorNameElements.Id = Name;
            }

            divIndicatorName.AppendChild(txtIndicatorNameElements);
            divIndicatorName.SetStyleAttribute("width", ActualWidth - 40 + "px");
            divIndicatorName.SetStyleAttribute("height", ActualHeight + "px");
            divIndicatorName.SetStyleAttribute("display", "none");
            divIndicatorName.SetStyleAttribute("position", "absolute");
            divIndicatorName.SetStyleAttribute("z-index", "10000");
            divIndicatorName.SetStyleAttribute("left", string.Format("{0}px", 0));
            divIndicatorName.SetStyleAttribute("top", string.Format("{0}px", 0));
            //这个样式必须放在html中，动态生成的样式不起作用，原因不明
            divIndicatorName.CssClass = "divInputcss";
            txtIndicatorNameElements.SetStyleAttribute("background-color", "Transparent");
            //这个样式必须放在html中，动态生成的样式不起作用，原因不明

            string jsCode = @"function showTimePicker" + Name + @"(){
                layui.laydate.render({
                    elem:'#" + Name + @"',
                    type:'time',
                    format:'" + TimeFormat + @"',
                    value:'" + TextBox.Text + @"',
                    btns:['now','confirm'],
                    done: function(value, date, endDate){
                        var timeStr=date.year+'/'+date.month+'/'+date.date+' '+date.hours+':'+date.minutes+':'+date.seconds;
                        slHost.Content.SLTimePicker" + Name + @".HtmlValeChanged(timeStr);
                      }
                });
            };
            showTimePicker" + Name + "();";
            HtmlElement js = HtmlPage.Document.CreateElement("Script");
            js.SetAttribute("type", "text/javascript");
            js.SetProperty("text", jsCode);
            divIndicatorName.AppendChild(js);
            HtmlPage.Document.Body.AppendChild(divIndicatorName);
            txtIndicatorNameElements.AttachEvent("onblur", new EventHandler(onLostFocus));

            HtmlPage.RegisterScriptableObject(nameof(SLTimePicker) + Name, this);  //注册JS可调用
        }

        [ScriptableMember]
        public void HtmlValeChanged(string value)
        {
            DateTime dt = DateTimeHelper.FromString(value, "yyyy/M/d H:m:s");
            if (Time != dt)
                Time = dt;
            else
                TextBox.Text = dt.ToString(TimeFormat);
        }

        public void onLostFocus(object sender, EventArgs e)
        {
            hideHtmlElementByResize(null, null);
        }

        private void hideHtmlElementByResize(object sender, EventArgs e)
        {
            divIndicatorName.SetStyleAttribute("display", "none");
            divIndicatorName.SetStyleAttribute("left", string.Format("{0}px", 0));
            divIndicatorName.SetStyleAttribute("top", string.Format("{0}px", 0));
            if (TextBox.Text == "")
                TextBox.Text = Time.ToString(TimeFormat);
        }
    }
}
