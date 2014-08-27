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

namespace Wx.Utils.SqlServer
{
    /// <summary>
    /// 【闻祖东 2012-6-25-180442】当前只支持执行无业务参数返回的批执行。
    /// 无返回参数（DataTable、DataSet），无输出参数（output param\return param）
    /// </summary>
    public class QueryObjectCollection : List<QueryObject>
    {
        object _lock;

        QueryObject SymbolQueryObject { get { return this[0]; } }
        public CommandType CommandType { get { return SymbolQueryObject.CommandType; } }
        /// <summary>
        /// 【闻祖东 2013-3-5-115438】所影响的行数。
        /// </summary>
        public int EffectCount { get; private set; }
        /// <summary>
        /// 【闻祖东 2013-3-5-162525】执行所耗费的时间(s)
        /// </summary>
        public double CostSecond { get; private set; }
        /// <summary>
        /// 【闻祖东 2013-3-5-162515】执行的超时时间(s)
        /// </summary>
        public int CmdTimeOut { get; set; }

        /// <summary>
        /// 【闻祖东 2013-3-5-115425】输入参数列表。
        /// </summary>
        public Dictionary<string, object> DicInputParams { get; private set; }
        /// <summary>
        /// 【闻祖东 2013-3-5-115356】每次批量执行的类型，默认为同步执行。
        /// </summary>
        public QoCollBatchType BatchType { get; set; }
        /// <summary>
        /// 【闻祖东 2013-3-5-115339】每次批量执行的条数，默认值为100。
        /// </summary>
        public int BatchCount { get; set; }

        public QueryObjectCollection()
        {
            DicInputParams = new Dictionary<string, object>();
            BatchType = QoCollBatchType.Sync;
            CostSecond
            = EffectCount
            = 0;
            BatchCount = 100;
            CmdTimeOut = 60;

            _lock = new object();
        }

        [Obsolete("【闻祖东 2014-1-6-114401】此方法标记为过期。请用三个参数的同名方法替代。")]
        public void AddParam(string paramName, object paramValue)
        {
            DicInputParams[paramName] = (paramValue != null && paramValue.GetType().IsEnum)
                ? paramValue.ToString()
                : paramValue;
        }

        public void AddParam(string paramName, object paramValue, SqlDbType dbType)
        {
            SqlParameter param = new SqlParameter()
            {
                ParameterName = paramName,
                SqlDbType = dbType,
                SqlValue = paramValue == null
                    ? DBNull.Value
                    : paramValue.GetType().IsEnum
                        ? paramValue.ToString()
                        : paramValue,
            };

            DicInputParams[paramName] = param;
        }

        public string CmdText
        {
            get
            {
                string sql = string.Empty;
                this.ForEach(qo => sql += string.Format("{0};\r\n", qo.CmdText));
                return sql;
            }
        }

        public string DraftSqlStatement
        {
            get
            {
                string sql = string.Empty;
                this.ForEach(qo => sql += string.Format("{0};\r\n", qo.DraftSqlStatement));

                IOrderedEnumerable<KeyValuePair<string, object>> paramColl = from p in DicInputParams orderby p.Key.Length descending select p;
                foreach (KeyValuePair<string, object> kvp in paramColl)
                {
                    if (kvp.Value == null)
                        sql = sql.Replace(kvp.Key, "NULL");
                    else if (kvp.Value is bool)
                        sql = sql.Replace(kvp.Key, Convert.ToInt16(kvp.Value).ToString());
                    else if (kvp.Value is string || kvp.Value is char || kvp.Value is DateTime)
                    {
                        string sTempValue = kvp.Value.ToString().Replace("'", "''");
                        sql = sql.Replace(kvp.Key, string.Format("'{0}'", sTempValue));
                    }
                    else
                        sql = sql.Replace(kvp.Key, kvp.Value.ToString());
                }

                return sql;
            }
        }

        public new void Add(QueryObject queryObject)
        {
            if (this.Count == 0 || SymbolQueryObject.CommandType == queryObject.CommandType)
                base.Add(queryObject);
            else
                throw new Exception("【闻祖东 2012-6-25-174831】CRC自定义异常，QueryObjectCollection对象里面的成员的CommandType应该保持一致。");
        }

        public int xExecuteNonQuery()
        {
            if (this.Count == 0)
            {
                CostSecond
                = EffectCount
                = 0;

                return EffectCount;
            }

            DateTime dtStart = DateTime.UtcNow;
            List<QueryObject> listQo = this.ToList();
            List<Thread> threads = new List<Thread>();
            while (listQo.Count > 0)
            {
                QueryObjectCollection tpQoColl = new QueryObjectCollection();
                tpQoColl.DicInputParams = this.DicInputParams;
                listQo.xTake(BatchCount).ForEach(qo => tpQoColl.Add(qo));

                switch (BatchType)
                {
                    case QoCollBatchType.Sync:
                        ProceedQoColl(tpQoColl);
                        break;
                    case QoCollBatchType.ASync:
                        //Thread thread = new Thread(() => ProceedQoColl(tpQoColl));
                        ///【闻祖东 2013-9-30-145531】启用匿名托管的方式，防止闭包泄露。
                        ParameterizedThreadStart threadStart = new ParameterizedThreadStart(ProceedQoColl);
                        Thread thread = new Thread(threadStart);

                        thread.Start(tpQoColl);
                        threads.Add(thread);
                        break;
                    default:
                        throw new Exception(string.Format("【闻祖东 2013-3-4-185937】自定义异常，不应该出现此类BatchType:{0}。", BatchType));
                }
            }

            if (BatchType == QoCollBatchType.ASync)
                threads.ForEach(thread => thread.Join());

            CostSecond = (DateTime.UtcNow - dtStart).TotalSeconds;
            return EffectCount;
        }

        //void ProceedQoColl(QueryObjectCollection tempQoColl)
        //{
        //    using (DbManager dbManager = new DbManager(tempQoColl.SymbolQueryObject.ConnectionStringNode))
        //    {
        //        dbManager.Command.CommandTimeout = CmdTimeOut;
        //        CumulateEffectCount(dbManager.xSetCommand(tempQoColl).ExecuteNonQuery());
        //    }
        //}

        void ProceedQoColl(object tempQoColl)
        {
            QueryObjectCollection qoColl = (QueryObjectCollection)tempQoColl;
            using (DbManager dbManager = new DbManager(qoColl.SymbolQueryObject.ConnectionStringNode))
            {
                dbManager.Command.CommandTimeout = CmdTimeOut;
                CumulateEffectCount(dbManager.xSetCommand(qoColl).ExecuteNonQuery());
            }
        }

        void CumulateEffectCount(int newEffectCount)
        {
            lock (_lock)
                EffectCount += newEffectCount;
        }
    }
}
