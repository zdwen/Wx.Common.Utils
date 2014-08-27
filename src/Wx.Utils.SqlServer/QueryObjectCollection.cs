using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BLToolkit.Data;
using System.Threading;
using System.Data.SqlClient;
using System.Data;
using Wx.Common.Utils.Extentions;
using Wx.Utils.SqlServer.Extentions;
using Wx.Utils.SqlServer.Enums;
using Wx.Common.Utils;
using Wx.Common.Utils.Modles.Entities.BOs.MultiTasks;
using Wx.Common.Utils.Modles.Entities.DTOs;

namespace Wx.Utils.SqlServer
{
    public class QueryObjectCollection
    {
        List<QueryObject> _QOs;

        /// <summary>
        /// 【闻祖东 2013-3-5-162525】执行所耗费的时间(s)
        /// </summary>
        public double CostSecond { get; private set; }

        /// <summary>
        /// 【闻祖东 2014-8-27-161654】批量执行的方式，默认为单线程执行。
        /// </summary>
        public BatchType BatchType { get; set; }
        /// <summary>
        /// 【闻祖东 2013-3-5-115339】每次批量执行的条数，默认值为100。
        /// </summary>
        public int BatchCount { get; set; }

        public QueryObjectCollection()
        {
            _QOs = new List<QueryObject>();
            BatchType = BatchType.SingleThread;
            CostSecond = 0;
            BatchCount = 100;
        }

        public string CmdText
        {
            get
            {
                string sql = string.Empty;
                _QOs.ForEach(qo => sql += string.Format("{0};\r\n", qo.CmdText));
                return sql;
            }
        }

        public void Add(QueryObject queryObject)
        {
            _QOs.Add(queryObject);
        }

        public void ExecuteNonQueries()
        {
            DateTime dtStart = DateTime.UtcNow;
            switch (BatchType)
            {
                case BatchType.SingleThread:
                    ExecuteBatches(_QOs);
                    break;
                case BatchType.MultiThread:
                    WxTaskCollection taskColl = new WxTaskCollection();

                    foreach (List<QueryObject> listQOs in _QOs.xPartition(BatchCount))
                        taskColl.AddTask(new WxAction<List<QueryObject>>(ExecuteBatches, listQOs));

                    taskColl.Execute();
                    break;
                default:
                    break;
            }

            CostSecond = (DateTime.UtcNow - dtStart).TotalSeconds;
        }




        static void ExecuteBatches(List<QueryObject> queryObjects)
        {
            foreach (QueryObject queryObject in queryObjects)
                queryObject.xExecuteNonQuery();
        }
    }
}
