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
        bool isWorking = false;


        bool server;
        IPAddress server_ip;
        int port;



        #region Server
        Socket reciever;
        public Network(int port)
        {
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
                Console.WriteLine("Yaay!");
            }
            else
            {
                Console.WriteLine("Boo!");
            }
        }

        bool auth(Socket sck)
        {
            bool succsess = false;
            login = recieveLogin(sck);
            Console.WriteLine("Client --" + login +"--> Server: ");
            return succsess;
        }

        string recieveLogin(Socket sck)
        {
            byte[] length = new byte[1];
            sck.Receive(length);//recieving length
            byte[] login_buff = new byte[length[0]];
            sck.Receive(login_buff);//recieving login
            return Encoding.UTF8.GetString(login_buff);
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
            if (auth(sck, login))
            {
                Console.WriteLine("Yaay!");
            }
            else
            {
                Console.WriteLine("Boo!");
            }
        }

        bool auth(Socket sck, string login)
        {
            bool succsess = false;
            sendLogin(sck, login);

            return succsess;
        }

        void sendLogin(Socket sck, string login)
        {
            sck.Send(new byte[] { (byte)login.Length });//sending login's length
            sck.Send(Encoding.UTF8.GetBytes(login));//sending login
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
}
