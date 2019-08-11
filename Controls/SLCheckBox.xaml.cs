using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CommonLibSL.Controls
{
    public partial class SLCheckBox : SLUserControl
    {
        public static readonly ImageSource defaultCheckImg =
            new BitmapImage(new Uri("/ImageResource;component/Images/fuxuankuang_onT.png", UriKind.Relative));

        public static readonly ImageSource defaultUnCheckImg =
            new BitmapImage(new Uri("/ImageResource;component/Images/fuxuankuangT.png", UriKind.Relative));


        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(SLCheckBox), new PropertyMetadata("SLCheckBox"));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty CheckImgProperty = DependencyProperty.Register(
             "CheckImg", typeof(ImageSource), typeof(SLCheckBox), new PropertyMetadata(defaultCheckImg));

        public static readonly DependencyProperty UnCheckImgProperty = DependencyProperty.Register(
            "UnCheckImg", typeof(ImageSource), typeof(SLCheckBox), new PropertyMetadata(defaultUnCheckImg));

        public ImageSource CheckImg
        {
            get { return (ImageSource)GetValue(CheckImgProperty); }
            set { SetValue(CheckImgProperty, value); }
        }

        public ImageSource UnCheckImg
        {
            get { return (ImageSource)GetValue(UnCheckImgProperty); }
            set { SetValue(UnCheckImgProperty, value); }
        }

        public static readonly DependencyProperty ExtImgProperty = DependencyProperty.Register("ExtImg", typeof(ImageSource), typeof(SLCheckBox), new PropertyMetadata(null,
            (x, y) =>
            {
                SLCheckBox chb = (SLCheckBox)x;
                if (!y.NewValue.Equals(y.OldValue))
                    chb.UpdateViewStatus();
            }));

        public ImageSource ExtImg
        {
            get { return (BitmapImage)GetValue(ExtImgProperty); }
            set { SetValue(ExtImgProperty, value); }
        }

        public event EventHandler CheckedChanged;

        public static readonly DependencyProperty CheckedProperty = DependencyProperty.Register(
            "Checked", typeof(bool), typeof(SLCheckBox), new PropertyMetadata(false,
                (x, y) =>
                {
                    SLCheckBox chb = (SLCheckBox)x;
                    chb.UpdateViewStatus();
                }));

        public bool Checked
        {
            get { return (bool)GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }

        public static readonly DependencyProperty SpecialModeProperty = DependencyProperty.Register(
            "TriggerInCheckBox", typeof(bool), typeof(SLCheckBox), new PropertyMetadata(false));

        /// <summary>
        /// 在复选框中触发选中事件（默认整体触发）
        /// </summary>
        public bool TriggerInCheckBox
        {
            get { return (bool)GetValue(SpecialModeProperty); }
            set { SetValue(SpecialModeProperty, value); }
        }

        public static readonly DependencyProperty EnableProperty = DependencyProperty.Register(
            "Enable", typeof(bool), typeof(SLCheckBox), new PropertyMetadata(true));

        public bool Enable
        {
            get { return (bool)GetValue(EnableProperty); }
            set { SetValue(EnableProperty, value); }
        }

        public SLCheckBox()
        {
            InitializeComponent();

            ImgCheck.MouseLeftButtonDown += ImgCheck_MouseLeftButtonDown;
            ImgUnCheck.MouseLeftButtonDown += ImgUnCheck_MouseLeftButtonDown;
        }

        void ImgUnCheck_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Enable) return;
            if (TriggerInCheckBox)
            {
                Checked = true;
                if (CheckedChanged != null)
                    CheckedChanged(this, new EventArgs());
                e.Handled = true;
            }
        }

        void ImgCheck_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Enable) return;
            if (TriggerInCheckBox)
            {
                Checked = false;
                if (CheckedChanged != null)
                    CheckedChanged(this, new EventArgs());
                e.Handled = true;
            }
        }

        protected override void OnLoad()
        {
            UpdateViewStatus();
        }

        void UpdateViewStatus()
        {
            if (CheckImg == null || UnCheckImg == null)
            {
                ImgCheck.Visibility = Visibility.Collapsed;
                ImgUnCheck.Visibility = Visibility.Collapsed;
            }
            else
            {
                ImgCheck.Visibility = Checked ? Visibility.Visible : Visibility.Collapsed;
                ImgUnCheck.Visibility = Checked ? Visibility.Collapsed : Visibility.Visible;
            }
            ImgExt.Visibility = ExtImg == null ? Visibility.Collapsed : Visibility.Visible;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!Enable) return;
            e.Handled = false;
            base.OnMouseLeftButtonDown(e);

            if (!TriggerInCheckBox)
            {
                Checked = !Checked;
                if (CheckedChanged != null)
                {
                    CheckedChanged(this, new EventArgs());
                }
            }
        }
    }
}
