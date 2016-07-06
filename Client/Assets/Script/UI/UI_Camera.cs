using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private WJ_Set Data;
    public void Btn_Capture_Click()
    {
        if (Data==null)
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
        UI_End();
        Application.Quit();
    }
    public void Btn_Set_Click()
    {
        UI_Manager.Instance.Show("UI_Set");
    }

    void Update()
    {
        obj_Mat.mainTexture = cameraTexture;
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
    //void OnGUI()
    //{
    //    if (GUI.Button(new Rect(300, 500, 300, 300), "保存"))
    //    {
    //        StartCoroutine(getTexture2d());
    //    }
    //}
    IEnumerator getTexture2d()
    {
        yield return new WaitForEndOfFrame();
        Texture2D snap = new Texture2D(cameraTexture.width, cameraTexture.height);
        snap.SetPixels(cameraTexture.GetPixels());
        snap.Apply();
        byte[] pngData = snap.EncodeToJPG();
        WJ_Photo_Submit module = new WJ_Photo_Submit();
        module.dt = System.DateTime.Now;
        module.run_index = 0;
        //==========
        module.CustomerID = 1;// App.Instance.SetData.CustomerID;
        module.WJID = Data.WJ_Code;
        module.PhotoID = string.Format("{0}_{1}_{2}",
            App.Instance.SetData.CustomerID, Data.WJ_Code, System.DateTime.Now.ToString("yyMMddHHmmss"));
        module.PhotoPath = App.Instance.ImgPath + module.PhotoID + ".png";
        module.AtTime = module.dt.ToString("yyyy-MM-dd HH:mm:ss");
        File.WriteAllBytes(module.PhotoPath, pngData);
        Destroy(snap);

        ////====存数据库====
        //int k1 = App.Instance.data.connection.Insert(module);
        //if (k1 <= 0) { TipsManager.Instance.RunItem("出现错误[01]"); yield return 0; }
        //module.run_index = 1;

        //WJ_Photo wj_photo = new WJ_Photo();
        //wj_photo.CustomerID = module.CustomerID;
        //wj_photo.WJID = module.WJID;
        //wj_photo.PhotoID = module.PhotoID;
        //wj_photo.PhotoPath = module.PhotoPath;
        //wj_photo.AtTime = module.AtTime;
        //k1 = App.Instance.data.connection.Insert(wj_photo);
        //if (k1 <= 0) { TipsManager.Instance.RunItem("出现错误[02]"); yield return 0; }

        //string query = "select * from WJ_Record where EndPhotoID = '0'";
        //var userQuery = App.Instance.data.connection.Query<WJ_Record>(query);
        //if (userQuery != null && userQuery.Count > 0)
        //{
        //    query = string.Format("UPDATE WJ_Record SET EndTime = '{0}', EndPhotoID = '{1}' WHERE ID = {2}", module.AtTime, module.PhotoID, userQuery[0].ID);
        //    k1 = App.Instance.data.connection.Execute(query);
        //    if (k1 <= 0) { TipsManager.Instance.RunItem("出现错误[03]"); yield return 0; }

        //    query = string.Format("UPDATE WJ_Record_Submit SET EndTime = '{0}', EndPhotoID = '{1}' WHERE ID = {2}", module.AtTime, module.PhotoID, userQuery[0].ID);
        //    k1 = App.Instance.data.connection.Execute(query);
        //    if (k1 <= 0) { TipsManager.Instance.RunItem("出现错误[04]"); yield return 0; }
        //}
        //else
        //{


        //    query = string.Format("INSERT INTO WJ_Record ('CustomerID',)  SET EndTime = '{0}', EndPhotoID = '{1}' WHERE ID = {2}", module.AtTime, module.PhotoID, userQuery[0].ID);
        //    k1 = App.Instance.data.connection.Execute(query);
        //    if (k1 <= 0) { TipsManager.Instance.RunItem("出现错误[03]"); yield return 0; }



        //    WJ_Record wj_record = new WJ_Record();
        //    wj_record.CustomerID = module.CustomerID;
        //    wj_record.WJID = module.WJID;
        //    wj_record.ID = module.dt.Ticks;
        //    wj_record.WorkSpace = Data.Place;
        //    wj_record.GoodsName = DL_Goods.options[DL_Goods.value].text;
        //    wj_record.BeginTime = module.AtTime;
        //    wj_record.EndTime = "0";
        //    wj_record.BgeinPhotoID = module.PhotoID;
        //    wj_record.EndPhotoID = "0";
        //    wj_record.longitude = 0;
        //    wj_record.Latitude = 0;
        //    wj_record.Mode = 0;
        //    k1 = App.Instance.data.connection.Insert(wj_record);
        //    if (k1 <= 0) { TipsManager.Instance.RunItem("出现错误[05]"); yield return 0; }

        //    WJ_Record_Submit wj_record_submit = new WJ_Record_Submit();
        //    wj_record_submit.CustomerID = wj_record.CustomerID;
        //    wj_record_submit.WJID = wj_record.WJID;
        //    wj_record_submit.ID = wj_record.ID;
        //    wj_record_submit.WorkSpace = wj_record.WorkSpace;
        //    wj_record_submit.GoodsName = wj_record.GoodsName;
        //    wj_record_submit.BeginTime = wj_record.BeginTime;
        //    wj_record_submit.EndTime = wj_record.EndTime;
        //    wj_record_submit.BgeinPhotoID = wj_record.BgeinPhotoID;
        //    wj_record_submit.EndPhotoID = wj_record.EndPhotoID;
        //    wj_record_submit.longitude = wj_record.longitude;
        //    wj_record_submit.Latitude = wj_record.Latitude;
        //    wj_record_submit.Mode = wj_record.Mode;
        //    k1 = App.Instance.data.connection.Insert(wj_record_submit);
        //    if (k1 <= 0) { TipsManager.Instance.RunItem("出现错误[06]"); yield return 0; }
        //}

        TextureScale.Bilinear(snap, 100, 100);
        pngData = snap.EncodeToJPG(20);
        File.WriteAllBytes(App.Instance.ImgPath + module.PhotoID + "_s.png", pngData);
        AppData.XmlTest();

        TipsManager.Instance.RunItem("添加成功！");
        cameraTexture.Play();

    }

    
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

        Data = App.Instance.data.connection.Table<WJ_Set>().FirstOrDefault();
        if (Data != null)
        {
            Txt_WJ_Code.text = string.Format("挖机号：{0}", Data.WJ_Code);
        }
        DL_Goods.options.Clear();
        DL_Goods.onValueChanged.RemoveAllListeners();
        DL_Goods.onValueChanged.AddListener(GoodsSelect);

        List<WJ_Goods> Goods = App.Instance.data.connection.Table<WJ_Goods>().ToList();
        if (Goods != null)
        {
            for (int i = 0; i < Goods.Count; i++)
            {
                Dropdown.OptionData od = new Dropdown.OptionData();
                od.text = Goods[i].GoodsName;
                DL_Goods.options.Add(od);
            }
            int value = UnityEngine.PlayerPrefs.GetInt("WJ_Goods_ID");
            if (value >= 0 && value < DL_Goods.options.Count)
            {
                DL_Goods.value = value;
            }
        }
        AppData.XmlInit();
    }

    private void GoodsSelect(int value)
    {
        PlayerPrefs.SetInt("WJ_Goods_ID", value);
    }
    public override void UI_End()
    {
        if (isPlay)
        {
            cameraTexture.Stop();
        }
        Img_Camera.texture = null;
    }

    public List<WJ_Photo_Submit> ImgDatas = new List<WJ_Photo_Submit>();
}

