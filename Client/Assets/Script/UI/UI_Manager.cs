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
    public List<UI_Base> UI;
    private Dictionary<string, UI_Base> UI_Datas;
    public UI_Base Current,OldCurrent;
    public Color NormalColor1;
    public Color NormalColor2;
    void Awake()
    {
        _instance = this;
        UI_Datas = new Dictionary<string, UI_Base>();
        for (int i = 0; i < UI.Count; i++)
        {
            UI_Datas.Add(UI[i].gameObject.name, UI[i]);
        }
    }


    public void Show(string name)
    {
        App.Instance.NetWorkCanDo = false;
        OldCurrent = Current;
        if (Current != null)
        {
            Current.UI_End();
            Current.gameObject.SetActive(false);
        }
        UI_Datas[name].gameObject.SetActive(true);
        UI_Datas[name].UI_Start();
        Current = UI_Datas[name];
        App.Instance.NetWorkCanDo = true;
    }

    public void Back()
    {
        if (Current != null)
        {
            Current.UI_End();
            Current.gameObject.SetActive(false);
        }
        OldCurrent.gameObject.SetActive(true);
        OldCurrent.UI_Start();
        Current = OldCurrent;
        OldCurrent = null;
    }
    
}
public delegate void InputCode(string str);

