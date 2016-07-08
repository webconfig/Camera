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
    public GameObject obj;
    public Material obj_Mat;
    WebCamTexture cameraTexture;
    string cameraName = "";
    private bool isPlay = false;

    public Button Btn_Capture, Btn_Look, Btn_Close, Btn_Set;
    public Text Txt_WJ_Code, Txt_Goods, Txt_Send, Txt_Time;
    public Dropdown DL_Goods;
    public RawImage Img_Camera;
    public override void UI_Start()
    {
        obj_Mat = obj.GetComponent<Renderer>().material;
        StartCoroutine(WebCamRun());

        Btn_Capture.onClick.RemoveAllListeners();
        Btn_Capture.onClick.AddListener(Btn_Capture_Click);
        Btn_Look.onClick.RemoveAllListeners();
        Btn_Look.onClick.AddListener(Btn_Look_Click);
        Btn_Close.onClick.RemoveAllListeners();
        Btn_Close.onClick.AddListener(Btn_Close_Click);
        Btn_Set.onClick.RemoveAllListeners();
        Btn_Set.onClick.AddListener(Btn_Set_Click);

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
    }
    public override void UI_End()
    {
        if (isPlay)
        {
            cameraTexture.Stop();
        }
    }

    public void Btn_Capture_Click()
    {
        if (string.IsNullOrEmpty(App.Instance.Data.Set.WJ_Code))
        {
            TipsManager.Instance.RunItem("请先设置基础信息");
            return;
        }
        cameraTexture.Pause();
        StartCoroutine(getTexture2d());
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
            obj_Mat.mainTexture = cameraTexture;
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
        snap.SetPixels(cameraTexture.GetPixels());
        snap.Apply();
        byte[] pngData = snap.EncodeToJPG();
        System.DateTime dt = System.DateTime.Now;
        WJ_Photo wj_photo = new WJ_Photo();
        wj_photo.CustomerID = App.Instance.Data.Set.CustomerID;
        wj_photo.WJID = App.Instance.Data.Set.WJ_Code;
        wj_photo.PhotoID = string.Format("{0}_{1}_{2}", App.Instance.Data.Set.CustomerID, App.Instance.Data.Set.WJ_Code, System.DateTime.Now.ToString("yyMMddHHmmss"));
        wj_photo.PhotoPath = App.Instance.ImgPath + wj_photo.PhotoID + ".png";
        wj_photo.PhotoMiniPath = App.Instance.ImgPath + wj_photo.PhotoID + "_s.png";
        wj_photo.AtTime = dt.ToString("yyyy-MM-dd HH:mm:ss");
        File.WriteAllBytes(wj_photo.PhotoPath, pngData);
        TextureScale.Bilinear(snap, 100, 100);
        pngData = snap.EncodeToJPG(20);
        File.WriteAllBytes(wj_photo.PhotoMiniPath, pngData);

        ////====存数据====
        if (string.IsNullOrEmpty(App.Instance.Data.Records[App.Instance.Data.Records.Count].EndPhotoID))
        {
            WJ_Record wj_record = App.Instance.Data.Records.Last();
            wj_record.EndPhotoID = wj_photo.PhotoID;
            wj_record.EndTime = wj_photo.AtTime;
            XmlNode node = App.Instance.Data.data_record_parent.LastChild;
            node.Attributes["EndPhotoID"].Value = wj_record.EndPhotoID;
            node.Attributes["EndTime"].Value = wj_record.EndTime;
            //===保存到xml
            App.Instance.Data.Data_Xml.Save(App.Instance.Data.NowDataXmlPath);

            node = App.Instance.Data.submit_record_parent.LastChild;
            node.Attributes["EndPhotoID"].Value = wj_record.EndPhotoID;
            node.Attributes["EndTime"].Value = wj_record.EndTime;
            //===保存到xml
            if (App.Instance.Data.submit_record_parent.ChildNodes.Count >= 20)
            {//一个xml只记录20条记录
                App.Instance.Data.SubmitDatas.Add(App.Instance.Data.Data_Submit_Xml.ToString());
                App.Instance.Data.Data_Submit_Xml.Save(App.Instance.Data.FilePath_Submit + System.DateTime.Now.Ticks.ToString() + ".xml");
                App.Instance.Data.Data_Submit_Xml.LoadXml("<root><photo></photo><record></record></root>");
                App.Instance.Data.submit_photo_parent = App.Instance.Data.Data_Submit_Xml.SelectSingleNode("root/photo");
                App.Instance.Data.submit_record_parent = App.Instance.Data.Data_Submit_Xml.SelectSingleNode("root/record");
                App.Instance.Data.Data_Submit_Xml.Save(App.Instance.Data.FilePath_Submit + "new.xml");
            }
            else
            {
                App.Instance.Data.Data_Submit_Xml.Save(App.Instance.Data.NowSubmitXml);
            }
        }
        else
        {
            WJ_Record wj_record = new WJ_Record();
            wj_record.CustomerID = wj_photo.CustomerID;
            wj_record.WJID = wj_photo.WJID;
            wj_record.ID = dt.Ticks.ToString();
            wj_record.WorkSpace = App.Instance.Data.Set.Place;
            wj_record.GoodsName = DL_Goods.options[DL_Goods.value].text;
            wj_record.BeginTime = wj_photo.AtTime;
            wj_record.EndTime = "";
            wj_record.BgeinPhotoID = wj_photo.PhotoID;
            wj_record.EndPhotoID = "";
            wj_record.longitude = "";
            wj_record.Latitude = "";
            wj_record.Mode = "0";

            //======保存Data======
            App.Instance.Data.Photos.Add(wj_photo.PhotoID, wj_photo);
            App.Instance.Data.Records.Add(wj_record);

            XmlElement node = App.Instance.Data.Data_Xml.CreateElement("item");
            node.SetAttribute("CustomerID", wj_photo.CustomerID);
            node.SetAttribute("WJID", wj_photo.WJID);
            node.SetAttribute("PhotoID", wj_photo.PhotoID);
            node.SetAttribute("PhotoPath", wj_photo.PhotoPath);
            node.SetAttribute("AtTime", wj_photo.AtTime);
            App.Instance.Data.data_photo_parent.AppendChild(node);

            node = App.Instance.Data.Data_Xml.CreateElement("item");
            node.SetAttribute("CustomerID", wj_record.CustomerID);
            node.SetAttribute("WJID", wj_record.WJID);
            node.SetAttribute("ID", wj_record.ID);
            node.SetAttribute("WorkSpace", wj_record.WorkSpace);
            node.SetAttribute("GoodsName", wj_record.GoodsName);
            node.SetAttribute("BeginTime", wj_record.BeginTime);
            node.SetAttribute("EndTime", wj_record.EndTime);
            node.SetAttribute("BgeinPhotoID", wj_record.BgeinPhotoID);
            node.SetAttribute("EndPhotoID", wj_record.EndPhotoID);
            node.SetAttribute("longitude", wj_record.longitude);
            node.SetAttribute("Latitude", wj_record.Latitude);
            node.SetAttribute("Mode", wj_record.Mode);
            App.Instance.Data.data_record_parent.AppendChild(node);
            //===保存到xml
            App.Instance.Data.Data_Xml.Save(App.Instance.Data.NowDataXmlPath);

            //======保存Submit======
            node = App.Instance.Data.Data_Submit_Xml.CreateElement("item");
            node.SetAttribute("CustomerID", wj_photo.CustomerID);
            node.SetAttribute("WJID", wj_photo.WJID);
            node.SetAttribute("PhotoID", wj_photo.PhotoID);
            node.SetAttribute("PhotoPath", wj_photo.PhotoPath);
            node.SetAttribute("AtTime", wj_photo.AtTime);
            App.Instance.Data.submit_photo_parent.AppendChild(node);

            node = App.Instance.Data.Data_Submit_Xml.CreateElement("item");
            node.SetAttribute("SeqID", dt.Ticks.ToString());
            node.SetAttribute("CustomerID", wj_record.CustomerID);
            node.SetAttribute("WJID", wj_record.WJID);
            node.SetAttribute("ID", wj_record.ID);
            node.SetAttribute("WorkSpace", wj_record.WorkSpace);
            node.SetAttribute("GoodsName", wj_record.GoodsName);
            node.SetAttribute("BeginTime", wj_record.BeginTime);
            node.SetAttribute("EndTime", wj_record.EndTime);
            node.SetAttribute("BgeinPhotoID", wj_record.BgeinPhotoID);
            node.SetAttribute("EndPhotoID", wj_record.EndPhotoID);
            node.SetAttribute("longitude", wj_record.longitude);
            node.SetAttribute("Latitude", wj_record.Latitude);
            node.SetAttribute("Mode", wj_record.Mode);
            App.Instance.Data.submit_record_parent.AppendChild(node);
            //===保存到xml
            App.Instance.Data.Data_Submit_Xml.Save(App.Instance.Data.NowSubmitXml);
        }
        TipsManager.Instance.RunItem("添加成功！");
        cameraTexture.Play();
        Img_Camera.texture = snap;
    }

    private void GoodsSelect(int value)
    {
        PlayerPrefs.SetInt("WJ_Goods_ID", value);
    }
}

