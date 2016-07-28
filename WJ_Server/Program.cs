using Dos.ORM;
using Dos.Model;
using System;
using System.Diagnostics;
namespace WJ_Server
{
    public class Program
    {
        public static Wj_Code Current;
        static void Main()
        {
            Console.Title = "WJ Server";
            TimeManager.GetInstance();
            ClientManager.GetInstance();
            NetworkFactory.GetInstance();
            //==============
            try
            {
                Db.Context = new DbSession(DatabaseType.SqlServer, "data source=.;initial catalog=RS_PIS;user id=wj;pwd=Mc111111");
                Current = Db.Context.From<Wj_Code>().First();
            }
            catch
            {
                Debug.Error("数据库异常！启动失败");
                return;
            }
            Debug.Info("服务器启动");
            Process.GetCurrentProcess().WaitForExit();
        }
    }
}
