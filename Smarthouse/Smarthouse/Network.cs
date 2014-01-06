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
            String login;
            reciever.BeginAccept(endAccept, null);//Continue listening to other clients
            #region authorization
            if (auth(sck, out login))
            {
                // Console.WriteLine("+ "+login + " acces granted!");
                Program.core.ud.GetUser(login).Status = (byte)UserDomain.Statuses.Net;
            }
            else
            {
                //Console.WriteLine("+ " +login + " wrong password or already connected!");
                sck.Disconnect(false);
                sck.Close();//reject this user
                return;
            }
            #endregion
            Console.WriteLine(login + " connected");
            Console.WriteLine(sessions[login].Crypt.key);
            Console.WriteLine("______________");

            #region Begin recieve
            StateObject so = new StateObject();
            so.workSocket = sck;
            sck.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, EndRecieve, so);
            #endregion
        }


        #region Auth
        bool auth(Socket sck, out string login)
        {
            User user;
            bool success = false; ;
            login = recieveLogin(sck);
            uint append = (uint)(rnd.Next(int.MinValue, int.MaxValue) + int.MaxValue);  // Generate append 


            if (Smarthouse.Program.core.ud.Contains(login)) //if there is such user
            {
                user = Smarthouse.Program.core.ud.GetUser(login);
                sendAppend(sck, append);                           //send append
                success = check(sck, Crypt.generateCheckKey(user.Pass, append)); // check key
                if (success)
                {
                    #region Adding login to the sessions
                    if (!sessions.TryAdd(login, new Session(append, sck, new Crypt(append, user.Pass))))
                    {
                        //Console.WriteLine("+_______ Error! \"" + login + "\" is already on the session list.");
                        success = false;
                    }
                    #endregion
                }
                else
                {
                    //Console.WriteLine("+_______ Error! \"" + login + "\" wrong password.");
                }
            }
            else
            {
                //Console.WriteLine("+_______ Error! No user named \"" + login + "\"");
                success = false;
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
        void server_process_recieved(UInt16 service, byte[] buff)
        {
            Console.WriteLine("Server recieved '" + Encoding.UTF8.GetString(buff) + "' for " + service + " service");
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

            if (auth(login_client) && sck_client.Connected == true)
            {
                //Console.WriteLine("- " + login_client + " connected!");
            }
            else
            {
                // Console.WriteLine("- " + login_client + " failed!"); 
            }

            Console.WriteLine(login_client);
            Console.WriteLine(crypt_client.key);
            Console.WriteLine("______________");
            #region Begin recieve
            StateObject so = new StateObject();
            so.workSocket = sck_client;
            sck_client.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, EndRecieve, so);
            #endregion
        }


        #region Auth
        bool auth(string login)
        {
            try
            {
                sendLogin(sck_client, login);               //sending login
                append_client = recieveAppend(sck_client);   //recieve append
                crypt_client = new Crypt(append_client, client_password);//create Crypt object
                sendCheckKey(sck_client, Crypt.generateCheckKey(client_password, append_client)); //send generated key

                return true;

            }
            catch
            {
                return false;
            };


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

        void client_process_recieved(UInt16 service, byte[] buff)
        {
            Console.WriteLine("Client recieved: '" + Encoding.UTF8.GetString(buff) + "' from " + service + " service");
        }
        #endregion

        public bool Send(Socket sck, Crypt crypt, UInt16 service, byte[] buff)
        {
            byte[] size = new byte[5];
            Array.Copy(BitConverter.GetBytes(buff.Length), size, 3); //Copy length to array. We need only 3 first bytes
            BitConverter.GetBytes(service).CopyTo(size, 3);//Copy service num to array
            try
            {
                sck.Send(crypt.Encode(size));
                sck.Send(crypt.Encode(buff));
                return true;
            }
            catch
            {
                return false;
            }
        }

        void EndRecieve(System.IAsyncResult ar)
        {
            #region buffwork
            StateObject so = (StateObject)ar.AsyncState;
            byte[] size_buff = new byte[3];
            Array.Copy(so.buffer, size_buff, 3);
            int size = BitConverter.ToUInt16(so.buffer, 0);//getting size
            byte[] buff = new byte[size];
            #endregion
            UInt16 service = BitConverter.ToUInt16(so.buffer, 3);//getting service num
            so.workSocket.Receive(buff);//recieving data

            if (server)
            {
                server_process_recieved(service,buff);
            }
            else
            {
                client_process_recieved(service, buff);
            }

            so.workSocket.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, EndRecieve, so); //вылетает ошибка при обрывании коннекта
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

    class StateObject
    {
        public Socket workSocket = null;
        public const int BUFFER_SIZE = 5; //3 bytes for size and 2 bytes for service num
        public byte[] buffer = new byte[BUFFER_SIZE];
    }

    class Session
    {
        uint append;
        public Socket Sck;
        public Crypt Crypt;

        public Session()
        {
            append = 0;
            Crypt = null;
            Sck = null;
        }

        public Session(uint append, Socket sck, Crypt crypt)
        {
            this.append = append;
            Sck = sck;
            Crypt = crypt;
        }

    }
}
