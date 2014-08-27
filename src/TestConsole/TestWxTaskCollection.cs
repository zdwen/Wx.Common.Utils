using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wx.Common.Utils.Modles.Entities.BOs.MultiTasks;

namespace TestConsole
{
    class TestWxTaskCollection
    {
        public static void Test()
        {
            WxTaskCollection taskColl = new WxTaskCollection();

            taskColl.AddTask(new WxAction(DoSomething1));
            taskColl.AddTask(new WxAction(DoSomething2));
            taskColl.AddTask(new WxAction<int>(DoSomething3, 123));

            taskColl.Execute();

            Console.WriteLine("全部执行完成，此时才触发断点。");
            Console.ReadLine();
        }

        static void DoSomething1()
        {
            int i = 0;
            while (i++ < 20)
            {
                Thread.Sleep(500);
                Console.WriteLine("{0}- DoSomething1-{1}", Thread.CurrentThread.ManagedThreadId, i);
            }
        }

        static void DoSomething2()
        {
            int i = 0;
            while (i++ < 20)
            {
                Thread.Sleep(600);
                Console.WriteLine("{0}:-DoSomething2-{1}", Thread.CurrentThread.ManagedThreadId, i);
            }
        }

        static void DoSomething3(int iHehe)
        {
            int i = 0;
            while (i++ < 20)
            {
                Thread.Sleep(700);
                Console.WriteLine("{0}:-DoSomething3-{1}-Param-{2}", Thread.CurrentThread.ManagedThreadId, i, iHehe);
            }
        }
    }
}
