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
            test.Start();
            do
            {
                Console.WriteLine("Main thread!");
                Thread.Sleep(10000);
                
            } while (true);
        }
    }
}
