using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using SocketMultiplayerGameServer.Controller;
using SocketGameProtocol;
using SocketMultiplayerGameServer.Tool;
using System.Threading;


namespace SocketMultiplayerGameServer.Servers
{
    class Server
    {
        private Socket socket;
        private UDPServer us;

        private Thread ausThread;

        private List<Client> clientList = new List<Client>();
        private List<Room> roomList = new List<Room>();

        private ControllerManager controllerManager;

        public Server(int port)
        {
            controllerManager = new ControllerManager(this);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
            socket.Listen(0);
            StartAccept();
            Console.WriteLine("TCP服务已启动...");
            us = new UDPServer(6667,this,controllerManager);

        }

        ~Server()
        {
            if (ausThread != null)
            {
                ausThread.Abort();
                ausThread = null;
            }
            
        }

        void StartAccept()
        {
            socket.BeginAccept(AcceptCallback,null);
        }

        void AcceptCallback(IAsyncResult iar)
        {
            Socket client = socket.EndAccept(iar);
            clientList.Add(new Client(client,this,us));
            StartAccept();
        }

        public Client ClientFromUserName(string user)
        {
            foreach(Client c in clientList)
            {
                if (c.GetUserInFo.UserName == user)
                {
                    return c;
                }
            }
            return null;
        }

        public bool SetIEP(EndPoint iPEnd,string user)
        {
            foreach(Client c in clientList)
            {
                if (c.GetUserInFo.UserName==user)
                {
                    c.IEP = iPEnd;
                    //Console.WriteLine("设置IEP： "+c.IEP.ToString());
                    return true;
                }
            }
            return false;
        }


        ////UDP协议

        



        //public class UdpState
        //{
        //    public UdpClient udpClient;
        //    public IPEndPoint ipEndPoint;
        //    public byte[] buffer = new byte[1024];
        //    public int counter = 0;
        //}

        //public class AsyncUdpServer
        //{
        //    //节点
        //    private IPEndPoint iPEndPoint;
        //    private IPEndPoint remoteEP;
        //    //发送和接收的socket
        //    private UdpClient udpReceive;
        //    private UdpClient udpSend;
        //    //端口
        //    private const int listenPort = 6667;
        //    private const int remotePort = 6668;
        //    UdpState udpReceiveState = null;
        //    UdpState udpSendState = null;
        //    //异步状态同步
        //    private ManualResetEvent sendDone = new ManualResetEvent(false);
        //    private ManualResetEvent receiveDone = new ManualResetEvent(false);
        //    public AsyncUdpServer()
        //    {
        //        //本机节点
        //        iPEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
        //        //远程节点
        //        remoteEP = new IPEndPoint(Dns.GetHostAddresses(Dns.GetHostName())[0], remotePort);
        //        //实例化
        //        udpReceive = new UdpClient(iPEndPoint);
        //        udpSend = new UdpClient();
        //        //实例化udpState
        //        udpReceiveState = new UdpState();
        //        udpReceiveState.udpClient = udpReceive;
        //        udpReceiveState.ipEndPoint = iPEndPoint;

        //        udpSendState = new UdpState();
        //        udpSendState.udpClient = udpSend;
        //        udpSendState.ipEndPoint = remoteEP;
        //    }

        //    public void ReceiveMsg()
        //    {
        //        Console.WriteLine("listen for message");
        //        while (true)
        //        {
        //            lock (this)
        //            {
        //                IAsyncResult iar = udpReceive.BeginReceive(ReceiveCallback, udpReceiveState);
        //                receiveDone.WaitOne();
        //                Thread.Sleep(100);
        //            }
        //        }
        //    }





        //    private void ReceiveCallback(IAsyncResult iar)
        //    {
        //        UdpState udpReceiveState = iar.AsyncState as UdpState;
        //        if (iar.IsCompleted)
        //        {
        //            Byte[] receiveBytes = udpReceiveState.udpClient.EndReceive(iar, ref udpReceiveState.ipEndPoint);
        //            //MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(receiveBytes);
        //            Console.WriteLine(Encoding.UTF8.GetString(receiveBytes));
        //            receiveDone.Set();
        //            SendMsg();
        //        }
        //    }

        //    private void SendMsg()
        //    {
        //        udpSend.Connect(udpSendState.ipEndPoint);
        //        udpSendState.udpClient = udpSend;
        //        udpSendState.counter++;

        //        string message = "hello";
        //        Byte[] sendBytes = Encoding.UTF8.GetBytes(message);
        //        udpSend.BeginSend(sendBytes, sendBytes.Length, SendCallback, udpSendState);
        //        sendDone.WaitOne();
        //    }

        //    private void SendCallback(IAsyncResult iar)
        //    {
        //        UdpState udpState = iar.AsyncState as UdpState;
        //        sendDone.Set();
        //    }

        //}


        ////







        public void RemoveClient(Client client)
        {
            clientList.Remove(client);
            client = null;
            //Memory.ClearMemory();
        }


        public void HandleRequest(MainPack pack,Client client)
        {
            controllerManager.HandleRequest(pack, client);
        }

        public MainPack CreateRoom(Client client,MainPack pack)
        {
            try
            {
                Room room = new Room(client, pack.Roompack[0],this);
                roomList.Add(room);
                foreach(PlayerPack p in room.GetPlayerInFo())
                {
                    pack.Playerpack.Add(p);
                }
                pack.Returncode = ReturnCode.Succeed;
                return pack;
            }
            catch
            {
                pack.Returncode = ReturnCode.Fail;
                return pack;
            }
            
        }

        public MainPack FindRoom()
        {
            MainPack pack = new MainPack();
            pack.Actioncode = ActionCode.FindRoom;
            try
            {
                if (roomList.Count == 0)
                {
                    pack.Returncode = ReturnCode.NotRoom;
                    return pack;
                }
                foreach (Room room in roomList)
                {
                    pack.Roompack.Add(room.GetRoomInFo);
                }
                pack.Returncode = ReturnCode.Succeed;
            }
            catch
            {
                pack.Returncode = ReturnCode.Fail;
            }
            return pack;
        } 


        public MainPack JoinRoom(Client client,MainPack pack)
        {
            foreach(Room r in roomList)
            {
                if (r.GetRoomInFo.Roomname.Equals(pack.Str))
                {
                    if (r.GetRoomInFo.Statc == 0)
                    {
                        //可以加入房间
                        r.Join(client);
                        pack.Roompack.Add(r.GetRoomInFo);
                        foreach (PlayerPack p in r.GetPlayerInFo())
                        {
                            pack.Playerpack.Add(p);
                        }
                        pack.Returncode = ReturnCode.Succeed;
                        return pack;
                    }
                    else
                    {
                        //房间不可加入
                        pack.Returncode = ReturnCode.Fail;
                        return pack;
                    }
                }
            }
            //没有此房间
            pack.Returncode = ReturnCode.NotRoom;
            return pack;
        }

        public MainPack ExitRoom(Client client,MainPack pack)
        {
            if (client.GetRoom == null)
            {
                pack.Returncode = ReturnCode.Fail;
                return pack;
            }

            client.GetRoom.Exit(this,client);
            pack.Returncode = ReturnCode.Succeed;
            return pack;
        }

        public void RemoveRoom(Room room)
        {
            roomList.Remove(room);
            room = null;
            //Memory.ClearMemory();
        }

        public void Chat(Client client,MainPack pack)
        {
            pack.Str = client.GetUserInFo.UserName + ":" +pack.Str;
            client.GetRoom.Broadcast(client, pack);
        }


    }
}
