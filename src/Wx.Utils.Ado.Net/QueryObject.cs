using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Wx.Utils.Ado.Net.Enums;
using System.Configuration;

namespace Wx.Utils.Ado.Net
{
    public abstract class QueryObject
    {
        public abstract string ConnectionStringNode { get; }

        public string CmdText { get; set; }
        public int CmdTimeOut { get; set; }
        public CmdType CmdType { get; set; }
        public bool EnumAsString { get; set; }

        internal Dictionary<string, SqlParameter> DicInputParams { get; set; }
        internal Dictionary<string, SqlParameter> ReturnParams { get; set; }
        internal Dictionary<string, SqlParameter> OutputParams { get; set; }

        SqlConnection CreateConn()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionStringNode].ConnectionString);
        }

        public QueryObject()
        {
            CmdType = CmdType.Text;
            CmdTimeOut = 30;
            CmdText = string.Empty;
            EnumAsString = true;
            DicInputParams = new Dictionary<string, SqlParameter>();
            OutputParams = new Dictionary<string, SqlParameter>();
            ReturnParams = new Dictionary<string, SqlParameter>();
        }

        public void AddParam(string paramName, object paramValue, SqlDbType? dbType = null)
        {
            SqlParameter param = new SqlParameter()
            {
                ParameterName = paramName,
                Direction = ParameterDirection.Input,
                SqlValue = paramValue == null
                    ? DBNull.Value
                    : EnumAsString && paramValue.GetType().IsEnum
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

        public void AddOutputParam(string paramName, SqlDbType dbType, int size)
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

        public void AddOutputParam(string paramName, SqlDbType dbType)
        {
            SqlParameter param = new SqlParameter()
            {
                ParameterName = paramName,
                Direction = ParameterDirection.Output,
                SqlDbType = dbType,
            };

            OutputParams[paramName] = param;
        }

        public DataSet ExecuteDataSet()
        {
            DataSet dsResult = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter(CmdText, CreateConn());
            adapter.Fill(dsResult);

            return dsResult;
        }

        public DataTable ExecuteDataTable()
        {
            return ExecuteDataSet().Tables[0];
        }

        public int ExecuteNonQuery()
        {
            using (SqlConnection conn = CreateConn())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(CmdText, conn);
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
