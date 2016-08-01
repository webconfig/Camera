﻿using UnityEngine;
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
    public MainClient DataServer;
    public event CallBack InputEvent;
    private int NetWork = 0;
    private float NetWorkStartTime = 0, NetWorkCD = 3;
    void Awake()
    {
        _instance = this;
        Data = new AppData();
        Data.Init();
        //Develop();
    }
    AndroidJavaObject android_help;
    void Start()
    {
        //Application.targetFrameRate = 20;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        DataServer = new MainClient();
        UI_Manager.Instance.Init();
        UI_Manager.Instance.Show("UI_Camera");
        DataServer.Init();
    }
    void LateUpdate()
    {
        if (NetWork==0)
        {
            DataServer.DealSend();
            DataServer.DelRecvData();
            Data.DelData();
        }
        else if (NetWork == 2)
        {
            if (Time.time - NetWorkStartTime >= NetWorkCD)
            {
                NetWork = 0;
            }
        }
    }
    public void DisAbleNewWork()
    {
        NetWork = 1;
    }
    public void EnAbleNetWork()
    {
        NetWork = 2;
    }
    public void messgae(string str)
    {
        if(InputEvent!=null)
        {
            InputEvent();
        }
    }
    void OnGUI()
    {
        if (GUI.Button(new Rect(50, 50, 50, 50), "Btn"))
        {
            gameObject.SetActive(false);
        }
    }
    #region 退出
    void OnApplicationQuit()
    {
        DataServer.OnApplicationQuit();
    }
    #endregion
}
public delegate void CallBack();
public delegate void CallBack<T>(T t);
public delegate void CallBack<T, V>(T t, V v);
public delegate void CallBack<T, V, U>(T t, V v, U u);
public delegate IEnumerator EnumeCallBack();