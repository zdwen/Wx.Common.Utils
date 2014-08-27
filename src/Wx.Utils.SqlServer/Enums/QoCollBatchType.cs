using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wx.Utils.SqlServer.Enums
{
    /// <summary>
    /// 【闻祖东 2013-3-4-185323】用于标识QueryObjectCollection对象的批量执行命令的执行方式。
    /// </summary>
    public enum QoCollBatchType
    {
        /// <summary>
        /// 【闻祖东 2013-3-4-185350】同步执行
        /// </summary>
        Sync = 0,
        /// <summary>
        /// 【闻祖东 2013-3-4-185401】异步执行
        /// </summary>
        ASync,
    }
}
