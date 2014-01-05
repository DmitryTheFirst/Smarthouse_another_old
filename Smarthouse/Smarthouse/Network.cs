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


        #endregion



        #region Client
        Socket sck;
        public Network(string ip, int port)
        {
            server = false;
            server_ip = IPAddress.Parse(ip);
            this.port = port;
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        void endConnect(System.IAsyncResult ar)
        {
            sck.EndConnect(ar);
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
        #endregion


        bool auth(Socket sck)
        {
            bool succsess = false;
            if (server)
            {
                
            }
            else
            {

            }
            return succsess;
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
