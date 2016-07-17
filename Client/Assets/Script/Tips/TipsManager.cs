using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
/// <summary>
/// 提示框
/// </summary>
public class TipsManager : MonoBehaviour
{
    private static TipsManager _instance;
    public static TipsManager Instance
    {
        get
        {
            return _instance;
        }
    }
    public List<TipsItem> datas = new List<TipsItem>();
    public TipsItem TipsPrefab;
    public AnimationCurve AnimCure;
    public Color InfoColor=Color.green, ErrorColor=Color.red;
    void Awake()
    {
        InfoColor = Color.green;
        _instance = this;
    }

    public void Info(string msg)
    {
        TipsPrefab.Run(msg, InfoColor);
    }
    public void Error(string msg)
    {
        TipsPrefab.Run(msg, ErrorColor);
    }
}
