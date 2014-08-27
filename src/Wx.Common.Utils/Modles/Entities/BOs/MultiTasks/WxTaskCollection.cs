using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wx.Common.Utils.Modles.Entities.BOs.MultiTasks
{
    /// <summary>
    /// 【闻祖东 2014-8-27-113626】多任务执行并等待返回的封装。
    /// </summary>
    public class WxTaskCollection
    {
        List<WxTask> _tasks;

        public WxTaskCollection()
        {
            _tasks = new List<WxTask>();
        }

        public void AddTask(WxTask task)
        {
            _tasks.Add(task);
        }

        public void Execute()
        {
            ///【闻祖东 2014-8-26-162032】由于本身是多线程，就不再用匿名托管的方式避免闭包泄露。
            foreach (WxTask task in _tasks)
                task.Execute();

            Join();
        }

        void Join()
        {
            foreach (WxTask task in _tasks)
                task.Join();
        }
    }
}
