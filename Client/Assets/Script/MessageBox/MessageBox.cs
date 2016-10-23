using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
/// <summary>
/// 提示框
/// </summary>
public class MessageBox : MonoBehaviour
{
    private static MessageBox _instance;
    public static MessageBox Instance
    {
        get
        {
            return _instance;
        }
    }
    public Button BtnOk;
    public Button BntCancle;
    public GameObject Content;
    public Text txt;
    private CallBack<bool> BackEvent;
    void Awake()
    {
        _instance = this;
        BtnOk.onClick.AddListener(Ok);
        BntCancle.onClick.AddListener(Cancle);
    }
    private  void Ok()
    {
        Content.SetActive(false);
        BackEvent(true);
    }
    private  void Cancle()
    {
        Content.SetActive(false);
        BackEvent(false);
    }
    public void Run(string msg,CallBack<bool> result)
    {
        BackEvent = result;
        txt.text = msg;
        Content.SetActive(true);
    }
}
