using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

public class Client
{
    public EndPoint _address;
    public TcpClient _client;
    public NetworkStream _stream;
    private List<byte> AllDatas;
    private int TotalNum = 0;
    private byte[] recieveData;
    private Int32 ReceiveBufferSize = 3 * 1024;
    public DataRecv DataServer;
    public FileRecv FileServer;
    public Client(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();
        _address = client.Client.RemoteEndPoint;
        AllDatas = new List<byte>();
        recieveData = new byte[ReceiveBufferSize];
        DataServer = new DataRecv();
        FileServer = new FileRecv();
        new Thread(new ThreadStart(BeginRead)).Start();
    }

    private void close()
    {
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

        int length = _stream.EndRead(ar);
        if (length == 0)
        {
            Debug.Error("接收数据长度：" + length);
            close();
            return;
        }
        AllDatas.AddRange(recieveData);
        //Buffer.BlockCopy(recieveData, 0, AllDatas, TotalNum, length);
        TotalNum += length;

        //读取消息体的长度
        byte[] lenByte = new byte[4];
        System.Array.Copy(recieveData, lenByte, 4);
        int len = NetHelp.BytesToInt(lenByte, 0);
        Debug.Info("接收数据长度：" + len + "-" + length);
        //读取消息体内容
        if(len+4<= TotalNum)
        {
            byte[] msgByte = new byte[len];
            System.Array.ConstrainedCopy(AllDatas, 4, msgByte, 0, len);

            byte[] ResponseTypeByte = new byte[4];
            System.Array.Copy(msgByte, ResponseTypeByte, 4);
            int tp = NetHelp.BytesToInt(ResponseTypeByte, 0);

            //byte[] DataByte = new byte[msgByte.Length - 4];
            //System.Array.ConstrainedCopy(msgByte, 4, DataByte, 0, DataByte.Length);

            if (tp == 0)
            {
                Debug.Info("【" + _address.ToString() + "】--相应心跳");
            }
            if (tp < 10)
            {//传输数据
                DataServer.Action(tp, msgByte, _stream);
            }
            else if (tp < 20)
            {//传输图片
                FileServer.Action(tp, msgByte, _stream);
            }
        }



        new Thread(new ThreadStart(BeginRead)).Start();
    }
}

        