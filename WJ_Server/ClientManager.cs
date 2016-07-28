using System.Collections.Generic;
using System.Net.Sockets;

internal class ClientManager
{
    private static ClientManager Instance = new ClientManager();
    public List<Client> _Clients = new List<Client>();

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
        // todo block ip
        string ip = tcp.Client.RemoteEndPoint.ToString().Split(':')[0];


        Client client = new Client(tcp);
        if (_Clients.Contains(client))
            Debug.Info("Client is already exists!");
        else
            _Clients.Add(client);
    }

    public void RemoveClient(Client loginClient)
    {
        if (!this._Clients.Contains(loginClient))
            return;

        this._Clients.Remove(loginClient);
    }

    public void EndClient(long CustomerID, string pwd)
    {
        Client item;
        for (int i = 0; i < _Clients.Count; i++)
        {
            item = _Clients[i];
            if (string.Equals(CustomerID, item.CustomerID) && string.Equals(pwd, item.pwd))
            {
                item.close();
                _Clients.Remove(item);
                return;
            }
        }
    }
}