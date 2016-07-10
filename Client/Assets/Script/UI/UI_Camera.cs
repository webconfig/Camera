using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 登录
/// </summary>
public class UI_Camera : UI_Base
{
    public RawImage obj;
    WebCamTexture cameraTexture;
    string cameraName = "";
    private bool isPlay = false;

    public Button Btn_Capture, Btn_Look, Btn_Close, Btn_Set;
    public Text Txt_WJ_Code, Txt_Goods, Txt_Send, Txt_Time,Txt_Total;
    public Dropdown DL_Goods;
    public RawImage Img_Camera;
    public override void UI_Start()
    {
        //obj.SetActive(true);
        //obj_Mat = obj.GetComponent<Image>().material;
        StartCoroutine(WebCamRun());

        Btn_Capture.onClick.AddListener(Btn_Capture_Click);
        Btn_Look.onClick.AddListener(Btn_Look_Click);
        Btn_Close.onClick.AddListener(Btn_Close_Click);
        Btn_Set.onClick.AddListener(Btn_Set_Click);
        App.Instance.Data.ValueChange += Data_ValueChange;

        Txt_Send.text = string.Format("已上传{1}车，总共{0}车", App.Instance.Data.Total, App.Instance.Data.Total-App.Instance.Data.LocalCount);
        Txt_WJ_Code.text = string.Format("挖机号：{0}", App.Instance.Data.Set.WJ_Code);

        DL_Goods.options.Clear();
        DL_Goods.onValueChanged.RemoveAllListeners();
        DL_Goods.onValueChanged.AddListener(GoodsSelect);

        for (int i = 0; i <App.Instance.Data.Goods.Count; i++)
        {
            Dropdown.OptionData od = new Dropdown.OptionData();
            od.text = App.Instance.Data.Goods[i].GoodsName;
            DL_Goods.options.Add(od);
        }
        int value = UnityEngine.PlayerPrefs.GetInt("WJ_Goods_ID");
        if (value >= 0 && value < DL_Goods.options.Count)
        {
            DL_Goods.value = value;
        }
        else
        {
            DL_Goods.value = 0;
        }
    }

    void Data_ValueChange(int t)
    {
        Txt_Send.text = string.Format("已上传{1}车，总共{0}车", App.Instance.Data.Total, t);
    }
    public override void UI_End()
    {
        Btn_Capture.onClick.RemoveAllListeners();
        Btn_Look.onClick.RemoveAllListeners();
        Btn_Close.onClick.RemoveAllListeners();
        Btn_Set.onClick.RemoveAllListeners();
        App.Instance.Data.ValueChange -= Data_ValueChange;

        if (isPlay)
        {
            isPlay = false;
            cameraTexture.Stop();
        }
        //obj.SetActive(false);
    }

    public void Btn_Capture_Click()
    {
        if (isPlay)
        {
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
        Texture2D snap = new Texture2D(cameraTexture.width, cameraTexture.height);
        //snap.SetPixels(cameraTexture.GetPixels());
        Color32[] cSource = cameraTexture.GetPixels32();
        Color32[] cRes = new Color32[cSource.Length];
        int width = cameraTexture.width;
        int height = cameraTexture.height;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cRes[((width - x - 1) * height) + height - y - 1] = cSource[(x * height) + y];
            }
        }
        snap.SetPixels32(cRes);
        snap.Apply();

        byte[] pngData = snap.EncodeToJPG(30);
        System.DateTime dt = System.DateTime.Now;
        WJ_Photo wj_photo = new WJ_Photo();
        wj_photo.CustomerID = App.Instance.Data.Set.CustomerID;
        wj_photo.WJID = App.Instance.Data.Set.WJ_Code;
        wj_photo.PhotoID = string.Format("{0}_{1}_{2}", App.Instance.Data.Set.CustomerID, App.Instance.Data.Set.WJ_Code, System.DateTime.Now.ToString("yyMMddHHmmss"));
        wj_photo.PhotoPath = wj_photo.PhotoID + ".jpg";
        wj_photo.PhotoMiniPath =string.Format("{0}{1}{2}","s_",wj_photo.PhotoID,".jpg");
        wj_photo.AtTime = dt.ToString("yyyy-MM-dd HH:mm:ss");

        File.WriteAllBytes(App.Instance.Data.ImgPath + wj_photo.PhotoPath, pngData);
        TextureScale.Bilinear(snap, 100, 100);
        pngData = snap.EncodeToJPG(20);
        File.WriteAllBytes(App.Instance.Data.ImgPath + wj_photo.PhotoMiniPath, pngData);
        if (App.Instance.Data.Add(wj_photo, dt, DL_Goods.options[DL_Goods.value].text))
        {
            Txt_Total.text = App.Instance.Data.Total.ToString();
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
        App.Instance.Data.UpLoadFiles.Add(wj_photo.PhotoPath);
        App.Instance.NetWorkCanDo = true;
    }

    private void GoodsSelect(int value)
    {
        PlayerPrefs.SetInt("WJ_Goods_ID", value);
    }
    public static Texture2D Rotate180(Texture2D source)
    {
        Texture2D result = new Texture2D(source.height, source.width);
        Color32[] cSource = source.GetPixels32();
        Color32[] cRes = new Color32[cSource.Length];

        for (int x = 0; x < source.width; x++)
        {
            for (int y = 0; y < source.height; y++)
            {
                cRes[((source.width - x - 1) * source.height) + source.height - y - 1] = cSource[(x * source.height) + y];
            }
        }

        result.SetPixels32(cRes);
        result.Apply();
        return result;

    }
}

