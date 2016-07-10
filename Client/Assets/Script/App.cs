using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class App : MonoBehaviour
{
    private static App _instance;
    public static App Instance
    {
        get
        {
            return _instance;
        }
    }
    public AppData Data;
    public MainClient Client;
    public FileClient FileClient;
    private float StartTimeNet=0, CdNet = 2;
    public int NetCanRun=1;
    public bool NetWorkCanDo
    {
        set
        {
            if (!value)
            {
                NetCanRun = 0;
            }
            else
            {
                NetCanRun = 2;
                StartTimeNet = Time.time;
            }
        }
    }
    void Awake()
    {
        _instance = this;
        Data = new AppData();
        Data.Init();
        Client = new MainClient();
        Client.Init();
    }
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        NetCanRun = 1;
        UI_Manager.Instance.Show("UI_Camera");
        FileClient = new FileClient();
        FileClient.Init();
    }

    void Update()
    {
        Client.Update();
        Data.DelSubmitRespinseData();
        if(NetCanRun==2)
        {
            if(Time.time-StartTimeNet>CdNet)
            {
                NetCanRun = 1;
            }
        }
    }

    void LateUpdate()
    {
        FileClient.Update();
    }

    #region 退出
    void OnApplicationQuit()
    {
        Client.OnApplicationQuit();
    }
    void OnDestroy()
    {
        Client.OnDestroy();
    }
    #endregion
}
public delegate void CallBack();
public delegate void CallBack<T>(T t);
public delegate void CallBack<T, V>(T t, V v);
public delegate void CallBack<T, V, U>(T t, V v, U u);
public delegate IEnumerator EnumeCallBack();