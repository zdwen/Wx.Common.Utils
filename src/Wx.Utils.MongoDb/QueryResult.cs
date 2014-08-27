using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wx.Utils.MongoDb
{
    public class QueryResult<T>
    {
        /// <summary>
        /// 【闻祖东 2014-8-20-180401】数据库中匹配Query查询条件的BsonDocument文档记录总数(无视Limit和Skip条件)。
        /// </summary>
        public int DocsMatchedSum { get; set; }

        /// <summary>
        /// 【闻祖东 2014-8-20-180435】在当前Query、Limit、Skip共同作用下返回的记录。
        /// </summary>
        public List<T> DataReturned { get; set; }
    }
}
