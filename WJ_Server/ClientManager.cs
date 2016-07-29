using System.Collections.Generic;
using System.Net.Sockets;
using System;
internal class ClientManager
{
    private static ClientManager Instance = new ClientManager();
    public List<Client> Clients = new List<Client>();
    public List<Client> Clients_Remove = new List<Client>();
    public List<Client> Clients_Add = new List<Client>();
    public List<CustomerData> CustomerEnd = new List<CustomerData>();
    public TimeSpan ts;
    public System.DateTime dt;
    static ClientManager()
    {
    }

    public ClientManager()
    {
        TimeManager.GetInstance().TimeAction += ClientManager_TimeAction;
        Debug.Info("ClientManager Loaded");
    }

    private void ClientManager_TimeAction()
    {
        if(Clients_Remove.Count>0)
        {//删除客户端
            for (int i = 0; i < Clients_Remove.Count; i++)
            {
                if(Clients.Contains(Clients_Remove[i]))
                {
                    Debug.Info("【ClientManager】--删除退出的客户端");
                    Clients.Remove(Clients_Remove[i]);
                }
            }
            Clients_Remove.Clear();
        }
        if (CustomerEnd.Count > 0)
        {//删除客户端
            for (int i = 0; i < CustomerEnd.Count; i++)
            {
                for (int j = 0; j < Clients.Count; j++)
                {
                    if (string.Equals(Clients[j].CustomerID, CustomerEnd[i].CustomerID) && string.Equals(Clients[j].pwd, CustomerEnd[i].pwd) && Clients[j].Ticks != CustomerEnd[i].Ticks)
                    {
                        Debug.Info("【ClientManager】--删除以前账户---" + CustomerEnd[i].CustomerID + ":" + CustomerEnd[i].pwd);
                        Clients[j].Disable();
                        Clients.RemoveAt(j);
                        j--;
                        CustomerEnd.RemoveAt(i);
                        i--;
                        return;
                    }
                }
            }
            CustomerEnd.Clear();
        }
        if (Clients_Add.Count > 0)
        {//添加客户端
            for (int i = 0; i < Clients_Add.Count; i++)
            {
                Debug.Info("【ClientManager】--添加客户端");
                Clients.Add(Clients_Add[i]);
            }
            Clients_Add.Clear();
        }
        dt = System.DateTime.Now;
        for (int i = 0; i < Clients.Count; i++)
        {//执行逻辑

            if(Clients[i].CustomerID<0)
            {
                ts = dt - Clients[i].StartTime;
                if(ts.TotalMinutes>2)
                {//超过2分钟未登陆的注销
                    Clients[i].Disable();
                    Clients.RemoveAt(i);
                    i--;
                    Debug.Info("【ClientManager】--添加客户端 2分钟未登陆的注销");
                    continue;
                }
            }
            Clients[i].Update();
        }
    }

    public static ClientManager GetInstance()
    {
        return Instance;
    }

    public void AddClient(TcpClient tcp)
    {
        Client client = new Client(tcp);
        Clients_Add.Add(client);
    }

    public void RemoveClient(Client item)
    {
        Clients_Remove.Add(item);
    }

    public void EndClient(long CustomerID, string pwd,long Ticks)
    {
        CustomerEnd.Add(new CustomerData(CustomerID, pwd, Ticks));
    }
}
public class CustomerData
{
    public long CustomerID;
    public string pwd;
    public long Ticks;
    public CustomerData(long id,string pd,long ticks)
    {
        CustomerID = id;
        pwd = pd;
        Ticks = ticks;
    }
}