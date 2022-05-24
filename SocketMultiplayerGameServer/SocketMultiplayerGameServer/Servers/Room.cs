using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketGameProtocol;
using Google.Protobuf.Collections;
using System.Threading;

namespace SocketMultiplayerGameServer.Servers
{
    class Room
    {
        private RoomPack roominfo;//房间信息
        private Server server;
        private List<Client> clientList = new List<Client>();//房间内所有的客户端

        /// <summary>
        /// 返回房间信息
        /// </summary>
        public RoomPack GetRoomInFo
        {
            get
            {
                roominfo.Curnum = clientList.Count;
                return roominfo;
            }
        }

        public Room(Client client,RoomPack pack,Server server)
        {
            roominfo = pack;
            clientList.Add(client);
            client.GetRoom = this;
            this.server = server;
        }

        
        public RepeatedField<PlayerPack> GetPlayerInFo()
        {
            RepeatedField<PlayerPack> pack = new RepeatedField<PlayerPack>();
            foreach (Client c in clientList)
            {
                PlayerPack player = new PlayerPack();
                player.Playername = c.GetUserInFo.UserName;
                pack.Add(player);
            }
            return pack;
        }
        

        public void Broadcast(Client client,MainPack pack)
        {
            foreach(Client c in clientList)
            {
                if (c.Equals(client))
                {
                    continue;
                }
                c.Send(pack);
            }
        }

        public void BroadcastTo(Client client,MainPack pack)
        {
            //Console.WriteLine("广播数据");
            foreach(Client c in clientList)
            {
                if (c.Equals(client))
                {
                    continue;
                }
                c.SendTo(pack);
            }
        }


        public void Damage(MainPack pack,Client cc)
        {
            BulletHitPack bulletHitPack = pack.Bullethitpack;
            PosPack posPack = null;
            Client client = null;
            foreach(Client c in clientList)
            {
                if (c.GetUserInFo.UserName == bulletHitPack.Hituser)
                {
                    posPack = c.GetUserInFo.Pos;
                    client = c;
                    break;
                }
            }

            double distance = Math.Sqrt(Math.Pow((bulletHitPack.PosX - posPack.PosX), 2) + Math.Pow((bulletHitPack.PosY - posPack.PosY), 2));
            
            Console.WriteLine(cc.GetUserInFo.UserName+" 击中 " + bulletHitPack.Hituser + " 距离 " + distance);

            if (distance < 0.7f)
            {
                //击中

                Broadcast(null, pack);
            }
        }


        public void Join(Client client)
        {
            clientList.Add(client);
            if (clientList.Count >= roominfo.Maxnum)
            {
                //满人了
                roominfo.Statc = 1;
            }
            client.GetRoom = this;
            MainPack pack = new MainPack();
            pack.Actioncode = ActionCode.PlayerList;
            foreach(PlayerPack player in GetPlayerInFo())
            {
                pack.Playerpack.Add(player);
            }
            Broadcast(client, pack);
        }

        public void Exit(Server server,Client client)
        {
            MainPack pack = new MainPack();
            if (roominfo.Statc == 2)//游戏已经开始
            {
                ExitGame(client);
            }
            else//游戏未开始
            {
                if (client == clientList[0])
                {
                    //房主离开
                    client.GetRoom = null;
                    pack.Actioncode = ActionCode.Exit;
                    Broadcast(client, pack);
                    server.RemoveRoom(this);
                    return;
                }
                clientList.Remove(client);
                roominfo.Statc = 0;
                client.GetRoom = null;
                pack.Actioncode = ActionCode.PlayerList;
                foreach (PlayerPack player in GetPlayerInFo())
                {
                    pack.Playerpack.Add(player);
                }
                Broadcast(client, pack);
            }

            
        }

        public ReturnCode StartGame(Client client)
        {
            if (client != clientList[0])
            {
                return ReturnCode.Fail;
            }
            roominfo.Statc = 2;
            Thread starttime=new Thread(Time);
            starttime.Start();
            Console.WriteLine("开始游戏");
            return ReturnCode.Succeed;
        }

        private void Time()
        {
            MainPack pack=new MainPack();
            pack.Actioncode = ActionCode.Chat;
            pack.Str = "房主已启动游戏...";
            Broadcast(null,pack);
            Thread.Sleep(1000);
            for (int i = 5; i > 0; i--)
            {
                pack.Str = i.ToString();
                Broadcast(null,pack);
                Thread.Sleep(1000);
            }

            pack.Actioncode = ActionCode.Starting;
            
            
            foreach (var VARIABLE in clientList)
            {
                PlayerPack player=new PlayerPack();
                VARIABLE.GetUserInFo.HP = 100;
                player.Playername = VARIABLE.GetUserInFo.UserName;
                player.Hp = VARIABLE.GetUserInFo.HP;
                pack.Playerpack.Add(player);
            }
            Broadcast(null,pack);
        }

        public void ExitGame(Client client)
        {
            MainPack pack=new MainPack();
            if (client == clientList[0])
            {
                //房主退出
                pack.Actioncode = ActionCode.ExitGame;
                pack.Str = "r";
                Broadcast(client,pack);
                server.RemoveRoom(this);
                client.GetRoom = null;
            }
            else
            {
                //其他成员退出
                clientList.Remove(client);
                client.GetRoom = null;
                pack.Actioncode = ActionCode.UpCharacterList;
                foreach (var VARIABLE in clientList)
                {
                    PlayerPack playerPack=new PlayerPack();
                    playerPack.Playername = VARIABLE.GetUserInFo.UserName;
                    playerPack.Hp = VARIABLE.GetUserInFo.HP;
                    pack.Playerpack.Add(playerPack);
                }
                pack.Str = client.GetUserInFo.UserName;
                Broadcast(client,pack);
            }
        }
    }
}
