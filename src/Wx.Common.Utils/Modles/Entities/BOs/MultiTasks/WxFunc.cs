using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wx.Common.Utils.Modles.Entities.BOs.MultiTasks
{
    public class WxFunc<U> : WxTask
    {
        public U ExeResult { get; set; }
        Func<U> _func { get; set; }

        public WxFunc(Func<U> func)
        {
            _func = func;
        }

        protected override void Do()
        {
            ExeResult = _func.Invoke();
        }
    }

    public class WxFunc<T, U> : WxTask<T>
    {
        public U ExeResult { get; set; }
        Func<T, U> _func { get; set; }

        public WxFunc(T param, Func<T, U> func)
            : base(param)
        {
            _func = func;
        }

        protected override void Do()
        {
            ExeResult = _func.Invoke(_param);
        }
    }
}
