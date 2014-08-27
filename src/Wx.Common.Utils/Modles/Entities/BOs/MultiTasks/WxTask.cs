using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Wx.Common.Utils.Modles.Entities.BOs.MultiTasks
{
    public abstract class WxTask
    {
        Thread _thread;

        public WxTask()
        {
            _thread = new Thread(new ThreadStart(Do));
            //_thread.IsBackground = true;
        }

        internal void Execute()
        {
            _thread.Start();
        }

        internal void Join()
        {
            _thread.Join();
        }

        protected abstract void Do();
    }

    public abstract class WxTask<T> : WxTask
    {
        protected T _param;

        public WxTask(T param)
        {
            _param = param;
        }
    }
}
