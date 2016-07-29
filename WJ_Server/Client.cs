using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

public class Client
{
    public long CustomerID = -199;
    public long Ticks = -1;
    public string  pwd="&*%";
    public TcpClient _client;
    public NetworkStream _stream;
    private List<byte> AddDatas;
    private List<byte> AllDatas;
    private bool CanAdd=true,CanRun=true;
    private byte[] recieveData;
    private Int32 ReceiveBufferSize = 5 * 1024;
    public DataRecv DataServer;
    public FileRecv FileServer;
    public System.DateTime StartTime;
    private int State = 0;

    public Client(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();
        AllDatas = new List<byte>();
        AddDatas = new List<byte>();
        recieveData = new byte[ReceiveBufferSize];
        DataServer = new DataRecv();
        FileServer = new FileRecv();
        StartTime = System.DateTime.Now;
        Ticks = StartTime.Ticks;
        BeginRead();
        State = 1;
    }

    public void Disable()
    {
        Debug.Info("Client--Disable");
        if (FileServer != null)
        {
            FileServer.Exit();
            FileServer = null;
        }
        if (DataServer != null)
        {
            DataServer = null;
        }
        if (_stream != null)
        {
            this._stream.Dispose();
            this._stream = null;
        }
    }

    public void close()
    {
        if (State != -1)
        {
            Debug.Info("【Client】--被动关闭");
            State = -1;
            Disable();
            ClientManager.GetInstance().RemoveClient(this);
        }
    }

    private void BeginRead()
    {
        try
        {
            if (this._stream == null || !this._stream.CanRead)
                return;
            _stream.BeginRead(recieveData, 0, ReceiveBufferSize, new AsyncCallback(OnReceiveCallback), null);
        }
        catch (Exception ex)
        {
            Debug.Error("[Client]: BeginRead() Exception" + ex);
            close();
        }
    }

    private void OnReceiveCallback(IAsyncResult ar)
    {
        Debug.Info("【Client】--接收数据");
        int length = 0;
        try
        {
             length = _stream.EndRead(ar);
        }
        catch
        {
            close();
            return;
        }
        if (length == 0)
        {
            Debug.Error("接收数据长度：" + length);
            close();
            return;
        }
        else if (length > 0)
        {
            CanAdd = false;
            //拷贝到缓存队列
            for (int i = 0; i < length; i++)
            {
                AddDatas.Add(recieveData[i]);
            }
            CanAdd = true;
        }
        BeginRead();
    }

    public void Update()
    {
        if (!CanRun)
        {
            return;
        }
        CanRun = false;
        if (CanAdd)
        {
            if (AddDatas.Count > 0)
            {
                AllDatas.AddRange(AddDatas);
                AddDatas.Clear();
            }
        }
        if (AllDatas.Count > 0)
        {
            //读取消息体的长度
            int len = NetHelp.BytesToInt(AllDatas, 0);
            //读取消息体内容
            if (len + 4 <= AllDatas.Count)
            {
                Debug.Info("【Client】--处理数据");
                int tp = NetHelp.BytesToInt(AllDatas, 4);//操作命令
                byte[] msgBytes = new byte[len - 4];
                AllDatas.CopyTo(8, msgBytes, 0, msgBytes.Length);
                AllDatas.RemoveRange(0, len + 4);
                if (tp == 0)
                {
                    Debug.Info("相应心跳");
                }
                if (tp < 10)
                {//传输数据
                    DataServer.Action(tp, msgBytes, this);
                }
                else if (tp < 20)
                {//传输图片
                    FileServer.Action(tp, msgBytes, _stream);
                }
            }
        }
        CanRun = true;
    }
}

        