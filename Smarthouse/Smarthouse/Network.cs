using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Smarthouse
{
    class Network : ThreadControllable
    {

        const byte append_length = 4;//uint - 32 bytes

        bool isWorking = false;

        bool server;
        IPAddress server_ip;
        int port;


        #region Server
        Dictionary<string, Session> sessions = new Dictionary<string, Session>();
        Random rnd;
        Socket reciever;
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

            string login = recieveLogin(sck); 
            uint append = (uint)(rnd.Next(int.MinValue, int.MaxValue) + int.MaxValue); // Generate append
            #region Adding login to the sessions
            try
            {
                if (Smarthouse.Program.core.ud.Contains(login))
                {
                    sessions.Add(login,
                        new Session(append, sck,
                            new Crypt(append, Smarthouse.Program.core.ud.GetUser(login).Pass)));
                }
            }
            catch { Console.WriteLine("Error! Already on the session list"); };
            #endregion
            sendAppend(sck, append); //send append

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
        #endregion



        #region Client
        Socket sck;
        string login;

        public Network(string ip, int port, string login)
        {
            server = false;
            server_ip = IPAddress.Parse(ip);
            this.port = port;
            this.login = login;
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }

        void endConnect(System.IAsyncResult ar)
        {
            sck.EndConnect(ar);
            //authorization
            if (auth(login))
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
            sendLogin(sck, login);
            uint append = recieveAppend(sck); 

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
        #endregion






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
                    sck.BeginConnect(server_ip, port, endConnect, null);
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
