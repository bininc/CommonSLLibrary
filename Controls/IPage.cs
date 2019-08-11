using System.Windows.Controls;

namespace CommonLibSL.Controls
{
    /// <summary>
    /// Page类接口
    /// </summary>
    public interface IPage
    {
        /// <summary>
        /// 从哪里导航而来
        /// </summary>
        /// <param name="fromPage"></param>
        /// <param name="param"></param>
        void NavigatedFrom(SLPage fromPage, object param);
    }
}
