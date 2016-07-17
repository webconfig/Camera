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
public class MainClient
{
    //public const string HOST = "112.74.80.245";
    //public const string HOST = "192.168.2.13";
    //public const int PORT = 3333;
    public static TcpClient client;
    private byte[] recieveData;
    private  Int32 ReceiveBufferSize = 3 * 1024;
    public ClientStat State = ClientStat.ConnFail;
    public float ConnStartTime,ConnCD = 30;
    public float SendStartTime = 0, SendTimeOut=10;
    public float SendDataStartTime,SendDataCD = 3;
    public float ReLoginStartTime, ReLoginCD = 5;
    public float SendFileStartTime, SendFileCD=5,SendFileTimeOut = 15;
    public float LoginStartTime = 0, LoginTimeOut = 10;
    public void Init()
    {

        if (string.IsNullOrEmpty(App.Instance.Data.Set.DataServer) || string.IsNullOrEmpty(App.Instance.Data.Set.DataPort))
        {
            TipsManager.Instance.Error("服务器配置信息错误");
            GotoConnFail();
            return;
        }

        client = new TcpClient();
        try
        {
            Debug.Log("==连接:" + App.Instance.Data.Set.DataServer + ":" + App.Instance.Data.Set.DataPort);
            client.Connect(App.Instance.Data.Set.DataServer, int.Parse(App.Instance.Data.Set.DataPort));
        }
        catch(Exception ex)
        {
            Debug.Log(ex.ToString());
            TipsManager.Instance.Error("连接服务器失败，单机工作");
            GotoConnFail();
            return;
        }
        Debug.Log("==连接成功==");
        State = ClientStat.Conn;
        recieveData = new byte[ReceiveBufferSize];
        client.GetStream().BeginRead(recieveData, 0, ReceiveBufferSize, ReceiveMsg, client.GetStream());//在start里面开始异步接收消息
        RequestLogin();//请求登录 
    }
    private void ConnServer()
    {
        try
        {
            Debug.Log("重新连接Server");
            client.Connect(App.Instance.Data.Set.DataServer, int.Parse(App.Instance.Data.Set.DataPort));
        }
        catch
        {
            return;
        }
        recieveData = new byte[ReceiveBufferSize];
        client.GetStream().BeginRead(recieveData, 0, ReceiveBufferSize, ReceiveMsg, client.GetStream());//在start里面开始异步接收消息
        RequestLogin();//请求登录 
    }
    #region 登录
    public int LoginResult = 0;
    /// <summary>
    /// 登陆
    /// </summary>
    public void RequestLogin()
    {
        if(App.Instance.Data.Set.CustomerID==0||string.IsNullOrEmpty(App.Instance.Data.Set.Password))
        {//没有用户名和密码不用登录
            Debug.Log("==没有用户名和密码不用登录==");
            TipsManager.Instance.Error("请先设置用户名和密码");
            State = ClientStat.LoginFail;
            return;
        }
        LoginRequest LoginModel = new LoginRequest();
        LoginModel.CustomerID = App.Instance.Data.Set.CustomerID;
        LoginModel.Password = App.Instance.Data.Set.Password;
        Debug.Log("========登录：" + LoginModel.CustomerID + "-" + LoginModel.Password);
        State = ClientStat.Logining;
        LoginStartTime = Time.time;
        Send<LoginRequest>(1, LoginModel);
    }
    /// <summary>
    /// 处理登录结果
    /// </summary>
    public void LoginBack()
    {
        if (LoginResult == 1)
        {//成功
            TipsManager.Instance.Info("登录成功");
            GotoSend();
            RequestGoods();//请求物品
        }
        else
        {
            TipsManager.Instance.Error("用户ID或者用户密码错误");
            State = ClientStat.LoginFail;
        }
    }

    public void ReLogin()
    {
        State = ClientStat.ReLogin;
        ReLoginStartTime = Time.time;
    }
    #endregion
    /// <summary>
    /// 请求Goods
    /// </summary>
    public void RequestGoods()
    {
        Debug.Log("========请求Goods=======");
        GoodsRequest request = new GoodsRequest();
        request.CustomerID = App.Instance.Data.Set.CustomerID;
        Send<GoodsRequest>(2, request);
    }
    public void GotoSendData()
    {
        State = ClientStat.SendDataInit;
    }
    public void GotoSend()
    {
        State = ClientStat.Sending;
        SendStartTime = Time.time;
    }
    public void GotoConnFail()
    {
        Debug.Log("==断线重连==");
        State = ClientStat.ConnFail;
        ConnStartTime = Time.time;
    }

