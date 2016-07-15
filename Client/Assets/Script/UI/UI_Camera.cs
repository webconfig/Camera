﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
public class UI_Camera : UI_Base
{
    public RawImage obj;
    WebCamTexture cameraTexture;
    string cameraName = "";
    private bool isPlay = false;
    private float PlayCD = 2;
    private float PlayStartTime=-3;
    public Button Btn_Capture, Btn_Look, Btn_Close, Btn_Set,Btn_JS;
    public Text Txt_WJ_Code, Txt_Goods, Txt_Send, Txt_Time,Txt_Total,Txt_JS_Time,Txt_Js_Btn;
    public Dropdown DL_Goods;
    public RawImage Img_Camera;
    /// <summary>
    /// 0:初始 1：开始计时 
    /// </summary>
    public int JsState = 0;
    public System.DateTime JsStartTime;
    public override void UI_Start()
    {
        Debug.Log(App.Instance.Data.Set.RunType);
        if (App.Instance.Data.Set.RunType == "0")
        {
            Btn_JS.gameObject.SetActive(false);
            Txt_JS_Time.gameObject.SetActive(false);
        }
        else
        {
            if(JsState==0)
            {
                Txt_Js_Btn.text = "开 始";
            }
            else
            {
                Txt_Js_Btn.text = "结 束";
            }

            Btn_JS.gameObject.SetActive(true);
            Txt_JS_Time.gameObject.SetActive(true);
        }

        StartCoroutine(WebCamRun());
        Btn_JS.onClick.AddListener(JS_Btn_Click);
        Btn_Capture.onClick.AddListener(Btn_Capture_Click);
        Btn_Look.onClick.AddListener(Btn_Look_Click);
        Btn_Close.onClick.AddListener(Btn_Close_Click);
        Btn_Set.onClick.AddListener(Btn_Set_Click);
        App.Instance.InputEvent += Btn_Capture_Click;
        App.Instance.Data.ValueChange += Data_ValueChange;
        App.Instance.Data.GoodsChangeEvent += Data_GoodsChangeEvent;
        Txt_Send.text = string.Format("已上传{1}车，总共{0}车", App.Instance.Data.Set.Total, App.Instance.Data.Set.Total - App.Instance.Data.LocalCount);
        Txt_WJ_Code.text = string.Format("挖机号：{0}", App.Instance.Data.Set.WJ_Code);
        Data_GoodsChangeEvent();
    }
    
