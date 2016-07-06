using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_Control_Table_Item : MonoBehaviour
{
    public UI_Control_Table_Row row;
    public TableItemType ItemType;
    public string value;
    public string str;
    public UnityEngine.UI.Button.ButtonClickedEvent BtnClickEvent;
     
    public string Temp;
    public string key;

    public void Init(UI_Control_Table_Row _row)
    {
        row = _row;
    }
    public void SetValue(string _val)
    {
        value = _val;
        switch (ItemType)
        {
            case TableItemType.Txt:
                str = String.Format(Temp, value);
                GetComponent<Text>().text = str;
                break;
            case TableItemType.RawImg:
                FileStream fileStream = new FileStream(value, FileMode.Open, FileAccess.Read);
                fileStream.Seek(0, SeekOrigin.Begin);
                //创建文件长度缓冲区
                byte[] bytes = new byte[fileStream.Length];
                //读取文件
                fileStream.Read(bytes, 0, (int)fileStream.Length);
                //释放文件读取流
                fileStream.Close();
                fileStream.Dispose();
                fileStream = null;

                //创建Texture
                texture = new Texture2D(116, 116);
                texture.LoadImage(bytes);
                GetComponent<RawImage>().texture = texture;
                break;
        }
    }
    private Texture2D texture;
    public void Clear()
    {
        texture = null;
    }
}
public enum TableItemType
{
    Txt,
    RawImg
}

