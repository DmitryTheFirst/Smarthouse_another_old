using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Smarthouse
{
    class Program
    {
        public static Core core;
        static void Main(string[] args)
        {
            //Just installed win 8.1(from msdn, yaay!). Configuring git
            System.Console.WriteLine("Hello world!");
            //init core
            core = new Core();
            core.Start();
            do
            {
                //System.Console.WriteLine("Main! Main!");
                Thread.Sleep(10000);
            }while(true);
        }
    }
}
