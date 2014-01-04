using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smarthouse
{
    interface ThreadControllable
    {
        void Start();
        void Stop();

        bool IsWorking
        {
            get;
        }

    }
}
