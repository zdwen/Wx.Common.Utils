using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wx.Utils.SqlServer.Entities
{
    public class ReturnValueEntityCollection : List<ReturnValueEntity>
    {
        public new void Add(ReturnValueEntity returnValueEntity)
        {
            if (Exists(rtnValue => rtnValue.Name == returnValueEntity.Name))
                throw new Exception("已经存在这个返回参数:" + returnValueEntity.Name);

            base.Add(returnValueEntity);
        }

        public ReturnValueEntity this[string name]
        {
            get
            {
                if (!Exists(rtnValue => rtnValue.Name == name))
                    throw new Exception("不存在这个返回参数:" + name);

                return this.First(rtnValue => rtnValue.Name == name);
            }
        }
    }
}
