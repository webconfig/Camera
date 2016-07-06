using System;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxType1 : MonoBehaviour
{
    public event CallBack BackEvent;
    public Text Title_UI;
    public string Title
    {
        set
        {
            Title_UI.text = value;
        }
    }
    public Text Msg_UI;
    public string Msg
    {
        set
        {
            Msg_UI.text = value;
        }
    }
    public GameObject Bottom;
    public Button Return;
    public bool HasBtn
    {
        set
        {
            if(value)
            {
                Bottom.SetActive(true);
            }
            else
            {
                Bottom.SetActive(false);
            }
        }
    }
    public void Init(string _title, string _content, CallBack _back)
    {
        Return.onClick.RemoveAllListeners();
        Return.onClick.AddListener(Back);

        Title = _title;
        Msg = _content;
        BackEvent = _back;
        gameObject.SetActive(true);
    }
    public void Back()
    {
        if (BackEvent != null)
        {
            BackEvent();
        }
        gameObject.SetActive(false);
    }

}
