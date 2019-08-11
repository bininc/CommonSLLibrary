using System;
using System.Globalization;
using System.Windows.Data;
using CommLiby;

namespace CommonLibSL.Themes.ValueConverter
{
    public class DateTimeDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //下面可以自行把value格式化成你想要的格式  
            //以下是我把传进来的值按照日期类型格式化的 
            if (value is DateTime)
            {
                if (parameter == null)
                    parameter = "yyyy-MM-dd HH:mm:ss";
                DateTime dt = (DateTime)value;
                
                if (!dt.IsValid())
                    return null;
                else
                    return dt.ToString(parameter.ToString());
            }
            else
            {
                return "转换类型错误";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
