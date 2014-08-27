using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wx.Common.Utils.Modles.Entities.BOs.MultiTasks
{
    public class WxAction : WxTask
    {
        Action _action;

        public WxAction(Action action)
            : base()
        {
            _action = action;
        }

        protected override void Do()
        {
            _action.Invoke();
        }
    }

    public class WxAction<T> : WxTask<T>
    {
        Action<T> _action;

        public WxAction(Action<T> action, T param)
            : base(param)
        {
            _action = action;
        }

        protected override void Do()
        {
            _action.Invoke(_param);
        }
    }
}
