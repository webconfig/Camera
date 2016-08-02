using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class UI_Camera : UI_Base
{
    public GameObject Content;
    //WebCamTexture cameraTexture;
    //string cameraName = "";
    private bool isPlay = true;
    private float PlayStartTime=-10000;
    public Button Btn_Capture, Btn_Look, Btn_Close, Btn_Set,Btn_JS;
    public Text Txt_WJ_Code, Txt_Goods, Txt_Send, Txt_Time,Txt_Total,Txt_JS_Time,Txt_Js_Btn,Txt_NetWork,Txt_RunStrs,Txt_NowTime;
    public Dropdown DL_Goods;
    public RawImage Img_Camera;
    private Texture2D snap;
    private Color[] cRes;
    private bool Js_Begin = false;
    private WJ_Record_Local last_record_js;

    public override void UI_Init()
    {
        Btn_JS.onClick.AddListener(Btn_Capture_Click2);
        Btn_Capture.onClick.AddListener(Btn_Capture_Click);
        Btn_Look.onClick.AddListener(Btn_Look_Click);
        Btn_Close.onClick.AddListener(Btn_Close_Click);
        Btn_Set.onClick.AddListener(Btn_Set_Click);
        Txt_Send.text = string.Format("已上传<color=#00ff00ff>{1}</color>车，总共<color=#00ff00ff>{0}</color>车", App.Instance.Data.Set.Total, App.Instance.Data.Set.Total - App.Instance.Data.LocalCount);
        Data_GoodsChangeEvent();
        width = Screen.width;
        height = Screen.height;
        transform.localRotation
    }
    public override void UI_Start()
    {
        //StartCoroutine(WebCamRun());
        Txt_WJ_Code.text = string.Format("挖机号：<color=#00ff00ff>{0}</color>", App.Instance.Data.Set.WJ_Code);
        App.Instance.InputEvent += Btn_Capture_Click;
        App.Instance.Data.ValueChange += Data_ValueChange;
        App.Instance.Data.GoodsChangeEvent += Data_GoodsChangeEvent;
        App.Instance.DataServer.RunStrsChangeEvent += DataServer_RunStrsChangeEvent;
        WJ_Record_Local wr = null;
        if (App.Instance.Data.CurrentData.Records.Count > 0)
        {
            wr = App.Instance.Data.CurrentData.Records.Values.Last();
        }
        time_old = System.DateTime.Now;
        if (wr != null && !string.IsNullOrEmpty(wr.Data.BgeinPhotoID) && string.IsNullOrEmpty(wr.Data.EndPhotoID))
        {//最后一次为结束
            Txt_Total.gameObject.SetActive(false);
            Img_Camera.gameObject.SetActive(true);

            if (Img_Camera.texture == null)
            {//加载最后一次的照片
                string path = App.Instance.Data.ImgMinPath + App.Instance.Data.CurrentData.Photos[wr.Data.BgeinPhotoID].PhotoMiniPath;
                FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
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
                snap = new Texture2D(116, 116);
                snap.LoadImage(bytes);
                Img_Camera.texture = snap;
            }
        }
        else
        {
            Txt_Total.text = App.Instance.Data.Set.Total.ToString();
            Txt_Total.gameObject.SetActive(true);
            Img_Camera.gameObject.SetActive(false);
            Destroy(Img_Camera.texture);
        }
        if (App.Instance.Data.Set.RunType == 0)
        {//计次
            Btn_JS.gameObject.SetActive(false);
            Txt_JS_Time.gameObject.SetActive(false);
        }
        else
        {//计时
            if (App.Instance.Data.CurrentData.Records_JS.Count > 0)
            {
                last_record_js = App.Instance.Data.CurrentData.Records_JS.Values.Last();
                Js_Begin = string.IsNullOrEmpty(last_record_js.Data.EndPhotoID);
                last_record_js.BeginTime_T = System.Convert.ToDateTime(last_record_js.Data.BeginTime);
                last_record_js.StartTime_T = last_record_js.BeginTime_T;
                last_record_js.time = Time.time;
                if (Js_Begin)
                {
                    Txt_Js_Btn.text = "结 束";
                }
                else
                {
                    Txt_Js_Btn.text = "开 始";
                }
            }
            else
            {
                Txt_Js_Btn.text = "开 始";
            }

            Btn_JS.gameObject.SetActive(true);
            Txt_JS_Time.gameObject.SetActive(true);
        }
    }
    public override void UI_End()
    {
        Img_Camera.gameObject.SetActive(false);
        App.Instance.InputEvent -= Btn_Capture_Click;
        App.Instance.Data.ValueChange -= Data_ValueChange;
        App.Instance.Data.GoodsChangeEvent -= Data_GoodsChangeEvent;
        App.Instance.DataServer.RunStrsChangeEvent -= DataServer_RunStrsChangeEvent;
        //cameraTexture.Stop();
    }
    
    #region 抓拍
    private void Btn_Capture_Click()
    {
        if (App.Instance.Data.Set.JCType == 1)
        {
            jc_one();
        }
        else
        {
            jc_two();
        }
    }
    public void jc_one()
    {
        if (isPlay)
        {
            if (string.IsNullOrEmpty(App.Instance.Data.Set.WJ_Code))
            {
                TipsManager.Instance.Error("请先设置基础信息");
                return;
            }
            if (Time.time - PlayStartTime < App.Instance.Data.Set.CD1)
            {
                float k = App.Instance.Data.Set.CD1 - Time.time + PlayStartTime;
                TipsManager.Instance.Error(string.Format("{0}秒后才能再次抓拍", k.ToString("0.0")));
                return;
            }
            App.Instance.DisAbleNewWork();
            isPlay = false;
            Content.gameObject.SetActive(false);
            StartCoroutine(jc_one_Action());
        }
    }
    IEnumerator jc_one_Action()
    {
        yield return new WaitForEndOfFrame();
        try
        {
            //新建一个model
            System.DateTime dt = System.DateTime.Now;
            WJ_Photo_Local wj_photo = new WJ_Photo_Local();
            wj_photo.Data = new google.protobuf.WJ_Photo();
            wj_photo.SeqID = dt.Ticks.ToString();
            wj_photo.Data.CustomerID = App.Instance.Data.Set.CustomerID;
            wj_photo.Data.WJID = App.Instance.Data.Set.WJ_Code;
            wj_photo.Data.PhotoID = string.Format("{0}_{1}_{2}", App.Instance.Data.Set.CustomerID, App.Instance.Data.Set.WJ_Code, dt.ToString("yyMMddHHmmss"));
            wj_photo.Data.PhotoPath = wj_photo.Data.PhotoID + ".jpg";
            wj_photo.PhotoMiniPath = wj_photo.Data.PhotoPath;
            wj_photo.Data.AtTime = dt.ToString("yyyy-MM-dd HH:mm:ss");
            #region 保存图片
            //图片翻转180度
            snap = GetTexture();// new Texture2D(width, height);
            //Color[] cSource = GetTexture().GetPixels();// cameraTexture.GetPixels();
            //cRes = new Color[cSource.Length];
            //for (int x = 0; x < width; x++)
            //{
            //    for (int y = 0; y < height; y++)
            //    {
            //        cRes[((width - x - 1) * height) + height - y - 1] = cSource[(x * height) + y];
            //    }
            //}
            //snap.SetPixels(cRes);
            //snap.Apply(false);
            //===生成大图
            byte[] pngData = snap.EncodeToJPG(50);
            File.WriteAllBytes(App.Instance.Data.ImgPath + wj_photo.Data.PhotoPath, pngData);
            //===生成缩略图
            TextureScale.Bilinear(snap, 100, 100);
            pngData = snap.EncodeToJPG(40);
            File.WriteAllBytes(App.Instance.Data.ImgMinPath + wj_photo.PhotoMiniPath, pngData);
            #endregion
            //===修改xml
            App.Instance.Data.Add(wj_photo, dt, DL_Goods.options[DL_Goods.value].text);
            PlayStartTime = Time.time;
            Txt_Total.text = App.Instance.Data.Set.Total.ToString();
            Txt_Total.gameObject.SetActive(true);
            Img_Camera.gameObject.SetActive(false);
            Destroy(Img_Camera.texture);

            TipsManager.Instance.Info("添加成功！");
        }
        catch
        {
            App.Instance.DataServer.AddStr("添加计次错误");
        }
        Content.gameObject.SetActive(true);
        isPlay = true;
        App.Instance.EnAbleNetWork();
    }
    public void jc_two()
    {
        if (isPlay)
        {
            if (string.IsNullOrEmpty(App.Instance.Data.Set.WJ_Code))
            {
                TipsManager.Instance.Error("请先设置基础信息");
                return;
            }
            if (Time.time - PlayStartTime < App.Instance.Data.Set.CD)
            {
                float k = App.Instance.Data.Set.CD - Time.time + PlayStartTime;
                TipsManager.Instance.Error(string.Format("{0}秒后才能再次抓拍", k.ToString("0.0")));
                return;
            }
            App.Instance.DisAbleNewWork();
            isPlay = false;
            Content.gameObject.SetActive(false);
            StartCoroutine(jc_two_Action());
        }
    }
    IEnumerator jc_two_Action()
    {
        yield return new WaitForEndOfFrame();
        try
        {
            //新建一个model
            System.DateTime dt = System.DateTime.Now;
            WJ_Photo_Local wj_photo = new WJ_Photo_Local();
            wj_photo.Data = new google.protobuf.WJ_Photo();
            wj_photo.SeqID = dt.Ticks.ToString();
            wj_photo.Data.CustomerID = App.Instance.Data.Set.CustomerID;
            wj_photo.Data.WJID = App.Instance.Data.Set.WJ_Code;
            wj_photo.Data.PhotoID = string.Format("{0}_{1}_{2}", App.Instance.Data.Set.CustomerID, App.Instance.Data.Set.WJ_Code, dt.ToString("yyMMddHHmmss"));
            wj_photo.Data.PhotoPath = wj_photo.Data.PhotoID + ".jpg";
            wj_photo.PhotoMiniPath = wj_photo.Data.PhotoPath;
            wj_photo.Data.AtTime = dt.ToString("yyyy-MM-dd HH:mm:ss");
            #region 保存图片
            //图片翻转180度
            snap = GetTexture();// new Texture2D(width, height);
            //Color[] cSource = GetTexture().GetPixels();// cameraTexture.GetPixels();
            //cRes = new Color[cSource.Length];
            //for (int x = 0; x < width; x++)
            //{
            //    for (int y = 0; y < height; y++)
            //    {
            //        cRes[((width - x - 1) * height) + height - y - 1] = cSource[(x * height) + y];
            //    }
            //}
            //snap.SetPixels(cRes);
            //snap.Apply(false);
            //===生成大图
            byte[] pngData = snap.EncodeToJPG(50);
            File.WriteAllBytes(App.Instance.Data.ImgPath + wj_photo.Data.PhotoPath, pngData);
            //===生成缩略图
            TextureScale.Bilinear(snap, 100, 100);
            pngData = snap.EncodeToJPG(40);
            File.WriteAllBytes(App.Instance.Data.ImgMinPath + wj_photo.PhotoMiniPath, pngData);
            #endregion
            //===修改xml
            if (App.Instance.Data.Add(wj_photo, dt, DL_Goods.options[DL_Goods.value].text))
            {//完成一车
                PlayStartTime = Time.time + 1.1f - App.Instance.Data.Set.CD;//App.Instance.Data.Set.CD - Time.time + PlayStartTime=1
                Txt_Total.text = App.Instance.Data.Set.Total.ToString();
                Txt_Total.gameObject.SetActive(true);
                Img_Camera.gameObject.SetActive(false);
                Destroy(Img_Camera.texture);
            }
            else
            {//第一次抓拍
                PlayStartTime = Time.time;
                Txt_Total.gameObject.SetActive(false);
                Img_Camera.gameObject.SetActive(true);
                Img_Camera.texture = snap;
            }
            TipsManager.Instance.Info("添加成功！");
        }
        catch
        {
            App.Instance.DataServer.AddStr("添加两次计次错误");
        }
        Content.gameObject.SetActive(true);
        isPlay = true;
        App.Instance.EnAbleNetWork();
    }
    public void Btn_Capture_Click2()
    {
        if (isPlay)
        {
            if (string.IsNullOrEmpty(App.Instance.Data.Set.WJ_Code))
            {
                TipsManager.Instance.Error("请先设置基础信息");
                return;
            }
            App.Instance.DisAbleNewWork();
            Content.gameObject.SetActive(false);
            StartCoroutine(Btn_Capture_Click2_Action());
        }
    }
    IEnumerator Btn_Capture_Click2_Action()
    {
        yield return new WaitForEndOfFrame();
        try
        {
            isPlay = false;
            //新建一个model
            System.DateTime dt = System.DateTime.Now;
            WJ_Photo_Local wj_photo = new WJ_Photo_Local();
            wj_photo.Data = new google.protobuf.WJ_Photo();
            wj_photo.SeqID = dt.Ticks.ToString();
            wj_photo.Data.CustomerID = App.Instance.Data.Set.CustomerID;
            wj_photo.Data.WJID = App.Instance.Data.Set.WJ_Code;
            wj_photo.Data.PhotoID = string.Format("{0}_{1}_{2}", App.Instance.Data.Set.CustomerID, App.Instance.Data.Set.WJ_Code, dt.ToString("yyMMddHHmmss"));
            wj_photo.Data.PhotoPath = wj_photo.Data.PhotoID + ".jpg";
            wj_photo.PhotoMiniPath = wj_photo.Data.PhotoPath;
            wj_photo.Data.AtTime = dt.ToString("yyyy-MM-dd HH:mm:ss");
            #region 生成图片
            //图片翻转180度
            snap = GetTexture();// new Texture2D(width, height);
            //Color[] cSource = GetTexture().GetPixels(); //cameraTexture.GetPixels();
            //for (int x = 0; x < width; x++)
            //{
            //    for (int y = 0; y < height; y++)
            //    {
            //        cRes[((width - x - 1) * height) + height - y - 1] = cSource[(x * height) + y];
            //    }
            //}
            //snap.SetPixels(cRes);
            //snap.Apply(false);
            //===生成大图
            byte[] pngData = snap.EncodeToJPG(50);
            File.WriteAllBytes(App.Instance.Data.ImgPath + wj_photo.Data.PhotoPath, pngData);
            //===生成缩略图
            TextureScale.Bilinear(snap, 100, 100);
            pngData = snap.EncodeToJPG(40);
            File.WriteAllBytes(App.Instance.Data.ImgMinPath + wj_photo.PhotoMiniPath, pngData);
            #endregion
            //===修改xml
            if (App.Instance.Data.Add_JS(wj_photo, dt, DL_Goods.options[DL_Goods.value].text))
            {
                Txt_JS_Time.text = "00:00:00";
                Txt_Js_Btn.text = "结 束";
                TipsManager.Instance.Info("开始计时");
                Js_Begin = true;
                last_record_js = App.Instance.Data.CurrentData.Records_JS.Values.Last();
            }
            else
            {
                Txt_Js_Btn.text = "开 始";
                TipsManager.Instance.Info("结束计时");
                Js_Begin = false;
                last_record_js = null;

            }
        }
        catch
        {
            App.Instance.DataServer.AddStr("添加计时记录错误");
        }
        Content.gameObject.SetActive(true);
        isPlay = true;
        App.Instance.EnAbleNetWork();
    }
    #endregion
    #region 页面跳转
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
    #endregion

    public Texture2D GetTexture()
    {
        Texture2D photo = new Texture2D(width, height, TextureFormat.RGB24, false);
        photo.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
        photo.Apply();
        return photo;
    }
    //IEnumerator WebCamRun()
    //{
    //    yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
    //    if (Application.HasUserAuthorization(UserAuthorization.WebCam))
    //    {
    //        WebCamDevice[] devices = WebCamTexture.devices;
    //        cameraName = devices[0].name;
    //        cameraTexture = new WebCamTexture(cameraName, Screen.width, Screen.height);
    //        obj.texture = cameraTexture;
    //        obj.material.mainTexture = cameraTexture;
    //        cameraTexture.Play();
    //        width = cameraTexture.width;
    //        height = cameraTexture.height;
    //        cRes = cameraTexture.GetPixels();
    //        isPlay = true;
    //    }
    //    else
    //    {
    //        TipsManager.Instance.Error("没有访问摄像头的权限");
    //    }
    //}
    private int width,height;

    #region 页面内容
    void DataServer_RunStrsChangeEvent(string t)
    {
        Txt_RunStrs.text = t;
    }
    void DataServer_RttChangeEvent(string t)
    {
        Txt_NetWork.text = t;
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
            if (od.text == goods_save)
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
        Txt_Send.text = string.Format("已上传<color=#00ff00ff>{1}</color>车，总共<color=#00ff00ff>{0}</color>车", App.Instance.Data.Set.Total, App.Instance.Data.Set.Total - App.Instance.Data.LocalCount);
    }
    private void GoodsSelect(int value)
    {
        PlayerPrefs.SetString("goods_set", DL_Goods.options[DL_Goods.value].text);
    }
    #endregion
    void Update()
    {
        if (App.Instance.DataServer.State == ClientStat.ConnFail)
        {
            if (Time.time - ConnFailStartTime >= 1)
            {
                ConnFailStartTime = Time.time;
                float t = App.Instance.DataServer.ConnCD - Time.time + App.Instance.DataServer.ConnStartTime;
                if (t < 0) { t = 0; }
                Txt_NetWork.text = string.Format("网络：<color=#FF0000FF>断线</color>,{0}秒后重新连接", t.ToString("0.0"));
            }
        }
        if (App.Instance.Data.Set.RunType == 1)
        {//计时 
            if (Js_Begin)
            {
                ts = System.DateTime.Now - last_record_js.BeginTime_T;
                if (ts.TotalSeconds >= App.Instance.Data.Set.JSCD+ last_record_js.AddTime)
                {//自动结束
                    Btn_Capture_Click2();
                }
                else
                {
                    if (Time.time- last_record_js.time>=1)
                    {
                        last_record_js.time = Time.time;
                        Txt_JS_Time.text = string.Format("{0}:{1}:{2}",
                            ts.Hours.ToString("00"), ts.Minutes.ToString("00"), ts.Seconds.ToString("00"));
                    }
                    ts = System.DateTime.Now - last_record_js.StartTime_T;
                    if(ts.TotalMinutes>=1)
                    {//每隔5分钟保存一次
                       last_record_js.StartTime_T = System.DateTime.Now;
                       App.Instance.Data.SaveJs();
                    }
                }
            }
        }
        //=======日期=======
        ts2 = System.DateTime.Now - time_old;
        if(ts2.TotalSeconds>=1)
        {
            Txt_NowTime.text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

    }

    private float ConnFailStartTime = 0;
    private System.TimeSpan ts,ts2;
    private System.DateTime time_old;
}

