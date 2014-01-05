using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Collections.Concurrent;


namespace Smarthouse
{
    class Network : ThreadControllable
    {

        const byte append_length = 4;//uint - 32 bytes
        const byte md5_length = 32;// md5 hash is always 32 symbols length


        bool isWorking = false;

        bool server;
        IPAddress server_ip;
        int port;


        #region Server



        ConcurrentDictionary<string, Session> sessions = new ConcurrentDictionary<string, Session>();
        Random rnd;
        Socket reciever;
        //string check_key;

        public Network(int port)
        {
            rnd = new Random();
            server = true;
            server_ip = IPAddress.Any;
            reciever = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint myEP = new IPEndPoint(IPAddress.Any, port);
            reciever.Bind(myEP);
            reciever.Listen(10);
        }
        void endAccept(System.IAsyncResult ar)
        {
            Socket sck = reciever.EndAccept(ar);//now sck is our socket connected to client
            reciever.BeginAccept(endAccept, null);//Continue listening to other clients

            //authorization
            if (auth(sck))
            {
                //Console.WriteLine("Yaay!");
            }
            else
            {
                //Console.WriteLine("Boo!");
            }
        }

        bool auth(Socket sck)
        {
            bool success = false;
            User user;

            string login = recieveLogin(sck);
            uint append = (uint)(rnd.Next(int.MinValue, int.MaxValue) + int.MaxValue);  // Generate append 


            #region Adding login to the sessions
            if (Smarthouse.Program.core.ud.Contains(login))
            {
                user = Smarthouse.Program.core.ud.GetUser(login);

                if (!sessions.TryAdd(login, new Session(append, sck, new Crypt(append, user.Pass))))
                {
                    Console.WriteLine("Error! Already on the session list"); return false;
                }
            }
            else
            {
                Console.WriteLine("Error! No such user!"); return false;
            }
            #endregion


            sendAppend(sck, append);                                                    //send append


            if (check(sck, generateCheckKey(user.Pass, append)))
            {
                Console.WriteLine(login + " acces granted!");
            }
            else
            {
                Console.WriteLine(login + " wrong password!");
            }
            return success;
        }
        string recieveLogin(Socket sck)
        {
            byte[] length = new byte[1];
            sck.Receive(length);//recieving length
            byte[] login_buff = new byte[length[0]];
            sck.Receive(login_buff);//recieving login
            return Encoding.UTF8.GetString(login_buff);
        }
        void sendAppend(Socket sck, uint append)
        {
            sck.Send(BitConverter.GetBytes(append));//sending login
        }
        bool check(Socket sck, string check_key)
        {
            byte[] recieved_check_key = new byte[md5_length];
            sck.Receive(recieved_check_key);
            return Encoding.UTF8.GetString(recieved_check_key) == check_key;
        }



        #endregion

        #region Client



        Socket sck_client;
        string login_client;
        Crypt crypt_client;
        uint append_client;
        string client_password;

        public Network(string ip, int port, string login, string password)
        {
            server = false;
            server_ip = IPAddress.Parse(ip);
            client_password = password;
            this.port = port;
            login_client = login;
            sck_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }
        void endConnect(System.IAsyncResult ar)
        {
            sck_client.EndConnect(ar);
            //authorization
            if (auth(login_client))
            {
                // Console.WriteLine("Yaay!");
            }
            else
            {
                // Console.WriteLine("Boo!");
            }
        }
        bool auth(string login)
        {
            bool success = false;
            sendLogin(sck_client, login);               //sending login
            append_client = recieveAppend(sck_client);   //recieve append
            crypt_client = new Crypt(append_client, client_password);//create Crypt object
            sendCheckKey(sck_client, generateCheckKey(client_password, append_client)); //send generated key


            return success;
        }
        void sendLogin(Socket sck, string login)
        {
            sck.Send(new byte[] { (byte)login.Length });//sending login's length
            sck.Send(Encoding.UTF8.GetBytes(login));//sending login
        }
        uint recieveAppend(Socket sck)
        {
            byte[] append_buff = new byte[append_length];
            sck.Receive(append_buff);//recieving login
            return BitConverter.ToUInt32(append_buff, 0);
        }

        void sendCheckKey(Socket sck, string checkkey)
        {
            sck.Send(Encoding.UTF8.GetBytes(checkkey));//sending check key
        }


        #endregion


        string generateCheckKey(string pass, uint append)
        {
            return Crypt.MD5(Crypt.MD5(pass) + Crypt.MD5(append.ToString()));
        }



        #region ThreadControllable
        public void Start()
        {
            if (!isWorking)
            {
                isWorking = true;
                if (server)
                {
                    reciever.BeginAccept(endAccept, null);
                }
                else
                {
                    sck_client.BeginConnect(server_ip, port, endConnect, null);
                }
            }
        }
        public void Stop()
        {
            isWorking = false;
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

    class Session
    {
        uint Append;
        Socket Sck;
        Crypt Crypt;

        public Session()
        {
            Append = 0;
            Crypt = null;
            Sck = null;
        }

        public Session(uint append, Socket sck, Crypt crypt)
        {
            Append = append;
            Sck = sck;
            Crypt = crypt;
        }

    }
}
