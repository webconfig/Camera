using System;
using UnityEngine;
using UnityEngine.UI;
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

    public MessageBoxType1 BoxType1;

    public void ShowType1(string title, string content, bool hasBtn, CallBack back)
    {
        BoxType1.HasBtn = hasBtn;
        BoxType1.Init(title, content, back);
    }

    public void CloseAll()
    {
        BoxType1.gameObject.SetActive(false);
    }

    void Awake()
    {
        _instance = this;
    }
}