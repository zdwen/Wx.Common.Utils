using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BLToolkit.Data;
using System.Data;
using System.Data.SqlClient;
using Wx.Utils.SqlServer.Extentions;
using Wx.Utils.SqlServer.Entities;

namespace Wx.Utils.SqlServer
{
    /// <summary>
    /// 【闻祖东 2011-12-28-175344】CRC内部自定义的SQL命令执行对象
    /// 目的在于使与数据库打交道的业务中只关心必要的查询语句以及参数，
    /// 而不去关于创建连接以及释放资源等等，减少重复代码。
    /// </summary>
    public abstract class QueryObject
    {
        /// <summary>
        /// 【闻祖东 2012-1-31-154326】配置文件中的指向数据库连接字符串的节点
        /// </summary>
        public abstract string ConnectionStringNode { get; }

        /// <summary>
        /// 【闻祖东 2011-12-27-150501】等待执行的带参数的SQL语句或存储过程名称
        /// </summary>
        public string CmdText { get; set; }
        /// <summary>
        /// 【闻祖东 2013-2-4-111959】新加入的重要元素，执行的超时时间。
        /// 主要是近期风控检测线上环境频繁的插入操作超时。
        /// </summary>
        public int CmdTimeOut { get; set; }

        CommandType _commandType;
        /// <summary>
        /// 【闻祖东 2011-12-28-171958】命令类型
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
        /// 【闻祖东 2012-1-31-154314】Input参数字典
        /// </summary>
        public Dictionary<string, SqlParameter> DicInputParams { get; protected set; }

        /// <summary>
        /// 【闻祖东 2012-2-2-170521】Return参数字典
        /// </summary>
        public ReturnValueEntityCollection ReturnParams { get; private set; }
        /// <summary>
        /// 【闻祖东 2012-2-19-110211】Output参数字典
        /// </summary>
        public ReturnValueEntityCollection OutputParams { get; private set; }

        protected QueryObject()
        {
            CommandType = CommandType.Text;
            CmdTimeOut = 30;
            CmdText = string.Empty;
            DicInputParams = new Dictionary<string, SqlParameter>();
            ReturnParams = new ReturnValueEntityCollection();
            OutputParams = new ReturnValueEntityCollection();
        }

        /// <summary>
        /// 【闻祖东 2011-12-27-150815】执行的SQL的原始语句
        /// 【闻祖东 2012-11-23-153111】当前已添加支持存储过程的原始执行语句输出。
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

        /// <summary>
        /// 【闻祖东 2012-2-2-163120】默认的添加参数的方法，添加输入参数（InputParam）
        /// 【闻祖东 2012-3-26-151950】默认情况下，枚举会被转换成相应的名称字符串（注）。
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <param name="paramValue">参数值</param>
        [Obsolete("【闻祖东 2013-11-13-174251】此方法引起的参数类型转换造成数据库的性能问题，此方法强制建议采用AddParam(string paramName, object paramValue, SqlDbType dbType)来替代。")]
        public void AddParam(string paramName, object paramValue)
        {
            ///【闻祖东 2012-2-19-115605】在CRC里面凡是程序里面的枚举值，存储到数据库默认全部都是存储的其字符串的值，
            ///这算是CRC的一个硬性规范。
            //if (paramValue != null && paramValue.GetType().IsEnum)
            //    DicInputParams.Add(paramName, paramValue.ToString());
            //else
            //    DicInputParams.Add(paramName, paramValue);

            /*DicInputParams[paramName] = (paramValue != null && paramValue.GetType().IsEnum)
                ? paramValue.ToString()
                : paramValue;*/

            SqlParameter param = new SqlParameter()
            {
                ParameterName = paramName,
                SqlValue = (paramValue != null && paramValue.GetType().IsEnum)
                    ? paramValue.ToString()
                    : paramValue,
            };

            DicInputParams[paramName] = param;
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

        /// <summary>
        /// 【闻祖东 2012-2-2-163222】添加返回参数（ParameterDirection.ReturnValue）
        /// </summary>
        /// <param name="paramName">参数名</param>
        public void AddReturnParam(string paramName, DbType dbType, int size)
        {
            ReturnParams.Add(new ReturnValueEntity() { Name = paramName, DbType = dbType, Size = size });
        }

        public void AddOutputParam(string paramName, DbType dbType, int size)
        {
            OutputParams.Add(new ReturnValueEntity() { Name = paramName, DbType = dbType, Size = size });
        }

        public void AddOutputParam(string paramName, DbType dbType)
        {
            OutputParams.Add(new ReturnValueEntity() { Name = paramName, DbType = dbType });
        }

        public DbManager CreateDbManager4Crc(string connectionStringNode)
        {
            DbManager dbManager = new DbManager(connectionStringNode);
            dbManager.Command.CommandTimeout = CmdTimeOut;
            return dbManager;
        }

        public DataTable xExecuteDataTable()
        {
            using (DbManager dbManager = CreateDbManager4Crc(ConnectionStringNode))
            {
                DbManager dbAssigned = dbManager.xSetCommand(this);
                DataTable dtResult = dbAssigned.ExecuteDataTable();
                Evaluate4ReturnOrOutValue(dbAssigned);

                return dtResult;
            }
        }

        public DataSet xExecuteDataSet()
        {
            using (DbManager dbManager = CreateDbManager4Crc(ConnectionStringNode))
            {
                DbManager dbAssigned = dbManager.xSetCommand(this);
                DataSet dsResult = dbAssigned.ExecuteDataSet();
                Evaluate4ReturnOrOutValue(dbAssigned);

                return dsResult;
            }
        }

        public int xExecuteNonQuery()
        {
            using (DbManager dbManager = CreateDbManager4Crc(ConnectionStringNode))
            {
                DbManager dbAssigned = dbManager.xSetCommand(this);
                int iEffect = dbAssigned.ExecuteNonQuery();
                Evaluate4ReturnOrOutValue(dbAssigned);

                return iEffect;
            }
        }

        public List<T> xExecuteList<T>()
        {
            using (DbManager dbManager = CreateDbManager4Crc(ConnectionStringNode))
            {
                DbManager dbAssigned = dbManager.xSetCommand(this);
                List<T> listResult = dbAssigned.ExecuteList<T>();
                Evaluate4ReturnOrOutValue(dbAssigned);

                return listResult;
            }
        }

        public T xExecuteObject<T>()
        {
            using (DbManager dbManager = CreateDbManager4Crc(ConnectionStringNode))
            {
                DbManager dbAssigned = dbManager.xSetCommand(this);
                T tResult = dbAssigned.ExecuteObject<T>();
                Evaluate4ReturnOrOutValue(dbAssigned);

                return tResult;
            }
        }

        public T xExecuteScalar<T>()
        {
            using (DbManager dbManager = CreateDbManager4Crc(ConnectionStringNode))
            {
                DbManager dbAssigned = dbManager.xSetCommand(this);
                T tResult = dbAssigned.ExecuteScalar<T>();
                Evaluate4ReturnOrOutValue(dbAssigned);

                return tResult;
            }
        }

        void Evaluate4ReturnOrOutValue(DbManager dbAssigned)
        {
            ReturnParams.ForEach(rtnValue => rtnValue.Value = dbAssigned.Parameter(rtnValue.Name).Value);
            OutputParams.ForEach(otpValue => otpValue.Value = dbAssigned.Parameter(otpValue.Name).Value);
        }
    }
}
