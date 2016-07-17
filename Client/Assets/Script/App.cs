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
    public MainClient DataServer;
    private float StartTimeNet=0, CdNet = 2;
    public int NetCanRun=1;
    public event CallBack InputEvent;
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
    }
    void Start()
    {
        Application.targetFrameRate = 20;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        NetCanRun = 1;
        UI_Manager.Instance.Show("UI_Camera");
        DataServer = new MainClient();
        DataServer.Init();
    }

    void LateUpdate()
    {
        if (NetCanRun == 2)
        {
            if (Time.time - StartTimeNet > CdNet)
            {
                NetCanRun = 1;
            }
        }
        else if (NetCanRun == 1)
        {
            DataServer.LateUpdate();
        }
        Data.DelData();
    }


    public void messgae(string str)
    {
        Debug.Log("messgae: " + str);
        if(InputEvent!=null)
        {
            InputEvent();
        }
    }

    #region 退出
    void OnApplicationQuit()
    {
        DataServer.OnApplicationQuit();
    }
    void OnDestroy()
    {
        DataServer.OnDestroy();
    }
    #endregion
}
public delegate void CallBack();
public delegate void CallBack<T>(T t);
public delegate void CallBack<T, V>(T t, V v);
public delegate void CallBack<T, V, U>(T t, V v, U u);
public delegate IEnumerator EnumeCallBack();