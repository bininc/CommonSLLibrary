using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CommonLibSL.Controls
{
    public partial class LinkButton : SLUserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(ImageSource), typeof(LinkButton), new PropertyMetadata(new BitmapImage(new Uri("/ImageResource;component/Images/tishi_icon.png", UriKind.Relative))));

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(LinkButton), new PropertyMetadata("LinkButton"));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty ImgVisibilityProperty = DependencyProperty.Register(
            "ImgVisibility", typeof(Visibility), typeof(LinkButton), new PropertyMetadata(Visibility.Collapsed));

        private Visibility ImgVisibility
        {
            get { return (Visibility)GetValue(ImgVisibilityProperty); }
            set { SetValue(ImgVisibilityProperty, value); }
        }

        private bool _haveIcon = false;
        public bool HaveIcon
        {
            get { return _haveIcon; }
            set
            {
                _haveIcon = value;
                ImgVisibility = _haveIcon ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public static readonly DependencyProperty DisableAnimationProperty = DependencyProperty.Register(
            "DisableAnimation", typeof(bool), typeof(LinkButton), new PropertyMetadata(default(bool)));

        public bool DisableAnimation
        {
            get { return (bool)GetValue(DisableAnimationProperty); }
            set { SetValue(DisableAnimationProperty, value); }
        }

        public LinkButton()
        {
            InitializeComponent();
        }

        protected override void OnLoad()
        {
            if (HaveIcon && Source != null)
            {
                Img.Source = Source;
                Img.Visibility = Visibility.Visible;
            }
            else
                Img.Visibility = Visibility.Collapsed;
        }

        private readonly Brush _onBackGround = new SolidColorBrush(Colors.LightGray);
        private Brush _tmpBrush = null;

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (!DisableAnimation)
            {
                _tmpBrush = Root.Background;
                Root.Background = _onBackGround;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!DisableAnimation)
                Root.Background = _tmpBrush;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (!DisableAnimation)
                Root.Background = _onBackGround;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (!DisableAnimation)
                Root.Background = _tmpBrush;
        }
    }
}
