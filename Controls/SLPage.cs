using System;
using System.Windows.Controls;

namespace CommonLibSL.Controls
{
    public class SLPage : SLUserControl, IPage
    {
        public static event Action<SLPage, SLPage> Navigated;
        public virtual void NavigatedFrom(SLPage fromPage, object param)
        {
            Navigated?.Invoke(this, fromPage);
        }
    }
}
