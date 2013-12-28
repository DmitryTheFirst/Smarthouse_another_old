using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Smarthouse
{
    class Console
    {
        public Console()
        {
            StartReading();
        }

        public void StartReading()
        {
            do
            {
                Read(System.Console.ReadLine());
            } while (true);
        }

        void Read(string s)
        {
            //Do smth on read
            Console.WriteLine(s);
        }


        public static void WriteLine(string s)
        {
            System.Console.WriteLine("Smarthouse: " + s);
        }

    }
}
