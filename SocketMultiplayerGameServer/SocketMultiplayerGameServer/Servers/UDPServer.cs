﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SocketGameProtocol;
using SocketMultiplayerGameServer.Controller;
using SocketMultiplayerGameServer.Tool;

namespace SocketMultiplayerGameServer.Servers
{
    class UDPServer
    {
        Socket udpServer;//udpsocket
        IPEndPoint bindEP;//本地监听ip
        EndPoint remoteEP;//远程ip

        Server server;

        ControllerManager controllerManager;


        Byte[] buffer = new Byte[1024];//消息缓存

        Thread receiveThread;//接收线程

        public UDPServer(int port,Server server,ControllerManager controllerManager)
        {
            this.server = server;
            this.controllerManager = controllerManager;
            udpServer = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
            bindEP = new IPEndPoint(IPAddress.Any, port);
            remoteEP = (EndPoint)bindEP;
            udpServer.Bind(bindEP);
            receiveThread = new Thread(ReceiveMsg);
            receiveThread.Start();
            Console.WriteLine("UDP服务已启动...");
        }

        ~UDPServer()
        {
            if (receiveThread != null)
            {
                receiveThread.Abort();
                receiveThread = null;
            }
        }

        public void ReceiveMsg()
        {
            while (true)
            {
                int len= udpServer.ReceiveFrom(buffer, ref remoteEP);
                //Console.WriteLine(remoteEP.ToString());
                MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(buffer,0,len);
                Handlerequest(pack, remoteEP);
                //Console.WriteLine(remoteEP.ToString());
                //Thread.Sleep(100);
            }
        }


        public void Handlerequest(MainPack pack,EndPoint iPEndPoint)
        {
            
            Client client = server.ClientFromUserName(pack.User);
            if (client.IEP == null)
            {
                client.IEP = iPEndPoint;
                //if(server.SetIEP(iPEndPoint, pack.User) == false)
                //{
                //    return;
                //}
            }
            //Console.WriteLine(client.IEP.ToString());
            controllerManager.HandleRequest(pack, client,true);
        }

        public void SendTo(MainPack pack,EndPoint point)
        {
            byte[] buff = Message.PackDataUDP(pack);
            udpServer.SendTo(buff, buff.Length, SocketFlags.None, point);
        }
    }
}
