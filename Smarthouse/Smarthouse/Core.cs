using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Smarthouse
{
    class Core
    { 
        public Test test;
        public Core()
        {
            Random rnd = new Random();
            //Download smth from cfgs
            //Init other classes
            test = new Test();
            //watching all's status, doing smth interesting
            do
            {
                Console.WriteLine("Main thread!");
                Thread.Sleep(3000);
                test.Stop();
                test.Start();
            } while (true);
        }
    }
}
