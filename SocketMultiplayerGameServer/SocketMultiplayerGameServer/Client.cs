using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using ServerForUnity.Tool;
using ServerForUnity.DAO;
using ServerForUnityProtocol;
using MySql.Data.MySqlClient;

namespace ServerForUnity.Severs
{
    class Client
    {
        //private const string connstr = "database=firstdatabase;data source=119.45.237.141;user=root;password=1324545;pooling=false;charset=utf8;port=3306";
        private  const string connstr = "Database=firstdatabase;Data Source=127.0.0.1;Port=3306;UserId=root;Password=1324545;Charset=utf8;";
        private Socket socket;
        private Message message;
        private UserData userData;
        private Server server;
        private MySqlConnection sqlConnection;

        public UserData GetUserData
        {
            get { return userData; }
        }
        public Client (Socket socket,Server server)
        {
            userData = new UserData();
            message = new Message();
            sqlConnection = new MySqlConnection(connstr);

            sqlConnection.Open();

            this.server = server;
            this.socket = socket;

            StartReceive();
        }
        void StartReceive()
        {
            socket.BeginReceive(message.Buffer, message.StartIndex, message.RemSize, SocketFlags.None, ReceiveCallback, null);
            Console.WriteLine("已接收");
        }
        void ReceiveCallback(IAsyncResult iar)
        {
            try
            {
                if (socket == null || socket.Connected == false) return;
                int len = socket.EndReceive(iar);
                
                Console.WriteLine(len+"接收"+iar);
                if (len == 0)
                {
                    Console.WriteLine("接收数据为0");
                    Close();
                    return;
                }
                string str = Encoding.UTF8.GetString(message.Buffer, 0, len);
                Console.WriteLine(str);
                message.ReadBuffer(len, HandleRequest);
                Console.WriteLine(len+"))");
                StartReceive();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                Close();
            }
        }

        public void Send(MainPack pack)
        {
            if (socket == null || socket.Connected == false) return;
            try
            {
                Console.WriteLine("发送的解析");
                socket.Send(Message.PackData(pack));
            }
            catch
            {

            }

        }

        void HandleRequest(MainPack pack)
        {
            Console.WriteLine("hand");
            server.HandleRequest(pack, this);
        }

        public bool Logon(MainPack pack)
        {
            return GetUserData.Logon(pack,sqlConnection);
        }

        private void Close()
        {
            Console.WriteLine("断开");
            server.RemoveClient(this);
            socket.Close();
            sqlConnection.Close();
            server.RemoveClient(this);
        }
    }
}
