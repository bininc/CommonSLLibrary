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

namespace CommonLibSL.Controls
{
    /// <summary>
    /// 分组框
    /// </summary>
    [TemplatePart(Name = "PART_Header", Type = typeof(FrameworkElement)),
     TemplatePart(Name = "PART_Border", Type = typeof(FrameworkElement))]
    public class GroupBox : HeaderedContentControl
    {
        private FrameworkElement header;
        private FrameworkElement border;

        public GroupBox()
        {
            //this.DefaultStyleKey = typeof(GroupBox);
            Style = Themes.ThemeManger.GetStyle("GroupBoxStyle");
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.header = (base.GetTemplateChild("PART_Header") as FrameworkElement);
            this.border = (base.GetTemplateChild("PART_Border") as FrameworkElement);
            if (this.header != null && this.border != null)
            {
                this.header.Clip = null;
                this.header.SizeChanged += this.GroupBoxSizeChanged;
                this.border.SizeChanged += this.GroupBoxSizeChanged;
            }
        }

        private void GroupBoxSizeChanged(object sender, EventArgs e)
        {
            if (this.header != null && this.border != null)
            {
                var geometryGroup = new GeometryGroup();
                PresentationFrameworkCollection<Geometry> geometrys = geometryGroup.Children;
                var rectangleGeometry = new RectangleGeometry
                {
                    Rect = (new Rect(-1.0, -1.0, this.border.ActualWidth + 2.0, this.border.ActualHeight + 2.0))
                };
                geometrys.Add(rectangleGeometry);
                var rect = new Rect(default(Point), new Point(this.header.ActualWidth, this.header.ActualHeight));
                try
                {
                    GeneralTransform generalTransform = this.header.TransformToVisual(this.border);
                    rect = generalTransform.TransformBounds(rect);
                }
                catch
                {
                }

                PresentationFrameworkCollection<Geometry> geometrys2 = geometryGroup.Children;
                var rectangleGeometry2 = new RectangleGeometry {Rect = rect};
                geometrys2.Add(rectangleGeometry2);
                this.border.Clip = geometryGroup;
            }
        }
    }
}
