using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TipsItem : MonoBehaviour
{
    public float LifeTime=3,RunTime,StartTime;
    public Text txt;
    public Image img;
    public AnimPlay Anim;
    public bool _run = false;
    public Color start_color;
    private Vector3 _run_position;

    public void Run(string msg,Color color)
    {
        start_color = color;
        img.color = start_color;
        txt.text = msg;
        _run_position = Vector3.zero;
        StartTime = Time.time;
        if (_run)
        {
            transform.localPosition = _run_position;
        }
        _run = true;
    }

    void Update()
    {
        if(_run)
        {
            RunTime = (Time.time - StartTime) / LifeTime;
            RunTime=TipsManager.Instance.AnimCure.Evaluate(RunTime);
            _run_position.y = Mathf.Lerp(0, 500, RunTime);
            start_color.a = Mathf.Lerp(1, 0, RunTime);
            transform.localPosition = _run_position;
            img.color = start_color;
            if(RunTime>=1)
            {
                _run = false;
            }
        }
    }
}