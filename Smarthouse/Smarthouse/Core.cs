using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Smarthouse
{
    class Core : ThreadControllable
    {
        #region Thread stuff
        Thread main_thread;
        ThreadStart start_main;
        bool isWorking;
        #endregion
        #region Processes
        public Test test;
        #endregion
        public Core()
        {
            start_main = new ThreadStart(doCoreStuff);
            main_thread = new Thread(start_main);
        }

        void doCoreStuff()
        {
            init();
            watch();
        }

       
        void init()
        { 
            //Download smth from cfgs
            //Init other classes 
            test = new Test();
        }

        void watch()
        {
            //watching all's status, doing smth interesting
            test.Start();
            do
            {
                Console.WriteLine("Core!");
                Thread.Sleep(1000);
            } while (isWorking);
        }

        #region ThreadControllable
        public void Start()
        {
            isWorking = true;
            if (main_thread == null) { main_thread = new Thread(delegate() { doCoreStuff(); });}
            if (!main_thread.IsAlive) { main_thread.Start(); }
            
        }
        public void Stop()
        {
            isWorking = false;
            if (main_thread !=null && main_thread.IsAlive)
            {
                main_thread.Abort();
                main_thread = null;
            }
            test.Stop();
        }
        public bool IsWorking
        {
            get
            {
                return isWorking;
            }
        }
        #endregion
    }
}
