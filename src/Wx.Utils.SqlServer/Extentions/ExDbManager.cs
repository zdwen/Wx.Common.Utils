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

            list.AddRange(queryObject.DicInputParams.Values);
            list.AddRange(queryObject.ReturnParams.Values);
            list.AddRange(queryObject.OutputParams.Values);

            return dbManager.SetCommand(queryObject.CommandType, queryObject.CmdText, list.ToArray());
        }
    }
}
