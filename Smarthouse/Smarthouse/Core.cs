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
        public Network client;
        public Network server;
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
            client = new Network("127.0.0.1",31337);
            server = new Network(31337);

            UserDomain ud = new UserDomain();
            ud.AddUser("stranger", "12345", "some guy");
            ud.AddUser("stranger1", "123456", "some guy");
            ud.AddUser("Smirnyaga", "hard_pass", "some guy");
            ud.RemoveUser("stranger1");
            ud.ChangeUserPass("stranger1", "1234553426");
            ud.ChangeUserPass("Smirnyaga", "1234553426");
            Console.WriteLine(ud.Count());
            //test.Start();
            //server.Start();
            //client.Start();
            
        }
        void watch()
        {
            //watching all's status, doing smth interesting
            do
            {
                Console.WriteLine("Core! Core!");
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
