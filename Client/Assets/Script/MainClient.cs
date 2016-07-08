using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Net.Sockets;
using ProtoBuf;
using System;
using google.protobuf;

public class MainClient : MonoBehaviour
{
    private const string HOST = "127.0.0.1";
    private const int PORT = 3333;
    public static MainClient instance;
    public static TcpClient client;
    private byte[] recieveData;
    public Int32 ReceiveBufferSize = 3 * 1024;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    void Start()
    {
        if (client == null)
        {
            Connect();
        }
        recieveData = new byte[ReceiveBufferSize];
     
        client.GetStream().BeginRead(recieveData, 0, ReceiveBufferSize, ReceiveMsg, client.GetStream());//在start里面开始异步接收消息
    }
    public void Connect()
    {
        client = new TcpClient();
        client.Connect(HOST, PORT);
    }

    #region 发送请求
    /// <summary>
    /// 登陆
    /// </summary>
    public void RequestLogin()
    {
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
        Send(2);
    }
    #endregion

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
                break;
            case 2://Goods返回
                GoodsResponse GoodsResponseModel;
                RecvData<GoodsResponse>(DataByte, out GoodsResponseModel);
                for (int i = 0; i < GoodsResponseModel.result.Count; i++)
                {
                    Debug.Log("Goods:" + GoodsResponseModel.result[i].GoodsID + "---" + GoodsResponseModel.result[i].GoodsName);
                }
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
    void OnApplicationQuit()
    {
        client.Close();
    }
    void OnDestroy()
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
            client.GetStream().Write(data, 0, data.Length);
        }
        catch
        {
            Debug.Log("Send Error");
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
            Debug.Log("Send Error");
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
    private void OnGUI()
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