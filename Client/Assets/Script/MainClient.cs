using UnityEngine;
using System.IO;
using System.Net.Sockets;
using ProtoBuf;
using System;
using google.protobuf;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class MainClient
{
    public static TcpClient client;
    private byte[] recieveData;
    private  Int32 ReceiveBufferSize = 3 * 1024;
    public ClientStat State = ClientStat.ConnFail;
    public float ConnStartTime,ConnCD = 15;
    public float SendStartTime = 0, SendTimeOut=10;
    public float ReLoginStartTime, ReLoginCD = 1;
    public float SendFileStartTime,SendFileTimeOut = 15;
    public float LoginStartTime = 0, LoginTimeOut = 10;
    public float SendTime=0, SendCD = 2;
    public bool RttChange = false;
    private bool GetGoods = true;
    public bool ShowLoginOkTip = true;
    public NetworkStream NetStream;
    public void Init()
    {
        recieveData = new byte[ReceiveBufferSize];
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
            State = ClientStat.Conn;
        }
        catch
        {
            TipsManager.Instance.Error("连接服务器失败，脱机工作！");
            GotoConnFail();
            return;
        }
        
    }
    #region 连接登录
    public LoginResponse LoginResult;
    /// <summary>
    /// 重连
    /// </summary>
    IEnumerator  ConnServer()
    {
        try
        {
            client = new TcpClient();
            //Debug.Log("重新连接" + App.Instance.Data.Set.DataServer + ":" + App.Instance.Data.Set.DataPort);
            client.Connect(App.Instance.Data.Set.DataServer, int.Parse(App.Instance.Data.Set.DataPort));
            State = ClientStat.Conn;
        }
        catch
        {
            Debug.Log("连接服务器失败");
        }
        yield return null;
    }
    /// <summary>
    /// 登陆
    /// </summary>
    public void RequestLogin()
    {
        if (App.Instance.Data.Set.CustomerID==0||string.IsNullOrEmpty(App.Instance.Data.Set.Password))
        {//没有用户名和密码不用登录
            Debug.Log("==没有用户名和密码不用登录==");
            TipsManager.Instance.Error("请先设置用户名和密码");
            State = ClientStat.LoginFail;
            return;
        }
        LoginRequest LoginModel = new LoginRequest();
        LoginModel.CustomerID = App.Instance.Data.Set.CustomerID;
        LoginModel.Password = App.Instance.Data.Set.Password;
        LoginModel.CheckCode = App.Instance.Data.Set.CheckCode;
        Debug.Log("========登录：" + LoginModel.CustomerID + "-" + LoginModel.Password);
        State = ClientStat.Logining;
        LoginStartTime = Time.time;
        Send<LoginRequest>(1, LoginModel);
    }
    /// <summary>
    /// 重新登录
    /// </summary>
    public void ReLogin()
    {
        State = ClientStat.ReLogin;
        ReLoginStartTime = Time.time;
    }
    /// <summary>
    /// 重新连接
    /// </summary>
    public void GotoConnFail()
    {
        State = ClientStat.ConnFailStart;
    }
    #endregion
    /// <summary>
    /// 请求Goods
    /// </summary>
    public void RequestGoods()
    {
        GoodsRequest request = new GoodsRequest();
        request.CustomerID = App.Instance.Data.Set.CustomerID;
        Send<GoodsRequest>(2, request);
    }

    #region 传输文件
    public int SendFileState = 0;
    public WJ_Photo_Local CurrentFile;
    private System.IO.FileStream fs;
    private long lStartPos;
    private static int BLOCK_SIZE = 1024 * 2;
    Byte[] buffer = new Byte[BLOCK_SIZE];
    int iBytes = 0;
    ///// <summary>
    ///// 查找一个上传的图片
    ///// </summary>
    //public bool SelectFile()
    //{
    //    CurrentFile = null;
    //    if (App.Instance.Data.OldDatas.Count > 0)
    //    {
    //        for (int i = 0; i < App.Instance.Data.OldDatas.Count; i++)
    //        {
    //            if (App.Instance.Data.OldDatas[i].PhotosSubmit.Count > 0)
    //            {//删除没有记录的文件
    //                CurrentFile = App.Instance.Data.OldDatas[i].PhotosSubmit.Values.First();
    //            }
    //        }
    //    }
    //    else
    //    {
    //        if (App.Instance.Data.CurrentData.PhotosSubmit.Count > 0)
    //        {
    //            CurrentFile = App.Instance.Data.CurrentData.PhotosSubmit.Values.First();
    //        }
    //    }

    //    if (CurrentFile != null)
    //    {
    //        FileStartRequest request = new FileStartRequest();
    //        request.name = CurrentFile.Data.PhotoPath;
    //        request.CustomerID = CurrentFile.Data.CustomerID;
    //        request.WJID = CurrentFile.Data.WJID;
    //        request.AtTime =Convert.ToDateTime(CurrentFile.Data.AtTime).ToString("yyyy-MM-dd");
    //        Send<FileStartRequest>(11, request);
    //        Debug.Log("开始发送文件：" + CurrentFile.Data.PhotoPath);
    //        return true;
    //    }
    //    return false;
    //}
    public void CloseFile()
    {
        if (fs != null)
        {
            fs.Close();
            fs = null;
        }
    }
    #endregion

    #region 接收和处理数据
    /// <summary>
    /// 接受数据
    /// </summary>
    /// <param name="ar"></param>
    public void ReceiveMsg(IAsyncResult ar)//异步接收消息
    {
        int length = 0;
        try
        {
            length = NetStream.EndRead(ar);
        }
        catch
        {
            GotoConnFail();
            return;
        }
        if (length == 0)
        {
            GotoConnFail();
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
    private void BeginRead()
    {
        try
        {
            if (this.NetStream == null || !this.NetStream.CanRead)
                return;
            NetStream.BeginRead(recieveData, 0, ReceiveBufferSize, ReceiveMsg, null);
        }
        catch
        {
            GotoConnFail();
        }
    }
    /// <summary>
    /// 处理数据
    /// </summary>
    public void DelRecvData()
    {
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
            int len = BytesToInt(AllDatas, 0);
            //读取消息体内容
            if (len + 4 <= AllDatas.Count)
            {
                int tp = BytesToInt(AllDatas, 4);//操作命令
                byte[] msgBytes = new byte[len - 4];
                AllDatas.CopyTo(8, msgBytes, 0, msgBytes.Length);
                AllDatas.RemoveRange(0, len + 4);
                #region 具体命令
                if (tp == 0)
                {
                    Send(0);//相应服务器的心跳包
                }
                else if (tp == 1)
                {//登陆返回结果
                    LoginResponse ResponseModel;
                    RecvData<LoginResponse>(msgBytes, out ResponseModel);
                    LoginResult = ResponseModel;
                    State = ClientStat.LoginBack;
                }
                else
                {
                    if (State != ClientStat.Logining && State != ClientStat.LoginBack && State != ClientStat.LoginFail)
                    { //登录过程中不接受其他消息
                        if (tp == 2)
                        {//Goods返回
                            if (!GetGoods)
                            {
                                GoodsResponse GoodsResponseModel;
                                try
                                {
                                    RecvData<GoodsResponse>(msgBytes, out GoodsResponseModel);
                                    Debug.Log("===Goods返回结果:" + GoodsResponseModel.result.Count);
                                    App.Instance.Data.AddGoods(GoodsResponseModel);
                                }
                                catch { }
                                GetGoods = true;
                            }
                            GotoSend();
                        }
                        else if (tp == 3)
                        {//返回Record
                            RecordResponse RecordResponseModel;
                            RecvData<RecordResponse>(msgBytes, out RecordResponseModel);
                            if (RecordResponseModel.record_id == -1)
                            {
                                Debug.Log("传输一个Record发生错误");
                            }
                            else
                            {
                                App.Instance.Data.AddSubmitRespinse(RecordResponseModel);
                            }
                        }
                        else if (tp == 11)
                        {//开始发送文件
                            FileResponse ResponseFile;
                            RecvData<FileResponse>(msgBytes, out ResponseFile);
                            lStartPos = ResponseFile.Result;
                            SendFileState = 2;
                        }
                        else if (tp == 12)
                        {//接受完成文件
                            SendFileState = 4;
                        }
                    }
                }
                #endregion
            }
        }
        //if (StrChange)
        //{
        //    StrChange = false;
        //    if (RunStrsChangeEvent != null)
        //    {
        //        string result = "";
        //        for (int i = 0; i < RunStrs.Count; i++)
        //        {
        //            result = string.Concat(result, RunStrs[i]) + "\n";
        //        }
        //        RunStrsChangeEvent(result.ToString());
        //    }
        //}
    }
    public void GotoSend()
    {
        State = ClientStat.SendDataStart;
        SendTime = Time.time;
    }
    #endregion

    public void DealSend(bool NotSend)
    {
        if(State== ClientStat.NoNetWorkStart)
        {
            if (RttChangeEvent != null)
            {
                RttChangeEvent("网络：<color=#FF0000FF>没有网络</color>");
            }
            State = ClientStat.NoNetWork;
        }
        else if(State== ClientStat.NoNetWork)
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                State = ClientStat.ConnFailStart;
                return;
            }
        }
        else if (State== ClientStat.ConnFailStart)
        {
            if (NetStream != null)
            {
                NetStream.Close();
                NetStream = null;
            }
            if (client != null)
            {
                client.Close();
                client = null;
            }
            ConnStartTime = Time.time;
            State = ClientStat.ConnFail;
        }
        else if (State == ClientStat.ConnFail)
        {//连接失败
            if (NotSend) { return; }
            #region ConnFail
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                State = ClientStat.NoNetWorkStart;
                return;
            }
            if (Time.time - ConnStartTime >= ConnCD)
            {
                Debug.Log("重连服务器");
                App.Instance.StartCoroutine(ConnServer());
            }
            #endregion
        }
        else
        {
            if (Time.time - ConnStartTime >= 5)
            {//超过5秒链接正常，下次断线就马上链接
                ConnStartTime = -100;
            }
            if (State == ClientStat.Conn)
            {//连接成功,开始登录
                if (NotSend) { return; }
                #region 连接成功
                if (RttChangeEvent != null)
                {
                    RttChangeEvent("网络：<color=#00ff00ff>连线</color>");
                }
                NetStream = client.GetStream();
                State = ClientStat.Conn;
                BeginRead();
                Debug.Log("连接到服务器");
                RequestLogin();//请求登录
                #endregion
            }
            else if (State == ClientStat.Logining)
            {//登陆中
                #region 登录超时
                if (Time.time - LoginStartTime >= LoginTimeOut)
                {//超时
                    GotoConnFail();
                }
                #endregion
            }
            else if (State == ClientStat.LoginBack)
            {//登陆返回结果
                #region 完成登录
                if (LoginResult.Result != "0")
                {//成功
                    if (App.Instance.Data.Set.CheckCode)
                    {
                        if (App.Instance.Data.Code != LoginResult.Result)
                        {
                            //UnityEngine.Analytics.TrackEvent("Click URL Website", "app/help/website");
                            Application.OpenURL("http://" + LoginResult.Url);
                        }
                    }
                    if (ShowLoginOkTip)
                    {
                        ShowLoginOkTip = false;
                        TipsManager.Instance.Info("登录成功");
                    }
                    if (!GetGoods)
                    {
                        State = ClientStat.Sending;
                        SendStartTime = Time.time;
                        RequestGoods();//请求物品
                    }
                    else
                    {
                        GotoSend();
                    }
                }
                else
                {
                    TipsManager.Instance.Error("用户ID或者用户密码错误");
                    State = ClientStat.LoginFail;
                }
                #endregion
            }
            else if (State == ClientStat.ReLogin)
            {//重新登录
                if (NotSend) { return; }
                #region 重新登录
                if (Time.time - ReLoginStartTime >= ReLoginCD)
                {
                    Debug.Log("重新登录");
                    CloseFile();
                    RequestLogin();
                }
                #endregion
            }
            else if (State == ClientStat.SendDataStart)
            {
                if (NotSend) { return; }
                #region 开始发送数据
                if (Time.time - SendTime >= SendCD)
                {
                    State = ClientStat.SendData;
                }
                #endregion
            }
            else if (State == ClientStat.SendData)
            {//发送记录
                if (NotSend) { return; }
                if (App.Instance.Data.OldDatas.Count > 0)
                {
                    #region 发送历史数据
                    if (App.Instance.Data.OldDatas[0].Records_Submit.Count > 0)
                    {
                        Debug.Log("发送历史数据记录1条");
                        State = ClientStat.Sending;
                        SendStartTime = Time.time;
                        Send<WJ_Record>(4, App.Instance.Data.OldDatas[0].Records_Submit.Values.First().Data);
                    }
                    else
                    {
                        if(App.Instance.Data.OldDatas[0].PhotosSubmit.Count==0)
                        {//上传完毕
                            App.Instance.Data.OldDatas[0].Over();
                            App.Instance.Data.OldDatas.RemoveAt(0);
                        }
                        else
                        {//发送那天的图片
                            CurrentFile = App.Instance.Data.OldDatas[0].PhotosSubmit.Values.First();
                            StartSendFile();
                        }
                    }
                    #endregion
                }
                else 
                {
                    #region 发送当天数据
                    if (App.Instance.Data.CurrentData.Records_Submit.Count > 0)
                    {
                        Debug.Log("发送当天数据记录1条");
                        State = ClientStat.Sending;
                        SendStartTime = Time.time;
                        Send<WJ_Record>(4, App.Instance.Data.CurrentData.Records_Submit.Values.First().Data);
                    }
                    else
                    {//开始发送当天图片
                        if (App.Instance.Data.CurrentData.PhotosSubmit.Count > 0)
                        {
                            CurrentFile = App.Instance.Data.CurrentData.PhotosSubmit.Values.First();
                            StartSendFile();
                        }
                        else
                        {
                            GotoSend();
                        }
                    }
                    #endregion
                }
            }
            else if (State == ClientStat.SendFile)
            {//发送文件
                #region SendFile
                switch (SendFileState)
                {
                    case 1:
                        if (Time.time - SendFileStartTime >= SendFileTimeOut)
                        {//超时
                            Debug.Log("开始发送文件超时");
                            CloseFile();
                            GotoConnFail();
                        }
                        break;
                    case 2://开始发送文件
                        fs = System.IO.File.OpenRead(App.Instance.Data.ImgPath + CurrentFile.Data.PhotoPath);
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
                        if ((iBytes = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            FileSend send_data = new FileSend();
                            send_data.datas = buffer;
                            Send<FileSend>(12, send_data);
                        }
                        else
                        {//完成
                            SendFileState = 1;
                            Send<WJ_Photo>(13, CurrentFile.Data);
                        }
                        break;
                    case 4:
                        SendFileState = 0;
                        Debug.Log("完成文件：" + CurrentFile.Data.PhotoPath);
                        CloseFile();//关闭文件
                        App.Instance.Data.PhotoSubmitOver(CurrentFile);//更新xml信息
                        GotoSend();//发送完成，去发送数据
                        break;
                }
                #endregion
            }
            else if (State == ClientStat.Sending)
            {//用于发送请求物品和发送数据的超时
                #region 发送超时
                if (Time.time - SendStartTime >= SendTimeOut)
                {//超时
                    Debug.Log("发送数据超时");
                    GotoConnFail();
                }
                #endregion
            }
        }
    }


    private void StartSendFile()
    {
        State = ClientStat.SendFile;
        SendFileState = 1;
        SendFileStartTime = Time.time;
        FileStartRequest request = new FileStartRequest();
        request.name = CurrentFile.Data.PhotoPath;
        request.CustomerID = CurrentFile.Data.CustomerID;
        request.WJID = CurrentFile.Data.WJID;
        request.AtTime = Convert.ToDateTime(CurrentFile.Data.AtTime).ToString("yyyy-MM-dd");
        Send<FileStartRequest>(11, request);
        Debug.Log("开始发送文件：" + CurrentFile.Data.PhotoPath);
    }

    #region 退出
    public void OnApplicationQuit()
    {
        if (NetStream != null)
        {
            NetStream.Close();
            client.Close();
        }
    }
    #endregion

    #region 工具方法
    public  void Send<T>(int type,  T t)
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
            NetStream.Write(data, 0, data.Length);
            NetStream.Flush();
        }
        catch
        {
            Debug.Log("发送文件发生网络异常");
            CloseFile();
            GotoConnFail();
        }
    }
    public  void Send(int type)
    {
        byte[] type_value = IntToBytes(type);
        byte[] Length_value = IntToBytes(type_value.Length);
        //消息体结构：消息体长度+消息体
        byte[] data = new byte[Length_value.Length + type_value.Length];
        Length_value.CopyTo(data, 0);
        type_value.CopyTo(data, 4);
        try
        {
            NetStream.Write(data, 0, data.Length);
        }
        catch
        {
            Debug.Log("发送心跳包发生网络异常");
            CloseFile();
            GotoConnFail();
        }
    }
    public static void RecvData<T>(byte[] RecvDataByte,out T t)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            ms.Write(RecvDataByte, 0, RecvDataByte.Length);
            ms.Position = 0;
            t = Serializer.Deserialize<T>(ms);
        }
    }
    public static int BytesToInt(List<byte> data, int offset)
    {
        int num = 0;
        for (int i = offset; i < offset + 4; i++)
        {
            num <<= 8;
            num |= (data[i] & 0xff);
        }
        return num;
    }
    public static int BytesToInt(byte[] data, int offset)
    {
        int num = 0;
        for (int i = offset; i < offset + 4; i++)
        {
            num <<= 8;
            num |= (data[i] & 0xff);
        }
        return num;
    }
    public static byte[] IntToBytes(int num)
    {
        byte[] bytes = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            bytes[i] = (byte)(num >> (24 - i * 8));
        }
        return bytes;
    }
    #endregion

    public event CallBack<string> RttChangeEvent;
    private List<byte> AddDatas=new List<byte>();
    private List<byte> AllDatas = new List<byte>();
    private bool CanAdd = true;
}
public enum ClientStat
{
    NoNetWorkStart,
    NoNetWork,
    ConnFailStart,
    ConnFail,
    Conn,
    ReLogin,
    Logining,
    LoginBack,
    LoginFail,
    LoingOk,
    SendDataStart,
    SendData,
    SendFile,
    Sending,
}