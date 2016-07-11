using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System.Net.Sockets;
using ProtoBuf;
using System;
using google.protobuf;
using System.Xml;
using System.Collections.Generic;
using System.Linq; 

[System.Serializable]
public class FileClient
{
    public const string HOST = "112.74.80.245";
    //public const string HOST = "192.168.2.13";
    public const int PORT = 3334;
    public static TcpClient client;
    private byte[] recieveData;
    private  Int32 ReceiveBufferSize = 3 * 1024;
    public ClientStat State = ClientStat.None;
    public Dictionary<string, ClientEvent> Events;
    private ClientEvent eventitem;
    public FileRequest CurrentFile;
    private System.IO.FileStream fs;
    private long lStartPos;
    private static int BLOCK_SIZE = 1024*2;
    Byte[] buffer = new Byte[BLOCK_SIZE];
    public void Init()
    {
        client = new TcpClient();
        Events = new Dictionary<string, ClientEvent>();
        try
        {
            client.Connect(HOST, PORT);
            Debug.Log("File Server Conn Ok！");
        }
        catch
        {
            Debug.Log("File Server Conn Error");
            State = ClientStat.None;
            AddConnAct();//添加重连
            return;
        }
        State = ClientStat.Conn;
        recieveData = new byte[ReceiveBufferSize];
        client.GetStream().BeginRead(recieveData, 0, ReceiveBufferSize, ReceiveMsg, client.GetStream());//在start里面开始异步接收消息
        AddFileEvent();
    }
    private void AddConnAct()
    {
        ClientEvent act = new ClientEvent();
        act.action = ConnServer;
        act.RunState = 0;
        act.CD = 43;
        act.MaxCD = 60 * 5;
        act.StartTime = Time.time;
        Events.Add("Conn", act);
    }
    private void ConnServer()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }
        try
        {
            Debug.Log("连接FileServer");
            client.Connect(HOST, PORT);
        }
        catch
        {
            return;
        }
        Debug.Log("连接FileServer-----Ok");
        State = ClientStat.Conn;
        recieveData = new byte[ReceiveBufferSize];
        client.GetStream().BeginRead(recieveData, 0, ReceiveBufferSize, ReceiveMsg, client.GetStream());//在start里面开始异步接收消息
        Events.Remove("Conn");
        AddFileEvent();
    }

    public void Update()
    {
        if (Events.Count <= 0 || App.Instance.NetCanRun!=1) { return; }
        Dictionary<string, ClientEvent>.Enumerator enumerator = Events.GetEnumerator();
        while (enumerator.MoveNext())
        {
            eventitem = enumerator.Current.Value;
            if (eventitem.RunState == 0)
            {
                if (Time.time - eventitem.StartTime >= eventitem.CD)
                {
                    eventitem.StartTime = Time.time;
                    eventitem.action();
                }
            }
            else if (eventitem.RunState == 1 && (Time.time - eventitem.StartTime >= eventitem.MaxCD))
            {//超时重置
                Debug.Log("超时重置");
                eventitem.RunState = 0;
            }
            else if (eventitem.RunState == 2)
            {//开始开始传输文件命令
                fs = System.IO.File.OpenRead(App.Instance.Data.ImgPath+CurrentFile.PhotoPath);
                fs.Seek(lStartPos, SeekOrigin.Current);
                int iBytes = 0;
                if ((iBytes = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    FileSend send_data = new FileSend();
                    send_data.datas = buffer;
                    Send<FileSend>(2, send_data);
                }
                eventitem.RunState = 3;
            }
            else if(eventitem.RunState==3)
            {//上传文件中
                int iBytes = 0;
                if ((iBytes = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    FileSend send_data = new FileSend();
                    send_data.datas = buffer;
                    Send<FileSend>(2, send_data);
                }
                else
                {//完成
                    Send<FileRequest>(3, CurrentFile);
                }
            }
            else if(eventitem.RunState==4)
            {//确认上传完成
                CloseFile();//关闭文件
                App.Instance.Data.DelectSubmitItem(CurrentFile);//更新xml信息
            }
        }
    }
    int k = 0;
    public void CloseFile()
    {
        if (fs != null)
        {
            fs.Close();
        }
        if (Events.ContainsKey("file"))
        {
            Events["file"].RunState = 0;
            Events["file"].StartTime = Time.time;
        }
    }
    public void AddFileEvent()
    {
        if (Events.ContainsKey("file")) { return; }
        ClientEvent act = new ClientEvent();
        act.action = SelectFile;
        act.RunState = 0;
        act.CD = 10;
        act.MaxCD = 60;
        act.StartTime = Time.time;
        Events.Add("file", act);
    }
    public void SelectFile()
    {
        if (App.Instance.Data.FileRequestDatas.Count > 0)
        {
            Events["file"].RunState = 1;
            CurrentFile = App.Instance.Data.FileRequestDatas.Values.First();
            FileStartRequest request = new FileStartRequest();
            request.name = CurrentFile.PhotoPath;
            request.dir = App.Instance.Data.Set.CustomerID;
            Send<FileStartRequest>(1, request);
            Debug.Log("开始发送：" + CurrentFile);
        }
    }

    /// <summary>
    /// 接受数据
    /// </summary>
    /// <param name="ar"></param>
    public void ReceiveMsg(IAsyncResult ar)//异步接收消息
    {
        NetworkStream stream = (NetworkStream)ar.AsyncState;
        stream.EndRead(ar);
        //读取消息体的长度
        byte[] lenByte = new byte[4];
        System.Array.Copy(recieveData, lenByte, 4);
        int len = BytesToInt(lenByte, 0);
        //读取消息体内容
        byte[] msgByte = new byte[len];
        System.Array.ConstrainedCopy(recieveData, 4, msgByte, 0, len);

        byte[] ResponseTypeByte = new byte[4];
        System.Array.Copy(msgByte, ResponseTypeByte, 4);
        int tp = BytesToInt(ResponseTypeByte, 0);

        byte[] DataByte = new byte[msgByte.Length - 4];
        System.Array.ConstrainedCopy(msgByte, 4, DataByte, 0, DataByte.Length);

        switch (tp)
        {
            case 1://开始发送文件
                FileResponse ResponseModel;
                RecvData<FileResponse>(DataByte, out ResponseModel);
                Debug.Log("===File返回结果:" + ResponseModel.Result);
                lStartPos = ResponseModel.Result;
                Events["file"].RunState = 2;
                break;
            case 2://确认接受完成
                Events["file"].RunState = 4;
                break;

        }
        try
        {
            stream.BeginRead(recieveData, 0, ReceiveBufferSize, ReceiveMsg, stream);
        }
        catch
        {
            Debug.Log("Recv Error");
            CloseFile();
            AddConnAct();
        }
    }
    

    #region 退出
    public  void OnApplicationQuit()
    {
        client.Close();
    }
    public void OnDestroy()
    {
        client.Close();
    }
    #endregion

    #region 工具方法
    public void Send<T>(int type,  T t)
    {

        byte[] msg;
        using (MemoryStream ms = new MemoryStream())
        {
            Serializer.Serialize<T>(ms, t);
            msg = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(msg, 0, msg.Length);
        }
        byte[] type_value = IntToBytes(type);
        byte[] Length_value = IntToBytes(msg.Length + type_value.Length);
        //消息体结构：消息体长度+消息体
        byte[] data = new byte[Length_value.Length + type_value.Length + msg.Length];
        Length_value.CopyTo(data, 0);
        type_value.CopyTo(data, 4);
        msg.CopyTo(data, 8);
        try
        {
            //Debug.Log("send:" + data.Length);
            client.GetStream().Write(data, 0, data.Length);
        }
        catch
        {
            Debug.Log("Send Error");
            CloseFile();
            AddConnAct();
        }
    }
    public void Send(int type)
    {
        byte[] type_value = IntToBytes(type);
        byte[] Length_value = IntToBytes(type_value.Length);
        byte[] data = new byte[Length_value.Length + type_value.Length];
        Length_value.CopyTo(data, 0);
        type_value.CopyTo(data, 4);
        try
        {
            client.GetStream().Write(data, 0, data.Length);
        }
        catch
        {
            AddConnAct();
        }
    }

    public void RecvData<T>(byte[] RecvDataByte,out T t)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            ms.Write(RecvDataByte, 0, RecvDataByte.Length);
            ms.Position = 0;
            t = Serializer.Deserialize<T>(ms);
        }
    }

    public  int BytesToInt(byte[] data, int offset)
    {
        int num = 0;
        for (int i = offset; i < offset + 4; i++)
        {
            num <<= 8;
            num |= (data[i] & 0xff);
        }
        return num;
    }
    public  byte[] IntToBytes(int num)
    {
        byte[] bytes = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            bytes[i] = (byte)(num >> (24 - i * 8));
        }
        return bytes;
    }
    #endregion

}