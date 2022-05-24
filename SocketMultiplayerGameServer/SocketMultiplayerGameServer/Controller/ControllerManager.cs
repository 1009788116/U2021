using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using SocketGameProtocol;
using System.Reflection;
using SocketMultiplayerGameServer.Servers;

namespace SocketMultiplayerGameServer.Controller
{
    class ControllerManager
    {
        private Dictionary<RequestCode, BaseController> controlDict = new Dictionary<RequestCode, BaseController>();

        private Server server;
        public ControllerManager(Server server)
        {
            this.server = server;


            UserController userController = new UserController();
            controlDict.Add(userController.GetRequestCode,userController);
            RoomController roomController = new RoomController();
            controlDict.Add(roomController.GetRequestCode, roomController);
            GameController gameController = new GameController();
            controlDict.Add(gameController.GetRequestCode, gameController);
        }


        public void HandleRequest(MainPack pack,Client client,bool isUDP=false)
        {
            if(controlDict.TryGetValue(pack.Requestcode,out BaseController controller))
            {
                string metname = pack.Actioncode.ToString();
                MethodInfo method = controller.GetType().GetMethod(metname);
                if (method == null)
                {
                    Console.WriteLine("没有找到对应的处理方法");
                    return;
                }
                object[] obj;
                if (isUDP)//UDP
                {
                    obj = new object[] { client, pack };
                    method.Invoke(controller, obj);
                }
                else//TCP
                {
                    obj = new object[] { server, client, pack };
                    object ret = method.Invoke(controller, obj);
                    if (ret != null)
                    {
                        client.Send(ret as MainPack);
                        Console.WriteLine("发送数据：");
                    }
                }
               
            }
            else
            {
                Console.WriteLine("没有找到对应的controller处理");
            }
        }
    }
}
