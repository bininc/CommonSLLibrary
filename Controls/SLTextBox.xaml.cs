using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace CommonLibSL.Controls
{
    public partial class SLTextBox : SLUserControl
    {
        HtmlElement divIndicatorName;
        HtmlElement txtIndicatorNameElements;
        public delegate void KeyDownHandel(object sender, string keyCode);
        public event KeyDownHandel KeyDownHandelEvent;
        public event TextChangedEventHandler TextChanged;
        private bool _iswindowless;

        public SLTextBox()
        {
            InitializeComponent();
            if (!DesignerProperties.IsInDesignTool)
            {
                txtBox.TextChanged += TxtBox_OnTextChanged;
                txtBox.MouseLeftButtonDown += TxtBox_MouseLeftButtonDown;
            }
        }

        private void TxtBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);
        }

        protected override void OnLoad()
        {
            System.Windows.Interop.SilverlightHost host = Application.Current.Host;
            System.Windows.Interop.Settings setting = host.Settings;
            _iswindowless = setting.Windowless;

            if (_iswindowless)
            {
                CreateHtmlElement();
                this.SizeChanged += new SizeChangedEventHandler(EsmsInput_SizeChanged);
                if (!IsReadOnly)
                {
                    this.txtBox.Visibility = Visibility.Collapsed;
                    this.lblIndicatorName.Visibility = Visibility.Visible;
                }
            }
            this.txtBox.IsReadOnly = IsReadOnly;
        }

        /// <summary>
        /// 当这个控件大小发生了变化，需要重新调整input的大小
        /// </summary>
        void EsmsInput_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            divIndicatorName.SetStyleAttribute("width", e.NewSize.Width - 4 + "px");
            divIndicatorName.SetStyleAttribute("height", e.NewSize.Height + "px");
        }

        /// <summary>
        /// 创建一个input 元素作为输入设备
        /// </summary>
        void CreateHtmlElement()
        {
            divIndicatorName = HtmlPage.Document.CreateElement("div");
            if (!string.IsNullOrWhiteSpace(Name))
                divIndicatorName.Id = Name;
            if (TextWrapping == TextWrapping.Wrap)
            {
                txtIndicatorNameElements = HtmlPage.Document.CreateElement("textarea");
                txtIndicatorNameElements.SetStyleAttribute("overflow", "hidden");
            }
            else
                txtIndicatorNameElements = HtmlPage.Document.CreateElement("input");
            txtIndicatorNameElements.SetStyleAttribute("width", "100%");
            txtIndicatorNameElements.SetStyleAttribute("height", "100%");
            txtIndicatorNameElements.SetStyleAttribute("font-size", FontSize + "px");
            txtIndicatorNameElements.SetStyleAttribute("font-family", FontFamily.ToString());
            divIndicatorName.AppendChild(txtIndicatorNameElements);
            divIndicatorName.SetStyleAttribute("width", ActualWidth-4 + "px");
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
            txtIndicatorNameElements.CssClass = "inputcss";
            HtmlPage.Document.Body.AppendChild(divIndicatorName);
            txtIndicatorNameElements.AttachEvent("onblur", new EventHandler(onLostFocus));

            //注册一个keydown事件用于托管代码中调用
            txtIndicatorNameElements.AttachEvent("onkeydown", new EventHandler(onExecuteQueryByonKeyDown));
            //这是一个用border画的虚假的输入框，当它被点击的时候，显示input元素，并定位到这个border上面
            this.bdInputName.MouseLeftButtonDown += new MouseButtonEventHandler(bdInputName_MouseLeftButtonDown);
        }

        public void onLostFocus(object sender, EventArgs e)
        {
            hideHtmlElementByResize(null, null);
        }

        private void onExecuteQueryByonKeyDown(object sender, EventArgs e)
        {
            if (KeyDownHandelEvent != null)
            {
                string keyCode = HtmlPage.Window.Eval("event.keyCode").ToString();
                KeyDownHandelEvent(this, keyCode);
            }
        }

        private void hideHtmlElementByResize(object sender, EventArgs e)
        {
            divIndicatorName.SetStyleAttribute("display", "none");
            divIndicatorName.SetStyleAttribute("left", string.Format("{0}px", 0));
            divIndicatorName.SetStyleAttribute("top", string.Format("{0}px", 0));

            Text = txtIndicatorNameElements.GetProperty("value").ToString();
            lblIndicatorName.VerticalAlignment = TextWrapping == System.Windows.TextWrapping.Wrap ? VerticalAlignment.Stretch : VerticalAlignment.Center;

            this.lblIndicatorName.Opacity = 1;
            Application.Current.Host.Content.Resized -= new EventHandler(hideHtmlElementByResize);
        }

        void bdInputName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsReadOnly) return;

            Point posRoot = e.GetPosition(null);
            Point posRela = e.GetPosition(this.bdInputName);
            double left = posRoot.X - posRela.X + 4;//+2
            double top = posRoot.Y - posRela.Y + 1;//+2

            divIndicatorName.SetStyleAttribute("display", "block");
            divIndicatorName.SetStyleAttribute("left", string.Format("{0}px", left));
            divIndicatorName.SetStyleAttribute("top", string.Format("{0}px", top));
            divIndicatorName.SetStyleAttribute("position", "absolute");
            txtIndicatorNameElements.SetProperty("value", Text);
            txtIndicatorNameElements.Focus();

            this.lblIndicatorName.Opacity = 0;
            Application.Current.Host.Content.Resized += new EventHandler(hideHtmlElementByResize);
        }


        public double EsmsWidth
        {
            set
            {
                this.bdInputName.Width = value;
                EsmsInputWidth = value;
            }
            get
            {
                return this.bdInputName.Width;
            }
        }
        public double EsmsHeight
        {
            set
            {
                this.bdInputName.Height = value;
                EsmsInputHeight = value;
            }
            get
            {
                return this.bdInputName.Height;
            }
        }
        public double EsmsInputWidth
        {
            set
            {
                if (this.txtIndicatorNameElements != null)
                {
                    this.txtIndicatorNameElements.SetStyleAttribute("width", (value - 4).ToString());
                    this.divIndicatorName.SetStyleAttribute("width", (value - 4).ToString());
                    this.lblIndicatorName.Width = value;
                }
            }
            get
            {
                return this.bdInputName.Width;
            }
        }
        public double EsmsInputHeight
        {
            set
            {

                if (this.txtIndicatorNameElements != null)
                {
                    this.txtIndicatorNameElements.SetStyleAttribute("height", (value - 4).ToString());
                    this.divIndicatorName.SetStyleAttribute("height", (value - 4).ToString());
                }
            }
            get
            {
                return this.bdInputName.Height;
            }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(SLTextBox), new PropertyMetadata(string.Empty, (o, args) =>
            {
                SLTextBox input = (SLTextBox)o;
                string value = (string)args.NewValue;

                if (input.txtIndicatorNameElements != null)
                {
                    input.txtIndicatorNameElements.SetAttribute("value", value ?? "");
                }
                if (value == null)
                {
                    input.Text = string.Empty;
                }

                if (input.TextChanged != null)
                {
                    input.TextChanged(input, null);
                }
            }));

        public string Text
        {
            get
            {
                string val = (string)GetValue(TextProperty);
                if (val == null)
                {
                    val = string.Empty;
                }
                return val;
            }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            "IsReadOnly", typeof(bool), typeof(SLTextBox), new PropertyMetadata(default(bool), (o, args) =>
            {
                bool value = (bool)args.NewValue;
                SLTextBox input = (SLTextBox)o;
                if (input._iswindowless)
                {
                    if (!value)
                    {
                        input.txtBox.Visibility = Visibility.Collapsed;
                        input.lblIndicatorName.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        input.txtBox.Visibility = Visibility.Visible;
                        input.lblIndicatorName.Visibility = Visibility.Collapsed;
                    }
                }
                input.IsReadOnly = value;
            }));

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty IsShowBorderThicknessProperty = DependencyProperty.Register(
            "IsShowBorderThickness", typeof(bool), typeof(SLTextBox), new PropertyMetadata(true,
                (o, args) =>
                {
                    SLTextBox input = (SLTextBox)o;
                    bool show = (bool)args.NewValue;
                    input.bdInputName.BorderThickness = show ? new Thickness(1) : new Thickness(0);
                }));

        public bool IsShowBorderThickness
        {
            get { return (bool)GetValue(IsShowBorderThicknessProperty); }
            set { SetValue(IsShowBorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
            "TextWrapping", typeof(TextWrapping), typeof(SLTextBox), new PropertyMetadata(TextWrapping.NoWrap));

        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        public new static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background", typeof(Brush), typeof(SLTextBox), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        private void TxtBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = sender as TextBox;

            var bindingExpression = txt.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpression != null)
            {
                bindingExpression.UpdateSource();
            }
        }
    }
}
