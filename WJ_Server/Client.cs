using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Net;

public class Client
{
    public long CustomerID = -199;
    public long Ticks = -1;
    public string code= "&*%";
    public string pwd = "&*%";
    public TcpClient _client;
    public IPEndPoint ip;
    public NetworkStream _stream;
    private List<byte> AllDatas;
    private byte[] recieveData;
    private Int32 ReceiveBufferSize = 5 * 1024;
    public DataRecv DataServer;
    public FileRecv FileServer;
    public System.DateTime StartTime;
    private int State = 0;
    private int len = 0, command = 0;
    public Client(TcpClient client)
    {
        _client = client;
        ip = (IPEndPoint)_client.Client.RemoteEndPoint;
        _stream = client.GetStream();
        AllDatas = new List<byte>();
        recieveData = new byte[ReceiveBufferSize];
        DataServer = new DataRecv();
        FileServer = new FileRecv(this);
        StartTime = System.DateTime.Now;
        Ticks = StartTime.Ticks;
        BeginRead();
        State = 1;
    }

    private void BeginRead()
    {
        try
        {
            if (this._stream == null || !this._stream.CanRead)
            {
                Debug.Error("网络流不可读 关闭");
                close();
                return;
            }

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
        int length = 0;
        try
        {
            length = _stream.EndRead(ar);
        }
        catch
        {
            Debug.Error("接收数据错误，然后退出");
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
            //拷贝到缓存队列
            for (int i = 0; i < length; i++)
            {
                AllDatas.Add(recieveData[i]);
            }
            //===解析数据===
            do
            {
                if (AllDatas.Count > 7)//最小的包应该有8个字节
                {
                    NetHelp.BytesToInt(AllDatas, 0, ref len);//读取消息体的长度
                    len += 4;
                    //读取消息体内容
                    if (len <= AllDatas.Count)
                    {
                        //PackNum++;
                        //Debug.Info("解析出数据包数据：" + PackNum);
                        NetHelp.BytesToInt(AllDatas, 4, ref command);//操作命令
                        byte[] msgBytes = new byte[len - 8];
                        AllDatas.CopyTo(8, msgBytes, 0, msgBytes.Length);
                        AllDatas.RemoveRange(0, len);
                        if (command < 10)
                        {//传输数据
                            try
                            {
                                DataServer.Action(command, msgBytes, this);
                            }
                            catch(Exception ex)
                            {
                                Debug.Error("重大bug--->:" + ex.ToString());
                                close();
                                return;
                            }
                        }
                        else if (command < 20)
                        {//传输图片
                            try
                            {
                                FileServer.Action(command, msgBytes, _stream);
                            }
                            catch(Exception ex)
                            {
                                Debug.Error("重大bug--->:" + ex.ToString());
                                close();
                                return;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }

                }
                else
                {
                    break;
                }
            } while (true);
        }
        //Debug.Info("[接受]--Over");
        BeginRead();
    }
    public void close()
    {
        if (State != -1)
        {
            Debug.Info("【Client】--被动关闭");
            State = -1;
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
            ClientManager.GetInstance().RemoveClient(this);
        }
    }
}

