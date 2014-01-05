using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smarthouse
{
    class UserDomain
    {
        Dictionary<string, User> users = new Dictionary<string, User>();
        enum Statuses : byte { None = 0, Net = 1 };

        public bool AddUser(string login, string pass, string about)
        {
            return AddUser(login, pass, new List<uint>(), new List<uint>(), about);
        }
        public bool AddUser(string login, string pass, List<uint> privileges, List<uint> subscriptions, string about)
        {
            return AddUser(login, pass, privileges, subscriptions, about, DateTime.MinValue);
        }
        public bool AddUser(string login, string pass, List<uint> privileges, List<uint> subscriptions, string about, DateTime lastLogin)
        {
            return AddUser(login, new User(pass, privileges, subscriptions, (byte)Statuses.None, about, lastLogin));
        }
        public bool AddUser(string login, User user)
        {
            try
            {
                users.Add(login, user);
                return true;
            }
            catch (System.ArgumentException)
            {
                return false;
            }
        }

        public bool RemoveUser(string login)
        {
            return users.Remove(login);
        }

        public bool ChangeUserPass(string login, string newPass)
        {
            try
            {
                User current_user = users[login];
                current_user.Pass = newPass;
                return true;
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return false;
            }
        }

        public User GetUser(string login)
        {
            try
            {
                return  users[login];
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return null;
            }
        }

        public int Count()
        {
            return users.Count();
        }
    }
    class User
    {
        public string Pass { get; set; }
        public List<uint> Privileges { get; set; }
        public List<uint> Subscriptions { get; set; }
        public byte Status { get; set; }
        public string About { get; set; }
        public DateTime LastLogin { get; set; }

        public User(string pass, List<uint> privileges, List<uint> subscriptions, byte status, string about, DateTime lastLogin)
        {
            Pass = pass;
            Privileges = privileges;
            Subscriptions = subscriptions;
            Status = status;
            About = about;
            LastLogin = lastLogin;
        }
    }
}
