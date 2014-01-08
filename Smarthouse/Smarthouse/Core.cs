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
        public Network client1;
        public Network client2;
        
        public Network server;
        public UserDomain ud;
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
            client1 = new Network("192.168.0.8", 31337, "Smirnyaga", "hard_pass");
            client2 = new Network("192.168.0.8", 31337, "stranger", "12345");

            server = new Network(31337);

            ud = new UserDomain();
            ud.AddUser("stranger", "12345", "some guy");
            ud.AddUser("stranger1", "123456", "some guy");
            ud.AddUser("Smirnyaga", "hard_pass", "some guy");
           
            //test.Start();
            server.Start();
            client1.Start();
            client2.Start();
            
        }
        void watch()
        {
            //watching all's status, doing smth interesting
            do
            {
                //Console.WriteLine("Core! Core!");
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
