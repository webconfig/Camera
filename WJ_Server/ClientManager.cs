using System.Collections.Generic;
using System.Net.Sockets;
using System;
internal class ClientManager
{
    private static ClientManager Instance = new ClientManager();
    public System.Collections.Concurrent.ConcurrentBag<Client> Clients = new System.Collections.Concurrent.ConcurrentBag<Client>();
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
        Clients.Add(client);
    }

    public void RemoveClient(Client item)
    {
        Debug.Info("【ClientManager】--删除退出的客户端");
        Clients.TryTake(out item);
    }
    public void EndClient(long CustomerID,string Password,string code)
    {
        List<Client> end_items = new List<Client>();
        foreach (var item in Clients)
        {
            if (item.CustomerID == CustomerID && item.pwd == Password && item.code == code)
            {
                end_items.Add(item);
            }
        }
        if(end_items.Count>0)
        {
            for (int i = 0; i < end_items.Count; i++)
            {
                Debug.Info("【ClientManager】--删除以前的客户端--"+ end_items[i].CustomerID);
                end_items[i].close();
            }
        }
        end_items.Clear();
    }
}
