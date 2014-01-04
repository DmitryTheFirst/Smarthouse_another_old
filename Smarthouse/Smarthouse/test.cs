using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Smarthouse
{
    class Test : ThreadControllable
    {
        #region Thread stuff
        Thread main_thread;
        ThreadStart start_main;
        bool isWorking;
        #endregion
        byte length;
        char[] arr; 
        
        Random rnd=new Random(47009358);
        const byte init_length = 80;

        public Test()
        {
            arr = new char[]{'а','б','в','г','д','е','ё','ж','з','и','й','к','л','м','н','о','п','р','с','т','у','ф','х','ц','ч','ш','щ','ъ','ы','ь','э','ю','я',
                                     'А','Б','В','Г','Д','Е','Ё','Ж','З','И','Й','К','Л','М','Н','О','П','Р','С','Т','У','Ф','Х','Ц','Ч','Ш','Щ','Ъ','Ы','Ь','Э','Ю','Я'}; 
            length = init_length;

            start_main = new ThreadStart(writeOut);
            main_thread = new Thread(start_main);
        }

        public void ChangeLength(byte newLength)
        {
            length = newLength;
        }
        public void writeOut()
        {
            do
            {
                string s = "";
                for (int i = 0; i < length; i++)
                {
                    s += arr[rnd.Next(0, arr.Length)];
                }
                Console.Write(s);
                System.Threading.Thread.Sleep(1000);
            } while (isWorking);
        }


        #region ThreadControllable
        public void Start()
        { 
            isWorking = true;
            if (main_thread == null) { main_thread = new Thread(start_main); }
            if (!main_thread.IsAlive) { main_thread.Start(); }
           
        }
        public void Stop()
        {
            isWorking = false;
            if (main_thread != null && main_thread.IsAlive)
            {
                main_thread.Abort();
                main_thread = null;
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
