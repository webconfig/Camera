using System;
using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// UI 管理类
/// </summary>
public class UI_Manager:MonoBehaviour
{
    private static UI_Manager _instance;
    public static UI_Manager Instance
    {
        get
        {
            return _instance;
        }
    }
    private Dictionary<string, UI_Base> UI_Datas;
    public UI_Base Current,OldCurrent;
    public Color NormalColor1;
    public Color NormalColor2;
    void Awake()
    {
        _instance = this;
        UI_Datas = new Dictionary<string, UI_Base>();
        for (int i = 0; i < transform.childCount; i++)
        {
            UI_Base ub = transform.GetChild(i).GetComponent<UI_Base>();
            if(ub!=null)
            {
                UI_Datas.Add(ub.gameObject.name, ub);
            }
        }
    }
    public void Init()
    {
        var enumerator = UI_Datas.GetEnumerator();
        while (enumerator.MoveNext())
        {
            enumerator.Current.Value.UI_Init();
        }
    }
    public void Show(string name)
    {
        App.Instance.DisAbleNewWork();
        OldCurrent = Current;
        if (Current != null)
        {
            Current.UI_End();
            Current.gameObject.SetActive(false);
        }
        UI_Datas[name].gameObject.SetActive(true);
        UI_Datas[name].UI_Start();
        Current = UI_Datas[name];
        App.Instance.EnAbleNetWork();
    }
    public void Back()
    {
        App.Instance.DisAbleNewWork();
        if (Current != null)
        {
            Current.UI_End();
            Current.gameObject.SetActive(false);
        }
        OldCurrent.gameObject.SetActive(true);
        OldCurrent.UI_Start();
        Current = OldCurrent;
        OldCurrent = null;
        App.Instance.EnAbleNetWork();
    }
}
public delegate void InputCode(string str);

