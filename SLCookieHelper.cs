using System;
using System.Windows.Browser;

//引入此空间以便于使用Cookie操作


namespace CommonLibSL

{

    public class SLCookieHelper

    {
        #region Cookie相关操作函数


        #region 设置持久时间长的Cookie

        /**//// <summary>

        /// 设置持久时间长的Cookie

        /// </summary>

        /// <param name="key">the cookie key</param>

        /// <param name="value">the cookie value</param>

        public static void SetCookie(string key, string value)

        {
            string oldCookie = HtmlPage.Document.GetProperty("cookie") as String;

            DateTime expiration = DateTime.UtcNow + TimeSpan.FromDays(2000);

            string cookie = String.Format("{0}={1};expires={2}", key, value, expiration.ToString("R"));

            HtmlPage.Document.SetProperty("cookie", cookie);
        }

        #endregion


        #region 读取一个已经存在的Cookie

        /**//// <summary>

        /// 读取一个已经存在的Cookie

        /// </summary>

        /// <param name="key">cookie key</param>

        /// <returns>null if the cookie does not exist, otherwise the cookie value</returns>

        public static string GetCookie(string key)

        {

            string[] cookies = HtmlPage.Document.Cookies.Split(';');

            key += '=';

            foreach (string cookie in cookies)

            {

                string cookieStr = cookie.Trim();

                if (cookieStr.StartsWith(key, StringComparison.OrdinalIgnoreCase))

                {

                    string[] vals = cookieStr.Split('=');

                    if (vals.Length >= 2)

                    {

                        return vals[1];

                    }

                    return string.Empty;

                }

            }



            return null;

        }

        #endregion


        #region   删除特定的Cookie(清空它的Value值，过期值设置为-1天)

        /**//// <summary>

        /// 删除特定的Cookie(清空它的Value值，过期值设置为-1天)

        /// </summary>

        /// <param name="key">the cookie key to delete</param>



        public static void DeleteCookie(string key)

        {

            string oldCookie = HtmlPage.Document.GetProperty("cookie") as String;

            DateTime expiration = DateTime.UtcNow - TimeSpan.FromDays(1);

            string cookie = String.Format("{0}=;expires={1}", key, expiration.ToString("R"));

            HtmlPage.Document.SetProperty("cookie", cookie);

        }

        #endregion



        #region 判定指定的key-value对是否在cookie中存在

        public static bool Exists(String key, String value)

        {

            return HtmlPage.Document.Cookies.Contains(

                String.Format("{0}={1}", key, value)

                );

        }

        #endregion



        #region  获取当前cookie内容

        public static string getCookieContent()

        {

            return HtmlPage.Document.GetProperty("cookie") as String;

        }

        #endregion



        #endregion

    }

}