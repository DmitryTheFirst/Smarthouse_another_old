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
        ConcurrentDictionary<string, Session> sessions = new ConcurrentDictionary<string, Session>();

        #region Server
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
                server_process_accepted(login);//do on acceptance
            }
            else
            {
                sck.Disconnect(false);
                sck.Close();//reject this user
                return;
            }
            #endregion

        }

        #region Auth
        bool auth(Socket sck, out string login)
        {
            User user;
            bool success = false;
            login = recieveLogin(sck);
            uint append = (uint)(rnd.Next(int.MinValue, int.MaxValue) + int.MaxValue);  // Generate append 


            if (Smarthouse.Program.core.ud.Contains(login)) //if there is such user
            {
                user = Program.core.ud.GetUser(login);
                sendAppend(sck, append);                           //send append
                success = check(sck, Crypt.generateCheckKey(user.Pass, append)); // check key
                if (success)
                {
                    #region Adding login to the sessions
                    if (sessions.TryAdd(login, new Session(append, sck, user.Pass)))
                    {
                        user.Status = (byte)UserDomain.Statuses.Net;
                        success = true;
                    }
                    else
                    {
                        Console.WriteLine("Error! \"" + login + "\" is already on the session list.");
                        success = false;
                    }
                    #endregion
                }
                else
                {
                    Console.WriteLine("Error! \"" + login + "\" wrong password.");
                    success = false;
                }
            }
            else
            {
                Console.WriteLine("Error! No user named \"" + login + "\"");
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
        #endregion










        #region Client
        string login;
        string password;
        public Network(string ip, int port, string login, string password)
        {
            server = false;
            server_ip = IPAddress.Parse(ip);
            this.password = password;
            this.port = port;
            this.login = login;
            reciever = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }
        void endConnect(System.IAsyncResult ar)
        {
            reciever.EndConnect(ar);
            #region Authorization
            if (auth(reciever) && reciever.Connected == true)
            {
                client_process_accepted();
            }
            else
            {
                reciever.Close();
            }
            #endregion
        }
        #region Auth
        bool auth(Socket sck)
        {
            try
            {
                sendLogin(sck, login);               //sending login
                uint append = recieveAppend(sck);   //recieve append
                sessions.TryAdd(login, new Session(append, sck, password));//add session
                sendCheckKey(sck, Crypt.generateCheckKey(password, append)); //send generated key
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
                server_process_recieved(service, sessions[so.fromuser].Crypt.Decode(buff), so.fromuser);
            }
            else
            {
                client_process_recieved(service, sessions[login].Crypt.Decode(buff));
            }

            so.workSocket.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, EndRecieve, so); //вылетает ошибка при обрывании коннекта
        }


        void server_process_accepted(string login)
        {
            Console.WriteLine(login + " connected");
            Console.WriteLine(sessions[login].Crypt.key);
            Console.WriteLine("______________");
            #region Begin recieve
            StateObject so = new StateObject();
            so.workSocket = sessions[login].Sck;
            so.fromuser = login;
            sessions[login].Sck.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, EndRecieve, so);
            #endregion
        }
        void client_process_accepted()
        {
            Console.WriteLine(login);
            Console.WriteLine(sessions[login].Crypt.key);
            Console.WriteLine("______________");
            #region Begin recieve
            StateObject so = new StateObject();
            so.workSocket = sessions[login].Sck;
            sessions[login].Sck.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, EndRecieve, so);
            #endregion

            Send(sessions[login].Sck, sessions[login].Crypt, 23, Encoding.UTF8.GetBytes("Sup"));
        }


        void server_process_recieved(UInt16 service, byte[] buff, string fromuser)
        {
            Console.WriteLine("Server recieved '" + Encoding.UTF8.GetString(buff) + "' for " + service + " service from: '" + fromuser);
        }
        void client_process_recieved(UInt16 service, byte[] buff)
        {
            Console.WriteLine("Client recieved: '" + Encoding.UTF8.GetString(buff) + "' from " + service + " service");
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
                    reciever.BeginConnect(server_ip, port, endConnect, null);
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
        public string fromuser = null;
        public byte[] buffer = new byte[BUFFER_SIZE];
    }

    class Session
    {
        public Socket Sck;
        public Crypt Crypt;
        public Session()
        {
            Crypt = null;
            Sck = null;
        }
        public Session(uint append, Socket sck, string password)
        {
            Sck = sck;
            Crypt = new Crypt(append, password);
        }

    }
}
