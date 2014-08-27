using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BLToolkit.Data;
using System.Data;

namespace Wx.Utils.SqlServer.Extentions
{
    internal static class ExDbManager
    {
        public static DbManager xSetCommand(this DbManager dbManager, QueryObject queryObject)
        {
            List<IDbDataParameter> list = new List<IDbDataParameter>();

            //queryObject.DicInputParams.ToList().ForEach(kvp => list.Add(dbManager.Parameter(kvp.Key, kvp.Value)));
            //queryObject.DicInputParams2.Values.ToList().ForEach(param => list.AddRange((dbManager.Parameter(kvp.Key, kvp.Value)));
            list.AddRange(queryObject.DicInputParams.Values);
            queryObject.ReturnParams.ForEach(rtnValue => list.Add(dbManager.Parameter(ParameterDirection.ReturnValue, rtnValue.Name, rtnValue.DbType, rtnValue.Size)));
            queryObject.OutputParams.ForEach(otpValue => list.Add(dbManager.Parameter(ParameterDirection.Output, otpValue.Name, otpValue.DbType, otpValue.Size)));

            return dbManager.SetCommand(queryObject.CommandType, queryObject.CmdText, list.ToArray());
        }

        public static DbManager xSetCommand(this DbManager dbManager, QueryObjectCollection queryObjectColl)
        {
            List<IDbDataParameter> list = new List<IDbDataParameter>();

            foreach (QueryObject qo in queryObjectColl)
            {
                //qo.DicInputParams.ToList().ForEach(kvp => list.Add(dbManager.Parameter(kvp.Key, kvp.Value)));
                list.AddRange(qo.DicInputParams.Values);
            }

            queryObjectColl.DicInputParams.ToList().ForEach(kvp => list.Add(dbManager.Parameter(kvp.Key, kvp.Value)));

            return dbManager.SetCommand(queryObjectColl.CommandType, queryObjectColl.CmdText, list.ToArray());
        }
    }
}
