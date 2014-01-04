using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Smarthouse
{
    class Core
    {
        Thread main_thread;

        public Test test;
        public Core()
        {
            main_thread = new Thread(delegate() { doCoreStuff(); });
        }

        void doCoreStuff()
        {
            init();
            watch();
        }

        //Download smth from cfgs
        void init()
        {
            //Init other classes
            test = new Test();
        }

        void watch()
        {
            //watching all's status, doing smth interesting
            test.Start();
            do
            {
                Console.WriteLine("Main thread!");
                Thread.Sleep(10000);
            } while (true);
        }

        public void Start()
        {
            if (main_thread == null) { main_thread = new Thread(delegate() { doCoreStuff(); });}
            if (!main_thread.IsAlive) { main_thread.Start(); }
        }
        public void Stop()
        {
            if (main_thread.IsAlive)
            {
                main_thread.Abort();
                main_thread = null;
            }
        }

    }
}
