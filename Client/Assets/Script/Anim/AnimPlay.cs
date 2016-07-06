using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
/// <summary>
/// 播放动画
/// </summary>
public class AnimPlay : MonoBehaviour
{
    public List<AnimItem> Items;
    public event CallBack Over;
    public bool Is_Over = false;


    private Vector3 OldPosition;
    private Color OldColor;
    public bool _init = false;

    public void Init()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].Init();
        }
    }
    public void Run()
    {
        if (!_init)
        {
            _init = true;
            Init();
        }
        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].Run();
        }
    }

    public void Update()
    {
        if (!Is_Over)
        {
            Is_Over = true;
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Update();
                if (Items[i].State != AnimState.Over)
                {
                    Is_Over = false;
                }
            }
            if (Is_Over)
            {
                if (Over != null)
                {
                    ReSet();
                    Over();
                    Over = null;
                }
            }
        }
    }

    public void ReSet()
    {
        Is_Over = false;
        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].Reset();
        }
    }
}

[System.Serializable]
public class AnimItem
{
    public float Delay;
    public float LifeTime;
    public AnimType Type;
    public AnimState State;
    public AnimationCurve cure;
    public GameObject obj;
    public string StartValue;
    public string EndValue;

    private Vector3 PositionStart, PositionEnd;
    private Vector4 ColorStart, ColorEnd;
    public Color color;

    public RectTransform rect;
    public Vector3 OldPosition;
    public Image img;
    public Color OldColor;

    public bool NeedReset = false;

    public void Init()
    {
        switch(Type)
        {
            case AnimType.Position:
                rect = obj.GetComponent<RectTransform>();
                string[] strs=StartValue.Split(',');
                PositionStart = new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
                strs = EndValue.Split(',');
                PositionEnd = new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
                break;
            case AnimType.Color:
                img = obj.GetComponent<Image>();
                string[] strs2=StartValue.Split(',');
                ColorStart = new Vector4(float.Parse(strs2[0]), float.Parse(strs2[1]), float.Parse(strs2[2]), float.Parse(strs2[3]));
                strs2 = EndValue.Split(',');
                ColorEnd = new Vector4(float.Parse(strs2[0]), float.Parse(strs2[1]), float.Parse(strs2[2]), float.Parse(strs2[3]));
                break;
        }
    }

    public void Run()
    {
        StartTime = Time.time;
        if (Delay <= 0)
        {
            State = AnimState.Run;
        }
        else
        {
            State = AnimState.Delay;
        }
    }

    public void Update()
    {
        if (State == AnimState.Over) { return; }
        if (State == AnimState.Delay)
        {
            if (Time.time - StartTime > Delay)
            {
                State = AnimState.Run;
                StartTime = Time.time;
            }
        }
        if (State == AnimState.Run)
        {
            RunTime = (Time.time - StartTime) / LifeTime;
            if (RunTime >= 1)
            {
                RunTime = 1;
                State = AnimState.Over;
            }
            switch (Type)
            {
                case AnimType.Position:
                    rect.localPosition = Vector3.Lerp(PositionStart, PositionEnd, cure.Evaluate(RunTime));
                    break;
                case AnimType.Color:
                    color.a=Mathf.Lerp(ColorStart.x, ColorEnd.x, cure.Evaluate(RunTime));
                    color.r = Mathf.Lerp(ColorStart.y, ColorEnd.y, cure.Evaluate(RunTime));
                    color.g = Mathf.Lerp(ColorStart.z, ColorEnd.z, cure.Evaluate(RunTime));
                    color.b = Mathf.Lerp(ColorStart.w, ColorEnd.w, cure.Evaluate(RunTime));
                    img.color = color;
                    break;
            }
        }
    }

    public void Reset()
    {
        State = AnimState.Init;
        if (NeedReset)
        {
            switch (Type)
            {
                case AnimType.Position:
                    rect.transform.position = OldPosition;
                    break;
                case AnimType.Color:
                    img.color = OldColor;
                    break;
            }
        }
    }

    public float StartTime, RunTime;
}

public enum AnimType
{
   Position,
   Color
}
public enum AnimState
{
    Init,
    Run,
    Delay,
    Over
}
