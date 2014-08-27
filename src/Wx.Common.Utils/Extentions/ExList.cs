using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wx.Common.Utils.Modles.Entities.DTOs;

namespace Wx.Common.Utils.Extentions
{
    public static class ExList
    {
        /// <summary>
        /// 【闻祖东 2013-3-4-173647】扩展List的Take方法，原本的方法Take而不从原有的集合中删除，
        /// CRC的xTake先Take然后从原集合中删除。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<T> xTake<T>(this List<T> source, int count)
        {
            List<T> takeResult = source.Take(count).ToList();
            source.RemoveRange(0, Math.Min(count, source.Count));
            return takeResult;
        }

        /// <summary>
        /// 【闻祖东 2013-4-11-235737】分成每一份拥有固定数量元素的List。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="eachPartitionCount">【闻祖东 2013-4-26-151744】标识每一份有多少个对象。</param>
        /// <returns></returns>
        public static DevidedList<T> xPartition<T>(this IEnumerable<T> list, int eachPartitionCount)
        {
            DevidedList<T> list4Return = new DevidedList<T>();

            ///【闻祖东 2013-4-26-163152】其实是利用的ToList重新生成了一个List对象，尽量不对原有对象进行修改。
            List<T> tpList = list.ToList();

            while (tpList.Count > 0)
                list4Return.Add(tpList.xTake(eachPartitionCount));

            return list4Return;
        }

        /// <summary>
        /// 【闻祖东 2013-4-26-172250】总共需要将List分成指定数量的份数。
        /// 【闻祖东 2014-2-13-112832】也就是说，分成多少份。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="devideCount"></param>
        /// <returns></returns>
        public static DevidedList<T> xDevide<T>(this IEnumerable<T> list, int devideCount)
        {
            //int iPartitionCount = Convert.ToInt32(decimal.Ceiling((decimal)list.Count / devideCount));
            int iPartitionCount = Convert.ToInt32(decimal.Ceiling((decimal)list.Count() / devideCount));
            return list.xPartition(iPartitionCount);
        }
    }
}
