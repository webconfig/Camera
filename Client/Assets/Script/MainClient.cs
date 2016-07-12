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
public class MainClient
{
    public const string HOST = "112.74.80.245";
    //public const string HOST = "192.168.2.13";
    public const int PORT = 3333;
    public static TcpClient client;
    private byte[] recieveData;
    private  Int32 ReceiveBufferSize = 3 * 1024;
    public ClientStat State = ClientStat.None;
    public Dictionary<string, ClientEvent> Events;
    private ClientEvent eventitem;
    public void Init()
    {
        client = new TcpClient();
        Events = new Dictionary<string, ClientEvent>();
        try
        {
            client.Connect(HOST, PORT);
        }
        catch
        {
            State = ClientStat.None;
            AddConnAct();//添加重连
            return;
        }
        State = ClientStat.Conn;
        recieveData = new byte[ReceiveBufferSize];
        client.GetStream().BeginRead(recieveData, 0, ReceiveBufferSize, ReceiveMsg, client.GetStream());//在start里面开始异步接收消息
        RequestLogin();//请求登录 
    }
    private void AddConnAct()
    {
        ClientEvent act = new ClientEvent();
        act.action = ConnServer;
        act.RunState = 0;
        act.CD = 30;
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
            Debug.Log("重新连接DataServer");
            client.Connect(HOST, PORT);
        }
        catch
        {
            return;
        }
        recieveData = new byte[ReceiveBufferSize];
        client.GetStream().BeginRead(recieveData, 0, ReceiveBufferSize, ReceiveMsg, client.GetStream());//在start里面开始异步接收消息
        RequestLogin();//请求登录 
        Events.Remove("Conn");
    }
    public void LoginOk()
    {
        RequestGoods();
        ClientEvent act = new ClientEvent();
        act.action = RequestRecord;
        act.RunState = 0;
        act.CD = 5;
        act.MaxCD = 60 * 5;
        act.StartTime = Time.time;
        Events.Add("submit",act);
    }

    #region 发送请求
    /// <summary>
    /// 登陆
    /// </summary>
    public void RequestLogin()
    {
        Debug.Log("========登录=======");
        LoginRequest LoginModel = new LoginRequest();
        LoginModel.CustomerID = "admin";
        LoginModel.Password = "admin";
        Send<LoginRequest>(1, LoginModel);
    }
    /// <summary>
    /// 请求Goods
    /// </summary>
    public void RequestGoods()
    {
        Debug.Log("========请求Goods=======");
        GoodsRequest request = new GoodsRequest();
        request.time = App.Instance.Data.GoodsTimeMax;
        Send<GoodsRequest>(2, request);
    }
    /// <summary>
    /// 发送Record
    /// </summary>
    public void RequestRecord()
    {
        if (App.Instance.Data.SubmitDatas.Count > 0)
        {
            Debug.Log("========RequestRecord=======:" + App.Instance.Data.SubmitDatas[0].RequestDatas.records.Count);
            RecordRequest data = App.Instance.Data.SubmitDatas[0].RequestDatas;
            Events["submit"].RunState = 1;
            Send<RecordRequest>(3, data);
        }
        else
        {
            if(App.Instance.Data.SubmitDataNew.RequestDatas.records.Count>0)
            {
                Debug.Log("========RequestRecord2222=======:" + App.Instance.Data.SubmitDataNew.RequestDatas.records.Count);
                Events["submit"].RunState = 1;
                Send<RecordRequest>(4, App.Instance.Data.SubmitDataNew.RequestDatas);
            }
        }
    }
    #endregion

    public void Update()
    {
        if (State== ClientStat.LoingOk) 
        {
            State = ClientStat.Running;
            LoginOk();
        }
        if (App.Instance.NetCanRun != 1) { return; }
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
            else if(eventitem.RunState==2)
            {
                eventitem.StartTime = Time.time;
                eventitem.RunState = 0;
            }
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
            case 1://登陆返回结果
                LoginResponse ResponseModel;
                RecvData<LoginResponse>(DataByte, out ResponseModel);
                Debug.Log("===登陆返回结果:" + ResponseModel.Result);
                if (ResponseModel.Result == 1)
                {
                    State = ClientStat.LoingOk;
                }
                break;
            case 2://Goods返回
                GoodsResponse GoodsResponseModel;
                RecvData<GoodsResponse>(DataByte, out GoodsResponseModel);
                Debug.Log("===Goods返回结果:" + GoodsResponseModel.result.Count);
                App.Instance.Data.AddGoods(GoodsResponseModel);
                break;
            case 3://返回Record
                Events["submit"].RunState = 2;
                RecordResponse RecordResponseModel;
                RecvData<RecordResponse>(DataByte, out RecordResponseModel);
                Debug.Log("===Record返回结果:" + RecordResponseModel.records.Count);
                App.Instance.Data.AddSubmitRespinse(RecordResponseModel);
                break;
        }
        try
        {
            stream.BeginRead(recieveData, 0, ReceiveBufferSize, ReceiveMsg, stream);
        }
        catch
        {
            Debug.Log("Recv Error");
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
            Debug.Log("send:" + data.Length);
            client.GetStream().Write(data, 0, data.Length);
        }
        catch
        {
            Debug.Log("Send Error");
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

    /// <summary>
    /// 测试
    /// </summary>
    public void OnGUI()
    {
        if (GUI.Button(new Rect(100, 150, 100, 50), "Login"))
        {
            RequestLogin();
        }
        if (GUI.Button(new Rect(200, 150, 100, 50), "Goods"))
        {
            RequestGoods();
        }
    }
}
public class ClientEvent
{
    public CallBack action;
    private int _RunState;
    /// <summary>
    /// 0：初始状态 1：运行中
    /// </summary>
    public int RunState
    {
        set
        {
            _RunState = value;
            if(_RunState==0)
            {
                StartTime = 0;
            }
        }
        get
        {
            return _RunState;
        }
    }
    public float CD;
    public float MaxCD;
    public float StartTime;
}
public enum ClientStat
{
   None,
   Conn,
   LoingOk,
   Running
}