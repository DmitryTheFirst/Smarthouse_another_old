using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace Smarthouse
{
    class Network : ThreadControllable
    {
        
        #region Thread stuff
        Thread recieving_thread;
        Socket reciever;
        bool isWorking;
        #endregion
        ThreadStart start_recieve;

        public Network()
        {
            isWorking = false;
            start_recieve = new ThreadStart(recieve);
            recieving_thread = new Thread(start_recieve);
        }

        void recieve()
        {
            do
            {

            } while (isWorking);
        }


        #region ThreadControllable
        public void Start()
        { 
            isWorking = true;
            if (recieving_thread == null) { recieving_thread = new Thread(start_recieve); }
            if (!recieving_thread.IsAlive) { recieving_thread.Start(); }
        }
        public void Stop()
        {
            isWorking = false;
            if (recieving_thread != null && recieving_thread.IsAlive)
            {
                recieving_thread.Abort();
                recieving_thread = null;
            }
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