    void JS_Btn_Click()
    {
        if (JsState == 0)
        {
            JsState = 1;
            Txt_JS_Time.text = "00:00:00";
            JsStartTime =System.DateTime.Now;
            Txt_Js_Btn.text = "结 束";
        }
        else
        {
            JsState = 0;
            Txt_Js_Btn.text = "开 始";
        }
    }
    void Data_GoodsChangeEvent()
    {
        DL_Goods.onValueChanged.RemoveAllListeners();
        DL_Goods.options.Clear();
        string goods_save = UnityEngine.PlayerPrefs.GetString("goods_set");
        int goods_value = 0;
        for (int i = 0; i < App.Instance.Data.Goods.Count; i++)
        {
            Dropdown.OptionData od = new Dropdown.OptionData();
            od.text = App.Instance.Data.Goods[i].GoodsName;
            if(od.text==goods_save)
            {
                goods_value = i;
            }
            DL_Goods.options.Add(od);
        }
        DL_Goods.onValueChanged.AddListener(GoodsSelect);
        DL_Goods.value = goods_value;
        DL_Goods.captionText.text = DL_Goods.options[goods_value].text;
    }
    void Data_ValueChange(int t)
    {
        Txt_Send.text = string.Format("已上传{1}车，总共{0}车", App.Instance.Data.Set.Total, t);
    }
    public override void UI_End()
    {
        Btn_Capture.onClick.RemoveAllListeners();
        Btn_Look.onClick.RemoveAllListeners();
        Btn_Close.onClick.RemoveAllListeners();
        Btn_Set.onClick.RemoveAllListeners();
        Btn_JS.onClick.RemoveAllListeners();
        App.Instance.Data.ValueChange -= Data_ValueChange;
        App.Instance.Data.GoodsChangeEvent -= Data_GoodsChangeEvent;
        App.Instance.InputEvent -= Btn_Capture_Click;
        if (isPlay)
        {
            isPlay = false;
            cameraTexture.Stop();
        }
    }
    public void Btn_Capture_Click()
    {
        if (isPlay)
        {
            if (Time.time - PlayStartTime < PlayCD) { return; }
            PlayStartTime = Time.time;
            if (string.IsNullOrEmpty(App.Instance.Data.Set.WJ_Code))
            {
                TipsManager.Instance.RunItem("请先设置基础信息");
                return;
            }
            App.Instance.NetWorkCanDo = false;
            isPlay = false;
            cameraTexture.Pause();
            StartCoroutine(getTexture2d());
        }
    }
    public void Btn_Look_Click()
    {
        UI_Manager.Instance.Show("UI_History");
    }
    public void Btn_Close_Click()
    {
        if (Img_Camera.texture != null)
        {
            Destroy(Img_Camera.texture);
        }
        UI_End();
        Application.Quit();
    }
    public void Btn_Set_Click()
    {
        UI_Manager.Instance.Show("UI_Set");
    }
    IEnumerator WebCamRun()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            cameraName = devices[0].name;
            cameraTexture = new WebCamTexture(cameraName, Screen.width, Screen.height, 15);
            obj.texture = cameraTexture;
            obj.material.mainTexture = cameraTexture;
            cameraTexture.Play();
            isPlay = true;
        }
    }
    IEnumerator getTexture2d()
    {
        yield return new WaitForEndOfFrame();
        if (Img_Camera.texture != null)
        {
            Destroy(Img_Camera.texture);
        }
        //新建一个model
        System.DateTime dt = System.DateTime.Now;
        WJ_Photo wj_photo = new WJ_Photo();
        wj_photo.CustomerID = App.Instance.Data.Set.CustomerID;
        wj_photo.WJID = App.Instance.Data.Set.WJ_Code;
        wj_photo.PhotoID = string.Format("{0}_{1}_{2}", App.Instance.Data.Set.CustomerID,
            App.Instance.Data.Set.WJ_Code, dt.ToString("yyMMddHHmmss"));
        wj_photo.PhotoPath = wj_photo.PhotoID + ".jpg";
        wj_photo.PhotoMiniPath = string.Format("{0}{1}{2}", "s_", wj_photo.PhotoID, ".jpg");
        wj_photo.AtTime = dt.ToString("yyyy-MM-dd HH:mm:ss");

        //图片翻转180度
        int width = cameraTexture.width;
        int height = cameraTexture.height;
        Texture2D snap = new Texture2D(width, height);
        Color[] cSource = cameraTexture.GetPixels();
        Color[] cRes = new Color[cSource.Length];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cRes[((width - x - 1) * height) + height - y - 1] = cSource[(x * height) + y];
            }
        }
        snap.SetPixels(cRes);
        snap.Apply();
        //===生成大图
        byte[] pngData = snap.EncodeToJPG(30);
        File.WriteAllBytes(App.Instance.Data.ImgPath + wj_photo.PhotoPath, pngData);
        //===生成缩略图
        TextureScale.Bilinear(snap, 100, 100);
        pngData = snap.EncodeToJPG(20);
        File.WriteAllBytes(App.Instance.Data.ImgPath + wj_photo.PhotoMiniPath, pngData);
        //===修改xml
        if (App.Instance.Data.Add(wj_photo, dt, DL_Goods.options[DL_Goods.value].text))
        {
            Txt_Total.text = App.Instance.Data.Set.Total.ToString();
            Txt_Total.gameObject.SetActive(true);
            Img_Camera.gameObject.SetActive(false);
            Destroy(Img_Camera.texture);
        }
        else
        {
            Txt_Total.gameObject.SetActive(false);
            Img_Camera.gameObject.SetActive(true);
            Img_Camera.texture = snap;
        }
        TipsManager.Instance.RunItem("添加成功！");
        cameraTexture.Play();
        isPlay = true;
        App.Instance.NetWorkCanDo = true;
    }
    private void GoodsSelect(int value)
    {
        PlayerPrefs.SetString("goods_set", DL_Goods.options[DL_Goods.value].text);
    }

    void Update()
    {
        if(JsState==1)
        {
            ts = System.DateTime.Now - JsStartTime;
            if (ts.Seconds >= 1)
            {
                Txt_JS_Time.text = string.Format("{0}:{1}:{2}", 
                    ts.Hours.ToString("00"), ts.Minutes.ToString("00"), ts.Seconds.ToString("00"));
            }
        }
    }

    private System.TimeSpan ts;
}

