using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CommonLibSL
{
    public class ComboBoxColor : ComboBox
    {
        Dictionary<int, Color> int_color = new Dictionary<int, Color>();
        Dictionary<Color, int> color_int = new Dictionary<Color, int>();
        public ComboBoxColor()
            : base()
        {
        }
        public void IniColor()
        {
            this.Items.Clear();
            this.int_color.Clear();
            this.color_int.Clear();

            AddColor(Colors.Black);
            AddColor(Colors.Blue);
            AddColor(Colors.Brown);
            AddColor(Colors.Cyan);
            AddColor(Colors.DarkGray);
            AddColor(Colors.Gray);
            AddColor(Colors.Green);
            AddColor(Colors.LightGray);
            AddColor(Colors.Magenta);
            AddColor(Colors.Orange);
            AddColor(Colors.Purple);
            AddColor(Colors.Red);
            AddColor(Colors.White);
            AddColor(Colors.Yellow);
        }
        private void AddColor(Color clr)
        {
            int_color.Add(this.Items.Count, clr);
            color_int.Add(clr, this.Items.Count);
            ComboBoxItem item = new ComboBoxItem();
            Rectangle rect = new Rectangle();
            rect.Fill = new SolidColorBrush(clr);
            rect.Height = this.ColorHeight;
            rect.Width = this.ColorWidth;
            item.Content = rect;
            this.Items.Add(item);
        }
        public Color SelectColor
        {
            get
            {
                if (int_color.ContainsKey(this.SelectedIndex))
                    return int_color[this.SelectedIndex];
                else
                    return Colors.Blue;
            }
            set
            {
                if (color_int.ContainsKey(value))
                    this.SelectedIndex = color_int[value];
            }
        }

        private double ColorHeight
        {
            get
            {
                return this.Height - 8;
            }
        }
        private double ColorWidth
        {
            get
            {
                return this.Width - 10;
            }
        }
    }
}
