using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CommonLibSL.Controls
{
    [DefaultEvent(nameof(MouseLeftButtonDownEvent))]
    public partial class ImageButton : SLUserControl
    {
        #region 自定义外观相关
        private static readonly ImageSource defaultNormalImage =
            new BitmapImage(new Uri("/ImageResource;component/Images/nav_button.png", UriKind.Relative));

        public static readonly DependencyProperty NormalImageProperty = DependencyProperty.Register(
            "NormalImage", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(defaultNormalImage,
                (o, args) =>
                {
                    ImageButton imgButton = (ImageButton)o;
                    imgButton._normalBrush = null;
                    imgButton.UpdateViewStatus();
                }));

        public ImageSource NormalImage
        {
            get { return (ImageSource)GetValue(NormalImageProperty); }
            set { SetValue(NormalImageProperty, value); }
        }

        private static readonly ImageSource defaultOnImage =
            new BitmapImage(new Uri("/ImageResource;component/Images/nav_button__on.png", UriKind.Relative));

        public static readonly DependencyProperty OnImageProperty = DependencyProperty.Register(
            "OnImage", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(defaultOnImage,
                (o, args) =>
                {
                    ImageButton imgButton = (ImageButton)o;
                    imgButton._onBrush = null;
                    imgButton.UpdateViewStatus();
                }));

        public ImageSource OnImage
        {
            get { return (ImageSource)GetValue(OnImageProperty); }
            set { SetValue(OnImageProperty, value); }
        }

        private ImageBrush _normalBrush;
        private ImageBrush NormalBrush
        {
            get
            {
                if (_normalBrush == null)
                    _normalBrush = new ImageBrush() { ImageSource = NormalImage };
                return _normalBrush;
            }
        }

        private ImageBrush _onBrush;

        private ImageBrush OnBrush
        {
            get
            {
                if (_onBrush == null)
                    _onBrush = new ImageBrush() { ImageSource = OnImage };
                return _onBrush;
            }
        }

        public static readonly DependencyProperty NormalTextColorProperty = DependencyProperty.Register(
            "NormalTextColor", typeof(SolidColorBrush), typeof(ImageButton), new PropertyMetadata(new SolidColorBrush(Colors.White),
                (o, args) =>
                {
                    ImageButton imgButton = (ImageButton)o;
                    imgButton.UpdateViewStatus();
                }));

        public SolidColorBrush NormalTextColor
        {
            get { return (SolidColorBrush)GetValue(NormalTextColorProperty); }
            set { SetValue(NormalTextColorProperty, value); }
        }

        public static readonly DependencyProperty OnTextColorProperty = DependencyProperty.Register(
            "OnTextColor", typeof(SolidColorBrush), typeof(ImageButton), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 39, 148, 210)),
                (o, args) =>
                {
                    ImageButton imgButton = (ImageButton)o;
                    imgButton.UpdateViewStatus();
                }));

        public SolidColorBrush OnTextColor
        {
            get { return (SolidColorBrush)GetValue(OnTextColorProperty); }
            set { SetValue(OnTextColorProperty, value); }
        }
        #endregion

        #region 选项模式相关

        public static readonly DependencyProperty SelectedModeProperty = DependencyProperty.Register(
            "SelectedMode", typeof(bool), typeof(ImageButton), new PropertyMetadata(false, (o, args) =>
            {
                ImageButton imgButton = (ImageButton)o;
                imgButton.UpdateViewStatus();
            }));

        public bool SelectedMode
        {
            get { return (bool)GetValue(SelectedModeProperty); }
            set { SetValue(SelectedModeProperty, value); }
        }

        public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register(
            "Selected", typeof(bool), typeof(ImageButton), new PropertyMetadata(default(bool), (o, args) =>
            {
                ImageButton imgButton = (ImageButton)o;
                imgButton.UpdateViewStatus();

                SelectedArgs selectedArgs = new SelectedArgs();
                selectedArgs.OldValue = (bool)args.OldValue;
                selectedArgs.NewValue = (bool)args.NewValue;

                if (selectedArgs.NewValue && imgButton.SelectedMode)
                {
                    if (_currentSelectedButtons.ContainsKey(imgButton.GroupID))
                    {
                        ImageButton oldImgButton = _currentSelectedButtons[imgButton.GroupID];
                        if (oldImgButton != imgButton)
                            oldImgButton.Selected = false;
                        _currentSelectedButtons.Remove(imgButton.GroupID);
                    }
                    _currentSelectedButtons.Add(imgButton.GroupID, imgButton);
                }
                if (imgButton.SelectedChanged != null)
                    imgButton.SelectedChanged(imgButton, selectedArgs);
            }));
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool Selected
        {
            get { return (bool)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }

        public event EventHandler<SelectedArgs> SelectedChanged;

        public static readonly DependencyProperty GroupIDProperty = DependencyProperty.Register(
            "GroupID", typeof(string), typeof(ImageButton), new PropertyMetadata(default(string)));

        public string GroupID
        {
            get { return (string)GetValue(GroupIDProperty); }
            set { SetValue(GroupIDProperty, value); }
        }

        private static Dictionary<string, ImageButton> _currentSelectedButtons = new Dictionary<string, ImageButton>();
        #endregion

        public new bool IsEnabled
        {
            get { return base.IsEnabled; }
            set
            {
                base.IsEnabled = value;
                UpdateViewStatus();
            }
        }


        /// <summary>
        /// 更新界面状态
        /// </summary>
        void UpdateViewStatus()
        {
            if (SelectedMode)
            {
                LayoutRoot.Background = Selected ? OnBrush : NormalBrush;
                lblText.Foreground = Selected ? OnTextColor : NormalTextColor;
            }
            else
            {
                LayoutRoot.Background = IsEnabled ? NormalBrush : OnBrush;
                lblText.Foreground = IsEnabled ? NormalTextColor : new SolidColorBrush(Colors.DarkGray);
            }
        }

        #region 内部文本相关
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
           "Text", typeof(string), typeof(ImageButton), new PropertyMetadata("图片按钮"));
        /// <summary>
        /// 按钮文本
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty ContentVerticalAlignmentProperty = DependencyProperty.Register(
            "ContentVerticalAlignment", typeof(VerticalAlignment), typeof(ImageButton), new PropertyMetadata(VerticalAlignment.Center));

        public VerticalAlignment ContentVerticalAlignment
        {
            get { return (VerticalAlignment)GetValue(ContentVerticalAlignmentProperty); }
            set { SetValue(ContentVerticalAlignmentProperty, value); }
        }

        public static readonly DependencyProperty ContentHorizontalAlignmentProperty = DependencyProperty.Register(
            "ContentHorizontalAlignment", typeof(HorizontalAlignment), typeof(ImageButton), new PropertyMetadata(HorizontalAlignment.Center));

        public HorizontalAlignment ContentHorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(ContentHorizontalAlignmentProperty); }
            set { SetValue(ContentHorizontalAlignmentProperty, value); }
        }

        public static readonly DependencyProperty ContentMarginProperty = DependencyProperty.Register(
            "ContentMargin", typeof(Thickness), typeof(ImageButton), new PropertyMetadata(default(Thickness)));

        public Thickness ContentMargin
        {
            get { return (Thickness)GetValue(ContentMarginProperty); }
            set { SetValue(ContentMarginProperty, value); }
        }
        #endregion

        public ImageButton()
        {
            InitializeComponent();
        }

        protected override void OnLoad()
        {
            UpdateViewStatus();
        }

        #region 事件及外观控制

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (!LayoutRoot.Background.Equals(OnBrush))
                LayoutRoot.Background = OnBrush;
            if (!lblText.Foreground.Equals(OnTextColor))
                lblText.Foreground = OnTextColor;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            UpdateViewStatus();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (SelectedMode && !Selected)
                Selected = true;
            base.OnMouseLeftButtonDown(e);
        }
        #endregion
    }

    public class SelectedArgs : EventArgs
    {
        public bool NewValue { get; set; }
        public bool OldValue { get; set; }
    }
}
