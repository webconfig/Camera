﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
internal class NetworkFactory
{
    private static NetworkFactory Instance;
    private static TcpListener NetworkListener;

    public NetworkFactory()
    {
        new Thread(new ThreadStart(NetworkStart)).Start();
    }

    public static NetworkFactory GetInstance()
    {
        return (Instance != null) ? Instance : Instance = Instance = new NetworkFactory();
    }

    private void NetworkStart()
    {
        try
        {
            NetworkListener = new TcpListener(new System.Net.IPEndPoint(0, 3333));
            NetworkListener.Start();
            Debug.Info(string.Format("开始监听{0}", 3333));
            NetworkListener.BeginAcceptTcpClient(new AsyncCallback(BeginAcceptTcpClient), (object)null);
        }
        catch (Exception ex)
        {
            Debug.Error("NetworkStart:" + ex);
        }
    }

    private void BeginAcceptTcpClient(IAsyncResult ar)
    {
        try
        {
            TcpClient tcpClient = NetworkListener.EndAcceptTcpClient(ar);
            Accept(tcpClient);
        }
        catch
        {

        }
        NetworkListener.BeginAcceptTcpClient(new AsyncCallback(this.BeginAcceptTcpClient), (object)null);
    }

    private void Accept(TcpClient tcpClient)
    {
        ClientManager.GetInstance().AddClient(tcpClient);
    }
}