    public void LateUpdate()
    {
        switch (State)
        {
            case ClientStat.ConnFail:
                #region ConnFail
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    return;
                }
                if (Time.time - ConnStartTime >= ConnCD)
                {
                    ConnStartTime = Time.time;
                    //TipsManager.Instance.Info("重新连接服务器");
                    ConnServer();//连接服务器
                }
                #endregion
                break;
            case ClientStat.ReLogin:
                if (Time.time - ReLoginStartTime >= ReLoginCD)
                {
                    CloseFile();
                    RequestLogin();
                }
                break;
            case ClientStat.Logining:
                if (Time.time - LoginStartTime>=LoginTimeOut)
                {//超时
                    GotoConnFail();
                }
                break;
            case ClientStat.LoginBack:
                LoginBack();
                break;
            case ClientStat.SendDataInit:
                SendDataStartTime = Time.time;
                State = ClientStat.SendData;
                break;
            case ClientStat.SendData:
                #region SendData
                if (App.Instance.Data.SubmitDatas.Count > 0)
                {
                    if (Time.time - SendDataStartTime < SendDataCD) { return; }
                    SendDataStartTime = Time.time;
                    Debug.Log("========发送老的数据记录=======:" + App.Instance.Data.SubmitDatas[0].RequestDatas.records.Count);
                    RecordRequest data = App.Instance.Data.SubmitDatas[0].RequestDatas;
                    GotoSend();
                    Send<RecordRequest>(4, data);
                }
                else if (App.Instance.Data.SubmitDataNew.RequestDatas.records.Count > 0)
                {
                    if (Time.time - SendDataStartTime < SendDataCD) { return; }
                    SendDataStartTime = Time.time;
                    Debug.Log("========发送新的当前数据记录=======:" + App.Instance.Data.SubmitDataNew.RequestDatas.records.Count);
                    GotoSend();
                    Send<RecordRequest>(4, App.Instance.Data.SubmitDataNew.RequestDatas);
                }
                else
                {//没有要发送的数据后，开始发送图片
                    GotoSendFile();
                }
                #endregion
                break;
            case ClientStat.SendFile:
                #region SendFile
                switch (SendFileState)
                {
                    case 0:
                        if (Time.time - SendFileStartTime >= SendFileCD)
                        {//发送
                            SendFileStartTime = Time.time;
                            if (!SelectFile())
                            {
                                GotoSendData();//没有发送的文件，跳转到发送数据
                            }
                        }
                        break;
                    case 1:
                        if (Time.time - SendFileStartTime >= SendFileTimeOut)
                        {//超时
                            CloseFile();
                            GotoConnFail();
                        }
                        break;
                    case 2://开始发送文件
                        fs = System.IO.File.OpenRead(App.Instance.Data.ImgPath + CurrentFile.PhotoPath);
                        fs.Seek(lStartPos, SeekOrigin.Current);
                        iBytes = 0;
                        if ((iBytes = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            FileSend send_data = new FileSend();
                            send_data.datas = buffer;
                            Send<FileSend>(12, send_data);
                        }
                        SendFileState = 3;
                        break;
                    case 3://发送过程中
                        iBytes = 0;
                        if ((iBytes = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            FileSend send_data = new FileSend();
                            send_data.datas = buffer;
                            Send<FileSend>(12, send_data);
                        }
                        else
                        {//完成
                            SendFileState = 1;
                            Send<FileRequest>(13, CurrentFile);
                        }
                        break;
                    case 4:
                        SendFileState = 0;
                        Debug.Log("完成文件：" + CurrentFile.PhotoPath);
                        CloseFile();//关闭文件
                        App.Instance.Data.DelectSubmitItem(CurrentFile);//更新xml信息
                        GotoSendData();//发送完成，去发送数据
                        break;
                }
                #endregion
                break;
            case ClientStat.Sending://用于发送请求物品和发送数据的超时
                if (Time.time - SendStartTime >= SendTimeOut)
                {//超时
                    Debug.Log("==超时==");
                    GotoConnFail();
                }
                break;
        }
    }

    #region 传输文件
    public int SendFileState = 0;
    public FileRequest CurrentFile;
    private System.IO.FileStream fs;
    private long lStartPos;
    private static int BLOCK_SIZE = 1024 * 2;
    Byte[] buffer = new Byte[BLOCK_SIZE];
    int iBytes = 0;
    public void GotoSendFile()
    {
        State = ClientStat.SendFile;
        SendFileStartTime = Time.time;
        SendFileState = 0;
    }
    /// <summary>
    /// 查找一个上传的图片
    /// </summary>
    public bool SelectFile()
    {
        ImgXmlData NowImgXml = null;
        CurrentFile = null;
        if (App.Instance.Data.UpLoadImgXmls.Count > 0)
        {
            NowImgXml = App.Instance.Data.UpLoadImgXmls[0];
            if (NowImgXml.FileRequestDatas.Count <= 0)
            {//删除没有记录的文件
                File.Delete(NowImgXml.XmlPath);
                App.Instance.Data.UpLoadImgXmls.RemoveAt(0);
            }
            else
            {
                CurrentFile = NowImgXml.FileRequestDatas.Values.First();
            }
        }
        else
        {
            NowImgXml = App.Instance.Data.NowImgXml;
            if (NowImgXml.FileRequestDatas.Count > 0)
            {
                CurrentFile = NowImgXml.FileRequestDatas.Values.First();
            }
        }

        if (CurrentFile != null)
        {
            SendFileState = 1;
            FileStartRequest request = new FileStartRequest();
            request.name = CurrentFile.PhotoPath;
            request.dir = App.Instance.Data.Set.CustomerID;
            Send<FileStartRequest>(11, request);
            Debug.Log("开始发送文件：" + CurrentFile.PhotoPath);
            return true;
        }
        return false;
    }
    public void CloseFile()
    {
        if (fs != null)
        {
            fs.Close();
        }
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
        //==========================================================================
        if(tp==1)
        {//登陆返回结果
            LoginResponse ResponseModel;
            RecvData<LoginResponse>(DataByte, out ResponseModel);
            LoginResult = ResponseModel.Result;
            State = ClientStat.LoginBack;
        }
        else
        {
            if (State != ClientStat.Logining && State != ClientStat.LoginBack && State !=ClientStat.LoginFail)
            { //登录过程中不接受其他消息
                if (tp == 2)
                {//Goods返回
                    GoodsResponse GoodsResponseModel;
                    try
                    {
                        RecvData<GoodsResponse>(DataByte, out GoodsResponseModel);
                        Debug.Log("===Goods返回结果:" + GoodsResponseModel.result.Count);
                        App.Instance.Data.AddGoods(GoodsResponseModel);
                    }
                    catch { }
                    GotoSendData();
                }
                else if (tp == 3)
                {//返回Record
                    try
                    {
                        RecordResponse RecordResponseModel;
                        RecvData<RecordResponse>(DataByte, out RecordResponseModel);
                        Debug.Log("===Record返回结果:" + RecordResponseModel.records.Count);
                        App.Instance.Data.AddSubmitRespinse(RecordResponseModel);
                    }
                    catch { }
                    GotoSendData();
                }
                else if (tp == 11)
                {//开始发送文件
                    FileResponse ResponseFile;
                    RecvData<FileResponse>(DataByte, out ResponseFile);
                    lStartPos = ResponseFile.Result;
                    SendFileState = 2;
                }
                else if (tp == 12)
                {//接受完成文件
                    SendFileState = 4;
                }
            }
        }
        try
        {
            stream.BeginRead(recieveData, 0, ReceiveBufferSize, ReceiveMsg, stream);
        }
        catch
        {
            Debug.Log("Recv Error");
            GotoConnFail();
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
            GotoConnFail();
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
            CloseFile();
            GotoConnFail();
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
public enum ClientStat
{
    ConnFail,
    Conn,
    ReLogin,
    Logining,
    LoginBack,
    LoginFail,
    LoingOk,
    SendDataInit,
    SendData,
    SendFile,
    Sending,
}