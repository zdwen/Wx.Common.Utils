using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Wx.Utils.SqlServer.Entities
{
    public class ReturnValueEntity
    {
        public string Name { get; set; }
        public DbType DbType { get; set; }
        public int Size { get; set; }
        public object Value { get; set; }

        public ReturnValueEntity()
        {
            Name = string.Empty;
            DbType = DbType.String;
            Size = 0;
            Value = null;
        }
    }
}
