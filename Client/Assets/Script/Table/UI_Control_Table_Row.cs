using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UI_Control_Table_Row : MonoBehaviour
{
    [HideInInspector]
    public int index;
    public Button Btn_Del;
    [HideInInspector]
    public UI_Control_Table Table;
 
    public List<UI_Control_Table_Item> Items;
    public Dictionary<string, string> datas;

    public UnityEngine.UI.Image bg;
    [HideInInspector]
    public Color color;

    public void SetColor(Color _color)
    {
        color = _color;
        bg.color = color;
    }

    public void Btn_Delete_Click()
    {
        Table.RowDelete(this);
    }

    public void DataBind(Dictionary<string, string> _datas, CallBack<string,int> Click)
    {
        string key="";
        datas = _datas;
        for (int i = 0; i < Items.Count; i++)
        {
            key=Items[i].key;
            if (datas.ContainsKey(key))
            {
                Items[i].SetValue(datas[key]);
            }
            Items[i].Init(this);
        }
        if(Btn_Del!=null)
        {
            Btn_Del.onClick.RemoveAllListeners();
            Btn_Del.onClick.AddListener(Btn_Delete_Click);
        }
    }

    public void Clear()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].Clear();
        }
    }
}

