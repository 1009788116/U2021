using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketGameProtocol;
using SocketMultiplayerGameServer.Servers;
using System.Net;

namespace SocketMultiplayerGameServer.Controller
{
    class RoomController:BaseController
    {
        public RoomController()
        {
            requestCode = RequestCode.Room;
        }

        public MainPack CreateRoom(Server server, Client client, MainPack pack)
        {
            return server.CreateRoom(client, pack);
        }

        public MainPack FindRoom(Server server, Client client, MainPack pack)
        {
            return server.FindRoom();
        }

        public MainPack JoinRoom(Server server, Client client, MainPack pack)
        {
            return server.JoinRoom(client, pack);
        }

        public MainPack Exit(Server server, Client client, MainPack pack)
        {
            return server.ExitRoom(client, pack);
        }

        public MainPack Chat(Server server, Client client, MainPack pack )
        {
            server.Chat(client, pack);
            return null;
        }

        public MainPack StartGame(Server server, Client client, MainPack pack )
        {
            pack.Returncode= client.GetRoom.StartGame(client);
            return pack;
        }
    }
}
