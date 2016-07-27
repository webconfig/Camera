using UnityEngine;
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
        DataServer = new MainClient();
        DataServer.Init();
        UI_Manager.Instance.Init();
        UI_Manager.Instance.Show("UI_Camera");
    }
    void LateUpdate()
    {
        DataServer.LateUpdate();
        Data.DelData();
    }

    public void messgae(string str)
    {
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
    #endregion

    //void OnGUI()
    //{
    //    if (GUI.Button(new Rect(10, 10, 150, 50), "send 1"))
    //    {
    //        DataServer.Send1(1);
    //    }
    //    if (GUI.Button(new Rect(10, 110, 150, 50), "send 2"))
    //    {
    //        DataServer.Send1(2);
    //    }
    //    if (GUI.Button(new Rect(10, 210, 150, 50), "send 3"))
    //    {
    //        DataServer.Send1(3);
    //    }
    //    if (GUI.Button(new Rect(10, 210, 150, 50), "send 4"))
    //    {
    //        DataServer.Send1(4);
    //    }
    //}  
}
public delegate void CallBack();
public delegate void CallBack<T>(T t);
public delegate void CallBack<T, V>(T t, V v);
public delegate void CallBack<T, V, U>(T t, V v, U u);
public delegate IEnumerator EnumeCallBack();