using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SocketGameProtocol;

namespace SocketMultiplayerGameServer.DAO
{
    class UserData
    {


        public bool Logon(MainPack pack,MySqlConnection sqlConnection)
        {
            string username = pack.Loginpack.Username;
            string password = pack.Loginpack.Password;
            
            try
            {
                string sql = "INSERT INTO `sys`.`userdata` (`username`, `password`) VALUES ('" + username + "', '" + password + "')";
                MySqlCommand comd = new MySqlCommand(sql, sqlConnection);
                
                ////插入数据
                
                comd = new MySqlCommand(sql, sqlConnection);

                comd.ExecuteNonQuery();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            
        }

        public bool Login(MainPack pack, MySqlConnection sqlConnection)
        {
            string username = pack.Loginpack.Username;
            string password = pack.Loginpack.Password;

            string sql = "SELECT * FROM userdata WHERE username='" + username + "' AND password='" + password + "'";
            MySqlCommand cmd = new MySqlCommand(sql, sqlConnection);
            MySqlDataReader read = cmd.ExecuteReader();
            bool result = read.HasRows;
            read.Close();
            return result;
        }

    }
}
