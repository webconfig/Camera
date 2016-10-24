using System.Collections.Generic;
using System.Net.Sockets;
using System;
internal class ClientManager
{
    private static ClientManager Instance = new ClientManager();
    public List<Client> Clients = new List<Client>();
    public object clients_obj = new object();
    static ClientManager()
    {
    }

    public ClientManager()
    {
        Debug.Info("ClientManager Loaded");
    }
    public static ClientManager GetInstance()
    {
        return Instance;
    }

    public void AddClient(TcpClient tcp)
    {

        Client client = new Client(tcp);
        Debug.Info("【ClientManager】--添加客户端:" + client.ip.Address.ToString() + ":" + client.ip.Port.ToString());
        lock (clients_obj)
        {
            Clients.Add(client);
        }
    }

    public void RemoveClient(Client item)
    {
        Debug.Info(string.Format("[客户端id：{0},挖机号{1}]-->【ClientManager】--删除退出的客户端", item.CustomerID, item.code));
        lock (clients_obj)
        {
            if(Clients.Contains(item))
            {
                Clients.Remove(item);
            }
        }
    }
    public void EndClient(long CustomerID,string Password,string code)
    {
        lock (clients_obj)
        {
            Client item;
            for (int i = 0; i < Clients.Count; i++)
            {
                item = Clients[i];
                if(item.CustomerID == CustomerID && item.pwd == Password && item.code == code)
                {
                    Debug.Info(string.Format("[客户端id：{0},挖机号{1}]-->【ClientManager】--删除以前的客户端", item.CustomerID, item.code));
                    item.close();
                    Clients.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
