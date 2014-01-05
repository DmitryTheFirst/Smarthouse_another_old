using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smarthouse
{
    class UserDomain
    {

    }
    class User
    {
        string Pass;
        List<uint> Privileges;
        List<uint> Subscriptions;
        byte Status;
        string About;
        DateTime LastLogin;
    }
}
