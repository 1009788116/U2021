using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketGameProtocol;
using System.Net;
using System.Reflection;
using SocketMultiplayerGameServer.Servers;

namespace SocketMultiplayerGameServer.Controller
{
    class GameControllerManager
    {
        private Dictionary<RequestCode, BaseController> controlDict = new Dictionary<RequestCode, BaseController>();

        

        public GameControllerManager()
        {

            GameController gameController = new GameController();
            controlDict.Add(gameController.GetRequestCode, gameController);
        }


        public void HandleRequest(MainPack pack,Client client)
        {
            if (controlDict.TryGetValue(pack.Requestcode, out BaseController controller))
            {
                string metname = pack.Actioncode.ToString();
                MethodInfo method = controller.GetType().GetMethod(metname);
                if (method == null)
                {
                    Console.WriteLine("没有找到对应的处理方法");
                    return;
                }
                object[] obj = new object[] { client,pack };
                object ret = method.Invoke(controller, obj);
                if (ret != null)
                {
                    client.SendTo(ret as MainPack);
                    Console.WriteLine("UDP发送响应");
                }
            }
            else
            {
                Console.WriteLine("没有找到对应的controller处理");
            }
        }
    }
}
