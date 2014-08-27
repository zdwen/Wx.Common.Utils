using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Wx.Common.Utils
{
    public static class WxUtils
    {
        /// <summary>
        /// 【闻祖东 2014-8-26-152817】对列表对象进行计数。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int Count<T>(IList<T> list)
        {
            return list == null ? 0 : list.Count;
        }
    }
}
