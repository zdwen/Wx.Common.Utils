using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BLToolkit.Data;
using System.Data;
using System.Data.SqlClient;

namespace Wx.Utils.SqlServer
{
    /// <summary>
    /// 【zdwen 2011-12-28-175344】CRC内部自定义的SQL命令执行对象
    /// 目的在于使与数据库打交道的业务中只关心必要的查询语句以及参数，
    /// 而不去关于创建连接以及释放资源等等，减少重复代码。
    /// </summary>
    public abstract class WxQueryObject
    {
        /// <summary>
        /// 【zdwen 2012-1-31-154326】配置文件中的指向数据库连接字符串的节点
        /// </summary>
        public abstract string ConnectionStringNode { get; }

        /// <summary>
        /// 【zdwen 2011-12-27-150501】等待执行的带参数的SQL语句或存储过程名称
        /// </summary>
        public string CmdText { get; set; }
        /// <summary>
        /// 【zdwen 2013-2-4-111959】新加入的重要元素，执行的超时时间。
        /// 主要是近期风控检测线上环境频繁的插入操作超时。
        /// </summary>
        public int CmdTimeOut { get; set; }
        /// <summary>
        /// 【zdwen 2014-8-27-182145】标识当传入的参数为枚举类型的时候，是否将其处理为对应的字符串，否则处理为其对应的int值。
        /// </summary>
        public bool EnumAsString { get; set; }

        CommandType _commandType;
        /// <summary>
        /// 【zdwen 2011-12-28-171958】命令类型
        /// </summary>
        public CommandType CommandType
        {
            get { return _commandType; }
            set
            {
                if (value == CommandType.TableDirect)
                    throw new Exception("CRC自定义异常【83FD0687】，目前QueryObject不支持整表查询。");

                _commandType = value;
            }
        }

        /// <summary>
        /// 【zdwen 2012-1-31-154314】Input参数字典
        /// </summary>
        internal Dictionary<string, SqlParameter> DicInputParams { get; set; }
        public Dictionary<string, SqlParameter> ReturnParams { get; set; }
        public Dictionary<string, SqlParameter> OutputParams { get; set; }

        protected WxQueryObject()
        {
            CommandType = CommandType.Text;
            CmdTimeOut = 30;
            CmdText = string.Empty;
            EnumAsString = true;
            DicInputParams = new Dictionary<string, SqlParameter>();
            OutputParams = new Dictionary<string, SqlParameter>();
            ReturnParams = new Dictionary<string, SqlParameter>();
        }

        /// <summary>
        /// 【zdwen 2011-12-27-150815】执行的SQL的原始语句
        /// 【zdwen 2012-11-23-153111】当前已添加支持存储过程的原始执行语句输出。
        /// 只是当前Case不需要Output以及Return参数，故就未做实现。
        /// </summary>
        public string DraftSqlStatement
        {
            get
            {
                IEnumerable<SqlParameter> paramColl = from p in DicInputParams orderby p.Key.Length descending select p.Value;

                string sStatement = CmdText;

                if (CommandType == CommandType.Text)
                {
                    foreach (SqlParameter param in paramColl)
                    {
                        if (param.SqlValue == DBNull.Value)
                            sStatement = sStatement.Replace(param.ParameterName, "NULL");
                        else if (param.SqlDbType == SqlDbType.Bit)
                            sStatement = sStatement.Replace(param.ParameterName, Convert.ToInt16(param.SqlValue).ToString());
                        else if (param.SqlDbType == SqlDbType.VarChar
                            || param.SqlDbType == SqlDbType.Char
                            || param.SqlDbType == SqlDbType.Date
                            || param.SqlDbType == SqlDbType.DateTime
                            || param.SqlDbType == SqlDbType.DateTime2
                            || param.SqlDbType == SqlDbType.SmallDateTime
                            || param.SqlDbType == SqlDbType.Text
                            )
                        {
                            string sTempValue = param.SqlValue.ToString().Replace("'", "''");
                            sStatement = sStatement.Replace(param.ParameterName, string.Format("'{0}'", sTempValue));
                        }
                        else if (param.SqlDbType == SqlDbType.NVarChar
                            || param.SqlDbType == SqlDbType.NChar
                            || param.SqlDbType == SqlDbType.NText)
                        {
                            string sTempValue = param.SqlValue.ToString().Replace("'", "''");
                            sStatement = sStatement.Replace(param.ParameterName, string.Format("N'{0}'", sTempValue));
                        }
                        else
                            sStatement = sStatement.Replace(param.ParameterName, param.SqlValue.ToString());
                    }
                }
                else if (CommandType == CommandType.StoredProcedure)
                {
                    sStatement = string.Format("EXEC {0}\r\n", sStatement);
                    string sSingleParamDesc = "{0} = {1},\r\n";
                    foreach (SqlParameter param in paramColl)
                    {
                        if (param.SqlValue == DBNull.Value)
                            sStatement += string.Format(sSingleParamDesc, param.ParameterName, "NULL");
                        else if (param.SqlDbType == SqlDbType.Bit)
                            sStatement += string.Format(sSingleParamDesc, param.ParameterName, Convert.ToInt16(param.SqlValue).ToString());
                        else if (param.SqlDbType == SqlDbType.VarChar
                            || param.SqlDbType == SqlDbType.Char
                            || param.SqlDbType == SqlDbType.Date
                            || param.SqlDbType == SqlDbType.DateTime
                            || param.SqlDbType == SqlDbType.DateTime2
                            || param.SqlDbType == SqlDbType.SmallDateTime
                            || param.SqlDbType == SqlDbType.Text)
                        {
                            string sTempValue = param.SqlValue.ToString().Replace("'", "''");
                            sStatement += string.Format(sSingleParamDesc, param.ParameterName, string.Format("'{0}'", sTempValue));
                        }
                        else if (param.SqlDbType == SqlDbType.NVarChar
                            || param.SqlDbType == SqlDbType.NChar
                            || param.SqlDbType == SqlDbType.NText)
                        {
                            string sTempValue = param.SqlValue.ToString().Replace("'", "''");
                            sStatement = sStatement.Replace(param.ParameterName, string.Format("N'{0}'", sTempValue));
                        }
                        else
                            sStatement += string.Format(sSingleParamDesc, param.ParameterName, param.SqlValue.ToString());
                    }

                    if (DicInputParams.Count > 0)
                        sStatement = sStatement.Remove(sStatement.Length - 3, 3);

                    sStatement += ";";
                }

                return sStatement;
            }
        }

