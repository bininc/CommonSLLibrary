using System;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CommLiby;
using System.ComponentModel;

namespace CommonLibSL.Controls
{
    public partial class SLDatePicker : SLUserControl
    {
        HtmlElement divIndicatorName;
        HtmlElement txtIndicatorNameElements;
        private bool _iswindowless;
        public SLDatePicker()
        {
            InitializeComponent();
            if (!DesignerProperties.IsInDesignTool)
            {
                Image.MouseLeftButtonDown += TextBox_MouseLeftButtonDown;
                TextBox.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(TxtBox_MouseLeftButtonDown), true);
                TextBox.TextChanged += TextBox_TextChanged;
            }

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_iswindowless && string.IsNullOrWhiteSpace(txtIndicatorNameElements.GetProperty("value").ToString()))
            {
                txtIndicatorNameElements.SetProperty("value", TextBox.Text);
            }
        }

        private void TxtBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;

            if(!_iswindowless) return;

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
                HtmlPage.Window.Invoke("showDatePicker" + Name);
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
                if (!DatePicker.IsDropDownOpen)
                {
                    DatePicker.IsDropDownOpen = true;
                }
            }
        }

        public static readonly DependencyProperty DateProperty = DependencyProperty.Register(
            "Date", typeof(DateTime), typeof(SLDatePicker), new PropertyMetadata(DateTimeHelper.Now, (o, args) =>
            {
                SLDatePicker dp = (SLDatePicker)o;
                DateTime date = (DateTime)args.NewValue;
                string fstr = date.ToString(dp.DateFormat);
                dp.TextBox.Text = fstr;
            }));

        private string _dateFormat = "yyyy年MM月dd日 ddd";

        public DateTime Date
        {
            get { return (DateTime)GetValue(DateProperty); }
            set { SetValue(DateProperty, value); }
        }

        public string DateFormat
        {
            get { return _dateFormat; }
            set
            {
                _dateFormat = value;
            }
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
                this.DatePicker.Visibility = Visibility.Collapsed;
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

            string jsCode = @"function showDatePicker" + Name + @"(){
                layui.laydate.render({
                    elem:'#" + Name + @"',
                    type:'date',
                    format:'" + DateFormat + @"',
                    value:'" + TextBox.Text + @"',
                    btns:['now','confirm'],
                    done: function(value, date, endDate){
                        slHost.Content.SLDatePicker" + Name + @".HtmlValeChanged(date.year+'/'+date.month+'/'+date.date);
                      }
                });
            };
            showDatePicker" + Name + "();";
            HtmlElement js = HtmlPage.Document.CreateElement("Script");
            js.SetAttribute("type", "text/javascript");
            js.SetProperty("text", jsCode);
            divIndicatorName.AppendChild(js);
            HtmlPage.Document.Body.AppendChild(divIndicatorName);
            txtIndicatorNameElements.AttachEvent("onblur", new EventHandler(onLostFocus));

            HtmlPage.RegisterScriptableObject(nameof(SLDatePicker) + Name, this);  //注册JS可调用
        }

        [ScriptableMember]
        public void HtmlValeChanged(string value)
        {
            DateTime dt = DateTimeHelper.FromString(value, "yyyy/M/d");
            if (Date != dt)
                Date = dt;
            else
                TextBox.Text = dt.ToString(DateFormat);
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
                TextBox.Text = Date.ToString(DateFormat);
        }
    }
}
