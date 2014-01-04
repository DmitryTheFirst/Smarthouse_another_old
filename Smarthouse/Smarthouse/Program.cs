using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Smarthouse
{
    class Program
    {
        static void Main(string[] args)
        {
            //Just installed win 8.1(from msdn, yaay!). Configuring git
            System.Console.WriteLine("Hello world!");
            //init core
            Core core = new Core();
            core.Start();
            do
            {
                //System.Console.WriteLine("I'm alive! Main thread!");//lol. Don't write Console.WriteLine... Or it will be funny overflow exception :D
                Thread.Sleep(10000);
            }while(true);
        }
    }
}
