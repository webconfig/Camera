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
    void Awake()
    {
        _instance = this;
    }

    public void RunItem(string msg)
    {

        for (int i = 0; i < datas.Count; i++)
        {
            if(!datas[i].gameObject.activeInHierarchy)
            {
                datas[i].Run(msg);
                return;
            }
        }
        TipsItem scirpt = (Instantiate(TipsPrefab.gameObject) as GameObject).GetComponent<TipsItem>();
        scirpt.gameObject.transform.parent = transform;
        scirpt.gameObject.transform.localPosition = Vector3.zero;
        scirpt.gameObject.transform.localScale = new Vector3(1, 1, 1);
        scirpt.Run(msg);
        datas.Add(scirpt);
    }
}
