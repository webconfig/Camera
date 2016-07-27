using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

public class Client
{
    public TcpClient _client;
    public NetworkStream _stream;
    private List<byte> AllDatas;
    private byte[] recieveData;
    private Int32 ReceiveBufferSize = 5 * 1024;
    public DataRecv DataServer;
    public FileRecv FileServer;
    private System.DateTime ReadTime;
    private int ReadOutIndex = 0;

    public Client(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();
        AllDatas = new List<byte>();
        recieveData = new byte[ReceiveBufferSize];
        DataServer = new DataRecv();
        FileServer = new FileRecv();
        TimeManager.GetInstance().TimeAction += Client_TimeAction;
        new Thread(new ThreadStart(BeginRead)).Start();
    }

    void Client_TimeAction()
    {
        //TimeSpan ts = DateTime.Now - ReadTime;
        //if(ts.TotalSeconds>20)
        //{//超过20秒没读取到数据，触发一个心跳
        //    if(ReadOutIndex<1)
        //    {
        //        ReadOutIndex = 1;
        //        NetHelp.Send(0, _stream);
        //    }
        //    else
        //    {//连续超过2次
        //        ReadOutIndex = 0;
        //        close();
        //    }
        //}
    }

    private void close()
    {
        TimeManager.GetInstance().TimeAction -= Client_TimeAction;
        if (FileServer != null)
        {
            FileServer.Exit();
            FileServer = null;
        }
        this._stream.Dispose();
        this._stream = null;
        Debug.Info("关闭链接");
        ClientManager.GetInstance().RemoveClient(this);
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
        ReadTime = System.DateTime.Now;
        int length = _stream.EndRead(ar);
        if (length == 0)
        {
            Debug.Error("接收数据长度：" + length);
            close();
            return;
        }
        //if (length < 0)
        //{
        //    Debug.Error("接收数据长度居然小于0：" + length);
        //}
        else
        {
            //拷贝到缓存队列
            for (int i = 0; i < length; i++)
            {
                AllDatas.Add(recieveData[i]);
            }
            //读取消息体的长度
            int len = NetHelp.BytesToInt(AllDatas, 0);
            Debug.Info("接收数据长度：" + length + "|" + len);
            //读取消息体内容
            if (len + 4 <= AllDatas.Count)
            {
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
                    DataServer.Action(tp, msgBytes, _stream);
                }
                else if (tp < 20)
                {//传输图片
                    FileServer.Action(tp, msgBytes, _stream);
                }
            }
        }
        new Thread(new ThreadStart(BeginRead)).Start();
    }
}

        