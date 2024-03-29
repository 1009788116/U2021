﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using SocketMultiplayerGameServer.Tool;
using SocketMultiplayerGameServer.DAO;
using SocketGameProtocol;
using MySql.Data.MySqlClient;

namespace SocketMultiplayerGameServer.Servers
{
    class Client
    {
        private const string connstr = "database=firstdatabase;data source=127.0.0.1;user=root;password=1324545;pooling=false;charset=utf8;port=3306";

        private Socket socket;
        //private Socket udpClient;
        private EndPoint remoteEp;
        private Message message;
        private UserData userData;
        private Server server;
        private MySqlConnection sqlConnection;

        public UDPServer us;

        public UserInFo GetUserInFo
        {
            get;
            set;
        }

        public class UserInFo
        {
            public string UserName
            {
                get;set;
            }

            public int HP
            {
                set;
                get;
            }

            public PosPack Pos
            {
                get;
                set;
            }
        }
        
        public Room GetRoom
        {
            get;set;
        }

        public EndPoint IEP
        {
            get
            {
                return remoteEp;
            }
            set
            {
                remoteEp = value;
            }
        }

        

        public UserData GetUserData
        {
            get{ return userData; }
        }

        public MySqlConnection GetMysqlConnect
        {
            get
            {
                return sqlConnection;
            }
        }
        public Client(Socket socket,Server server,UDPServer us)
        {
            userData = new UserData();
            message = new Message();
            sqlConnection = new MySqlConnection(connstr);
            GetUserInFo=new UserInFo();

            sqlConnection.Open();

            this.us = us;
            this.server = server;
            this.socket = socket;
            

            StartReceive();
        }

        void StartReceive()
        {
            socket.BeginReceive(message.Buffer, message.StartIndex, message.Remsize, SocketFlags.None, ReceiveCallback, null);
        }

        void ReceiveCallback(IAsyncResult iar)
        {
            try
            {
                if (socket == null || socket.Connected == false) return;
                int len = socket.EndReceive(iar);
                Console.WriteLine("接收");
                if (len == 0)
                {
                    Console.WriteLine("接收数据为0");
                    Close();
                    return;
                }

                message.ReadBuffer(len, HandleRequest);
                StartReceive();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Close();
            }
        }

        public void Send(MainPack pack)
        {
            if (socket == null || socket.Connected == false) return;
            try
            {
                socket.Send(Message.PackData(pack));
            }
            catch
            {

            }
            
        }


        public void SendTo(MainPack pack)
        {
            if (IEP == null) return;
            us.SendTo(pack, IEP);
        }
        


        void HandleRequest(MainPack pack)
        {
            server.HandleRequest(pack, this);
        }


        private void Close()
        {
            Console.WriteLine("断开");
            if (GetRoom != null)
            {
                GetRoom.Exit(server,this);
            }
            socket.Close();
            sqlConnection.Close();
            server.RemoveClient(this);
        }

        
        public void UpPos(MainPack pack)
        {
            GetUserInFo.Pos = pack.Playerpack[0].Pospack;
        }

    }
}
