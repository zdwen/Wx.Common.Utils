using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Net;
using System.IO.Compression;

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

        /// <summary>
        /// 【闻祖东 2014-8-27-174439】计算一个对象所占用内存大小。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static long CalcObjectSize(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, obj);
                return ms.Length;
            }
        }

        public static string MD5Crypt(Encoding encoding, string str4Crypt)
        {
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                byte[] bts = md5.ComputeHash(encoding.GetBytes(str4Crypt));
                return BitConverter.ToString(bts).Replace("-", string.Empty).ToLower();
            }
        }

        public static void Swap(ref string str1, ref string str2)
        {
            string sTemp = str1;
            str1 = str2;
            str2 = sTemp;
        }

        /// <summary>
        /// 【闻祖东 2012-2-18-233326】将与枚举值字符串相同的字符串转换为相应的枚举值，不区分大小写。
        /// 注意泛型T一定需要是一个枚举值类型，否则会在运行时抛出自定义异常。
        /// </summary>
        /// <typeparam name="T">实际的枚举类型</typeparam>
        /// <param name="strValue">字符串的值</param>
        /// <param name="defaultValue">无法匹配时所取的默认值</param>
        /// <returns>匹配的枚举值</returns>
        public static T Convert2Enum<T>(object obj, T defaultValue) where T : struct, IComparable, IFormattable, IConvertible
        {
            ///【闻祖东 2012-2-18-233447】这里实在是没有办法在声明泛型T的时候约束他一定要继承自System.Enum，至少在.NET 4.0里面都还是不行的，不只是CRC遇到这个问题，
            ///网上很多帖子也是关于这个的，但是只能在运行时抛出异常，而不能在编译时控制编译。
            ///http://stackoverflow.com/questions/79126/create-generic-method-constraining-t-to-an-enum
            ///http://code.google.com/p/unconstrained-melody/downloads/list

            if (typeof(T).IsEnum)
            {
                if (obj == null)
                    return defaultValue;

                T t = defaultValue;
                string val = obj.ToString();

                return Enum.TryParse(val, true, out t) && Enum.IsDefined(typeof(T), t)
                    ? t
                    : defaultValue;
            }

            throw new Exception("【闻祖东 2014-1-26-115324】运行时异常，输入泛型类型<T>类型应该是一个枚举类型。");
        }

        public static string GetHttpResponse(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.Accept = "text/plain";

            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            using (Stream stream = resp.GetResponseStream())
            using (StreamReader sr = new StreamReader(stream))
                return sr.ReadToEnd();
        }

        public static byte[] Compress(Encoding encoding, string strSrc)
        {
            byte[] bts = encoding.GetBytes(strSrc);

            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream zipStream = new GZipStream(ms, CompressionMode.Compress, true))
                    zipStream.Write(bts, 0, bts.Length);

                ///【闻祖东 2013-12-25-113331】在ToArray的时候zipStream必须依据是关闭状态，也就是说，不能强迫症式的两个using连写的方式！！
                ///那样会发现ToArray出来的结果的长度是有细微差别的。压缩流关闭前和关闭后的内存流的内容，是不同的。
                return ms.ToArray();
            }
        }
    }
}
