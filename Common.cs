using Newtonsoft.Json;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CommonLibSL
{
    public class Common
    {
        /// <summary>
        /// 计算字符串尺寸
        /// </summary>
        /// <param name="s"></param>
        /// <param name="fontFamily"></param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        public static Size MeasureString(string s, FontFamily fontFamily, double fontSize, double maxWidth = 260)
        {

            if (String.IsNullOrEmpty(s))
            {
                return new Size(0, 0);
            }

            var TextBlock = new TextBlock()
            {
                Text = s
            };
            TextBlock.FontFamily = fontFamily;
            TextBlock.FontSize = fontSize;
            TextBlock.MaxWidth = maxWidth;

            return new Size(TextBlock.ActualWidth, TextBlock.ActualHeight);
        }

        /// <summary>
        /// 度分秒经纬度转换为度
        /// </summary>
        /// <param name="d"></param>
        /// <param name="f"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double ConvertToDu(float d, float f, float m)
        {
            f = f + m / 60;
            var du = f / 60 + d;
            return du;
        }

        public static readonly string Chchars = "。？！，、；：‘’“”（）【】《》";
        public static readonly string Enchars = ".?!,,;:''\"\"()[]<>";

        /// <summary> 转半角的函数(DBC case) </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>半角字符串</returns>
        ///<remarks>
        ///全角空格为12288，半角空格为32
        ///其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        ///</remarks>
        public static string ToDBC(string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }

                int index = Chchars.IndexOf(c[i]);
                if (index >= 0)
                {
                    c[i] = Enchars[index];
                    continue;
                }

                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }
        /// 转全角的函数(SBC case)
        ///
        ///任意字符串
        ///全角字符串
        ///
        ///全角空格为12288，半角空格为32
        ///其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        ///
        public static string ToSBC(string input)
        {
            // 半角转全角：
            char[] array = input.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == 32)
                {
                    array[i] = (char)12288;
                    continue;
                }
                if (array[i] < 127)
                {
                    array[i] = (char)(array[i] + 65248);
                }
            }
            return new string(array);
        }

        #region 控件相关
        /// <summary>
        /// ComBox索引移动到下一位
        /// </summary>
        /// <param name="cmbBox"></param>
        public static void SelectedNextItem(ComboBox cmbBox)
        {
            if (cmbBox == null) return;
            if (cmbBox.SelectedIndex == cmbBox.Items.Count - 1) //最后一条
                cmbBox.SelectedIndex = 0;
            else
                cmbBox.SelectedIndex++;
        }
        #endregion
        /// <summary>
        /// 只是保证dll能够引用
        /// </summary>
        public static void ResUsing()
        {
            ImageResource.ResUsing.Using();
        }
    }
}
