using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Smarthouse
{
    class Core
    {
        public Core()
        {
            //Download smth from cfgs
            //Init other classes
            Console console;
            Thread console_thread = new Thread(delegate() { console = new Console(); });
            console_thread.Start();
            //watching all's status, doing smth interesting
            do
            {
                System.Console.WriteLine("Core thread!");
                Thread.Sleep(10000);
            } while (true);
        }
    }
}