        public void AddParam(string paramName, object paramValue, SqlDbType? dbType = null)
        {
            AddParam(paramName, paramValue, false, dbType);
        }

        /// <summary>
        /// 【闻祖东 2015-04-18 143348】默认的添加参数的方法。为了避免数据库端的类型判断带来的性能问题，建议最好是提交dbType参数。
        /// 【闻祖东 2015-04-18 143403】默认情况下，枚举值会按照Int类型进行插入。
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <param name="paramValue">参数值</param>
        /// <param name="enumAsString">标识是否将枚举值作为String进行传输</param>
        /// <param name="dbType">数据类型</param>
        public void AddParam(string paramName, object paramValue, bool enumAsString = false, SqlDbType? dbType = null)
        {
            SqlParameter param = new SqlParameter()
            {
                ParameterName = paramName,
                Direction = ParameterDirection.Input,
                SqlValue = paramValue == null
                    ? DBNull.Value
                    : EnumAsString && paramValue.GetType().IsEnum && enumAsString
                        ? paramValue.ToString()
                        : paramValue,
            };

            if (dbType.HasValue)
                param.SqlDbType = dbType.Value;

            DicInputParams[paramName] = param;
        }

        public void AddReturnParamNew(string paramName, SqlDbType dbType, int size)
        {
            SqlParameter param = new SqlParameter()
            {
                ParameterName = paramName,
                Direction = ParameterDirection.ReturnValue,
                SqlDbType = dbType,
                Size = size,
            };

            ReturnParams[paramName] = param;
        }

        public void AddParamOutput(string paramName, SqlDbType dbType, int size)
        {
            SqlParameter param = new SqlParameter()
            {
                ParameterName = paramName,
                Direction = ParameterDirection.Output,
                SqlDbType = dbType,
                Size = size,
            };

            OutputParams[paramName] = param;
        }

        public void AddParamOutput(string paramName, SqlDbType dbType)
        {
            SqlParameter param = new SqlParameter()
            {
                ParameterName = paramName,
                Direction = ParameterDirection.Output,
                SqlDbType = dbType,
            };

            OutputParams[paramName] = param;
        }

        public DbManager CreateDbManager(string connectionStringNode)
        {
            DbManager dbManager = new DbManager(connectionStringNode);
            dbManager.Command.CommandTimeout = CmdTimeOut;
            List<IDbDataParameter> list = new List<IDbDataParameter>();

            list.AddRange(DicInputParams.Values);
            list.AddRange(ReturnParams.Values);
            list.AddRange(OutputParams.Values);

            return dbManager.SetCommand(CommandType, CmdText, list.ToArray());
        }

        public DataTable xExecuteDataTable()
        {
            using (DbManager dbManager = CreateDbManager(ConnectionStringNode))
            {
                DataTable dtResult = dbManager.ExecuteDataTable();
                Evaluate4ReturnOrOutValue(dbManager);

                return dtResult;
            }
        }

        public DataSet xExecuteDataSet()
        {
            using (DbManager dbManager = CreateDbManager(ConnectionStringNode))
            {
                DataSet dsResult = dbManager.ExecuteDataSet();
                Evaluate4ReturnOrOutValue(dbManager);

                return dsResult;
            }
        }

        public int xExecuteNonQuery()
        {
            using (DbManager dbManager = CreateDbManager(ConnectionStringNode))
            {
                int iEffect = dbManager.ExecuteNonQuery();
                Evaluate4ReturnOrOutValue(dbManager);

                return iEffect;
            }
        }

        public List<T> xExecuteList<T>()
        {
            using (DbManager dbManager = CreateDbManager(ConnectionStringNode))
            {
                List<T> listResult = dbManager.ExecuteList<T>();
                Evaluate4ReturnOrOutValue(dbManager);

                return listResult;
            }
        }

        public T xExecuteObject<T>()
        {
            using (DbManager dbManager = CreateDbManager(ConnectionStringNode))
            {
                T tResult = dbManager.ExecuteObject<T>();
                Evaluate4ReturnOrOutValue(dbManager);

                return tResult;
            }
        }

        public T xExecuteScalar<T>()
        {
            using (DbManager dbManager = CreateDbManager(ConnectionStringNode))
            {
                T tResult = dbManager.ExecuteScalar<T>();
                Evaluate4ReturnOrOutValue(dbManager);

                return tResult;
            }
        }

        void Evaluate4ReturnOrOutValue(DbManager dbAssigned)
        {
            ReturnParams.Values.ToList().ForEach(param => param.SqlValue = dbAssigned.Parameter(param.ParameterName).Value);
            OutputParams.Values.ToList().ForEach(param => param.SqlValue = dbAssigned.Parameter(param.ParameterName).Value);
        }
    }
}
