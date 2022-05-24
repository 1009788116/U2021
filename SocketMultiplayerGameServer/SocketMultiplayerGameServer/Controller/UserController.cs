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
    class UserController:BaseController
    {
        public UserController()
        {
            requestCode = RequestCode.User;
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <returns></returns>
        public MainPack Logon(Server server,Client client,MainPack pack)
        {
            if(client.GetUserData.Logon(pack,client.GetMysqlConnect))
            {
                pack.Returncode = ReturnCode.Succeed;
            }
            else
            {
                pack.Returncode = ReturnCode.Fail;
            }
            return pack;
        }


        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public MainPack Login(Server server, Client client, MainPack pack)
        {
            if (client.GetUserData.Login(pack, client.GetMysqlConnect))
            {
                pack.Returncode = ReturnCode.Succeed;
                client.GetUserInFo.UserName = pack.Loginpack.Username;
            }
            else
            {
                pack.Returncode = ReturnCode.Fail;
            }
            return pack;
        }
    }
}
