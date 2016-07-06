using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TipsItem : MonoBehaviour
{
    public float LifeTime;
    public Text txt;
    public AnimPlay Anim;
    public void Run(string msg)
    {
        txt.text = msg;
        gameObject.SetActive(true);
        Anim.Over += Anim_Over;
        Anim.Run();
    }

    void Anim_Over()
    {
        gameObject.SetActive(false);
    }

}