using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wx.Utils.SqlServer.Enums
{
    /// <summary>
    /// 【闻祖东 2013-3-4-185323】用于标识QueryObjectCollection对象的批量执行命令的执行方式。
    /// </summary>
    public enum BatchType
    {
        /// <summary>
        /// 【闻祖东 2014-8-27-161059】单线程执行
        /// </summary>
        SingleThread = 0,
        /// <summary>
        /// 【闻祖东 2014-8-27-161110】多线程执行
        /// </summary>
        MultiThread,
    }
}
