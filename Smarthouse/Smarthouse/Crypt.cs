using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Smarthouse
{
    class Crypt
    {
        const string crypto_const = "22";//just becouse. It's not "magic", but i just want it to be there. sorry if it offends your feelings..
        public string key;
        public Crypt(uint append, string password)
        {
            key = MD5(MD5(password+append)+crypto_const);
        }
        

        public static string MD5(string s)
        {
            MD5CryptoServiceProvider CSP = new MD5CryptoServiceProvider();
            byte[] byteHash = CSP.ComputeHash(Encoding.UTF8.GetBytes(s));
            string hash = string.Empty;
            foreach (byte b in byteHash)
                hash += string.Format("{0:x2}", b);
            return hash;
        }
    }
}
