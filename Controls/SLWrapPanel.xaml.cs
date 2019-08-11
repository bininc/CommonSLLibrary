using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using CommonLibSL.Model;

namespace CommonLibSL.Controls
{
    public partial class SLWrapPanel : WrapPanel
    {

        public event Action<SLCar> SelectedCar;

        public SLWrapPanel()
        {
            InitializeComponent();
        }

        public void ShowSearchResult(SLTextBox txtTarget, Panel addTo, List<SLCar> searchList)
        {
            if (searchList == null || searchList.Count == 0 || txtTarget == null || addTo == null) return;

            Orientation = Orientation.Vertical;
            Margin = new Thickness(5, 30, 0, 0);
            VerticalAlignment = VerticalAlignment.Top;
            HorizontalAlignment = HorizontalAlignment.Left;
            MaxHeight = 400;
            Width = txtTarget.Width;
            SLListBox listBox = null;
            if (Children.Count == 0)
            {
                listBox = new SLListBox();
                listBox.Width = Width;
                listBox.SelectionMode = SelectionMode.Single;
                listBox.MouseLeftButtonUp += (sender, args) =>
                {
                    args.Handled = true;
                    Visibility = Visibility.Collapsed;
                    if (listBox.SelectedItems.Count > 0)
                    {
                        SLCar selcar = (SLCar)listBox.SelectedItems[0];
                        SelectedCar?.Invoke(selcar);
                    }
                };
                Children.Add(listBox);
                txtTarget.MouseLeftButtonDown += (sender, e) =>
                {
                    Visibility = Visibility.Collapsed;
                    searchList.Clear();
                };
            }
            else
            {
                listBox = Children[0] as SLListBox;
            }

            listBox.ItemsSource = searchList;

            Grid.SetRowSpan(this, 3);

            if (!addTo.Children.Contains(this))
                addTo.Children.Add(this);

            Visibility = Visibility.Visible;
        }

    }
}
