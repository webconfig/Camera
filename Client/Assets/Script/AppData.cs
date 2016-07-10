using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Xml;
using UnityEngine;
using google.protobuf;
[System.Serializable]
public class AppData
{
    public void Init()
    {
        Debug.Log(Application.persistentDataPath);
        InitImageFile();
        InitSet();
        InitDataFile();
        InitSubmitFile();
        InitGoods();
    }

    #region Set
    public string SetFileName="set.xml";
    public string SetFilePath;
    public XmlDocument Data_Set_Xml;
    public WJ_Set Set;
    public XmlNode SetNode;
    private void InitSet()
    {
        Data_Set_Xml = new XmlDocument();
        Set = new WJ_Set();

        SetFilePath = string.Format("{0}/{1}", Application.persistentDataPath, SetFileName);
        if (File.Exists(SetFilePath))
        {
            string str = File.ReadAllText(SetFilePath);
            Data_Set_Xml.LoadXml(str);
            SetNode = Data_Set_Xml.SelectSingleNode("root");
            Set.WJ_Code = SetNode.Attributes["WJ_Code"].Value;
            Set.Place = SetNode.Attributes["Place"].Value;
            Set.FTPServer = SetNode.Attributes["FTPServer"].Value;
            Set.FTPPort = SetNode.Attributes["FTPPort"].Value;
            Set.DataServer = SetNode.Attributes["DataServer"].Value;
            Set.DataPort = SetNode.Attributes["DataPort"].Value;
            Set.CustomerID = SetNode.Attributes["CustomerID"].Value;
            Set.Password = SetNode.Attributes["Password"].Value;
            Set.RunType = SetNode.Attributes["RunType"].Value;
        }
        else
        {
            Data_Set_Xml.LoadXml("<root  WJ_Code='' Place='' FTPServer='' FTPPort='' DataServer='' DataPort='' CustomerID='' Password='' RunType='' ></root>");
            SetNode = Data_Set_Xml.SelectSingleNode("root");
        }
    }
    public void SaveSet()
    {
        SetNode.Attributes["WJ_Code"].Value = Set.WJ_Code;
        SetNode.Attributes["Place"].Value = Set.Place;
        SetNode.Attributes["FTPServer"].Value = Set.FTPServer;
        SetNode.Attributes["FTPPort"].Value = Set.FTPPort;
        SetNode.Attributes["DataServer"].Value = Set.DataServer;
        SetNode.Attributes["DataPort"].Value = Set.DataPort;
        SetNode.Attributes["CustomerID"].Value = Set.CustomerID;
        SetNode.Attributes["Password"].Value = Set.Password;
        SetNode.Attributes["RunType"].Value = Set.RunType;
        Data_Set_Xml.Save(SetFilePath);
    }
    public void SaveItem(string key,string value)
    {
        SetNode.Attributes[key].Value = value;
        Data_Set_Xml.Save(SetFilePath);
    }
    #endregion

    #region Data
    public int Total;
    public XmlDocument Data_Xml;
    public XmlNode data_photo_parent, data_record_parent;
    public string NowDataXmlPath;
    public Dictionary<string,WJ_Photo> Photos;
    public List<WJ_Record> Records;
    public void InitDataFile()
    {
        Data_Xml = new XmlDocument();
        Photos = new Dictionary<string, WJ_Photo>();
        Records = new List<WJ_Record>();

        string FilePath_Data = Application.persistentDataPath + "/data/";
        if (!Directory.Exists(FilePath_Data))
        {
            Directory.CreateDirectory(FilePath_Data);
        }
        else
        {
            DirectoryInfo theFolder = new DirectoryInfo(FilePath_Data);
            FileInfo[] fileInfos = theFolder.GetFiles();
            if (fileInfos != null && fileInfos.Length > 0)
            {
                List<FileInfo> ffs = fileInfos.ToList();
                ffs.Sort((s1, s2) => DateTime.Compare(s1.CreationTime, s2.CreationTime));
                for (int i = 0; i < ffs.Count; i++)
                {
                    TimeSpan ts = System.DateTime.Now.Subtract(ffs[i].CreationTime);
                    if (ts.Days >= 7)
                    {//删除7天前的数据
                        System.IO.File.Delete(ffs[i].FullName);
                        ffs.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        XmlDocument old = new XmlDocument();
                        old.LoadXml(File.ReadAllText(ffs[i].FullName));
                        Total += old.SelectNodes("root/record/item").Count;
                    }
                }
            }
        }
        //==========最新xml========
        NowDataXmlPath = string.Format("{0}{1}.xml", FilePath_Data, System.DateTime.Now.ToString("yyyy-MM-dd"));
        string str;
        if (File.Exists(NowDataXmlPath))
        {
            str = File.ReadAllText(NowDataXmlPath);
            Data_Xml.LoadXml(str);
            data_photo_parent = Data_Xml.SelectSingleNode("root/photo");
            data_record_parent = Data_Xml.SelectSingleNode("root/record");

            XmlNodeList nodes = data_photo_parent.SelectNodes("item");
            XmlNode node;
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                WJ_Photo photo = new WJ_Photo();
                photo.CustomerID = node.Attributes["CustomerID"].InnerText;
                photo.WJID = node.Attributes["WJID"].InnerText;
                photo.PhotoID = node.Attributes["PhotoID"].InnerText;
                photo.PhotoPath = node.Attributes["PhotoPath"].InnerText;
                photo.AtTime = node.Attributes["AtTime"].InnerText;
                photo.PhotoMiniPath = node.Attributes["PhotoMiniPath"].InnerText;
                Photos.Add(photo.PhotoID, photo);
            }

            nodes = data_record_parent.SelectNodes("item");
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                WJ_Record record = new WJ_Record();
                record.CustomerID = node.Attributes["CustomerID"].InnerText;
                record.WJID = node.Attributes["WJID"].InnerText;
                record.ID = node.Attributes["ID"].InnerText;
                record.WorkSpace = node.Attributes["WorkSpace"].InnerText;
                record.GoodsName = node.Attributes["GoodsName"].InnerText;

                record.BeginTime = node.Attributes["BeginTime"].InnerText;
                record.EndTime = node.Attributes["EndTime"].InnerText;
                record.BgeinPhotoID = node.Attributes["BgeinPhotoID"].InnerText;
                record.EndPhotoID = node.Attributes["EndPhotoID"].InnerText;
                record.longitude = node.Attributes["longitude"].InnerText;
                record.Latitude = node.Attributes["Latitude"].InnerText;
                record.Mode = node.Attributes["Mode"].InnerText;
                Records.Add(record);
            }
        }
        else
        {
            str = "<root><photo></photo><record></record></root>";
            Data_Xml.LoadXml(str);
            data_photo_parent = Data_Xml.SelectSingleNode("root/photo");
            data_record_parent = Data_Xml.SelectSingleNode("root/record");
        }

    }
    #endregion

    #region Submit
    public int LocalCount;
    public XmlDocument Data_Submit_Xml;
    public XmlNode submit_photo_parent, submit_record_parent;
    public Dictionary<string,RecordRequest> SubmitDatas_Olds;
    public RecordRequest SubmitDatas_New;
    public string NowSubmitXml;
    public string FilePath_Submit;
    public void InitSubmitFile()
    {
        Data_Submit_Xml = new XmlDocument();
        SubmitDatas_Olds = new Dictionary<string,RecordRequest>();
        SubmitDatas_New = new RecordRequest();
        FilePath_Submit = Application.persistentDataPath + "/submit/";
        if (!Directory.Exists(FilePath_Submit))
        {
            Directory.CreateDirectory(FilePath_Submit);
        }
        else
        {
            DirectoryInfo theFolder = new DirectoryInfo(FilePath_Submit);
            FileInfo[] fileInfos = theFolder.GetFiles();
            if (fileInfos != null && fileInfos.Length > 0)
            {
                List<FileInfo> ffs = fileInfos.ToList();
                ffs.Sort((s1, s2) => DateTime.Compare(s1.CreationTime, s2.CreationTime));
                for (int i = 0; i < ffs.Count; i++)
                {
                    if (ffs[i].Name == "new.xml") { continue; }
                    RecordRequest model = new RecordRequest();
                    XmlDocument old = new XmlDocument();
                    old.LoadXml(File.ReadAllText(ffs[i].FullName));
                    SetSubmitData(old,ref model);
                    model.id = ffs[i].Name;
                    SubmitDatas_Olds.Add(ffs[i].Name,model);
                    LocalCount += model.records.Count;

                }
            }
        }
        //==========最新xml========
        NowSubmitXml = FilePath_Submit + "new.xml";
        string str;
        if (File.Exists(NowSubmitXml))
        {
            str = File.ReadAllText(NowSubmitXml);
        }
        else
        {
            str = "<root><photo></photo><record></record></root>";
        }
        Data_Submit_Xml.LoadXml(str);
        submit_photo_parent = Data_Submit_Xml.SelectSingleNode("root/photo");
        submit_record_parent = Data_Submit_Xml.SelectSingleNode("root/record");
        SetSubmitData(Data_Submit_Xml,ref SubmitDatas_New);
        SubmitDatas_New.id = "new";
        LocalCount += SubmitDatas_New.records.Count;
        Debug.Log(SubmitDatas_New.photos.Count + "---" + SubmitDatas_New.records.Count);
    }
    public void SetSubmitData(XmlDocument old,ref RecordRequest SubmitDatas)
    {
        XmlNode node;
        XmlNodeList list = old.SelectNodes("root/photo/item");
        for (int j = 0; j < list.Count; j++)
        {
            node = list[j];
            RecordRequest.WJ_Photo_Submit photo = new RecordRequest.WJ_Photo_Submit();
            photo.CustomerID = node.Attributes["CustomerID"].Value;
            photo.WJID = node.Attributes["WJID"].Value;
            photo.PhotoID = node.Attributes["PhotoID"].Value;
            photo.PhotoPath = node.Attributes["PhotoPath"].Value;
            photo.AtTime = node.Attributes["AtTime"].Value;
            SubmitDatas.photos.Add(photo);
        }

        list = old.SelectNodes("root/record/item");
        for (int j = 0; j < list.Count; j++)
        {
            node = list[j];
            RecordRequest.WJ_Record_Submit record = new RecordRequest.WJ_Record_Submit();
            record.EndPhotoID = node.Attributes["EndPhotoID"].Value;
            if (!string.IsNullOrEmpty(record.EndPhotoID))
            {//排除还没完成二次拍照的记录
                record.SeqID = node.Attributes["SeqID"].Value;
                record.CustomerID = node.Attributes["CustomerID"].Value;
                record.WJID = node.Attributes["WJID"].Value;
                record.ID = node.Attributes["ID"].Value;
                record.WorkSpace = node.Attributes["WorkSpace"].Value;
                record.GoodsName = node.Attributes["GoodsName"].Value;
                record.BeginTime = node.Attributes["BeginTime"].Value;
                record.EndTime = node.Attributes["EndTime"].Value;
                record.BgeinPhotoID = node.Attributes["BgeinPhotoID"].Value;
                record.longitude = node.Attributes["longitude"].Value;
                record.Latitude = node.Attributes["Latitude"].Value;
                record.Mode = node.Attributes["Mode"].Value;
                SubmitDatas.records.Add(record);
            }
        }
    }
    private List<RecordResponse2> _SubmitRespinseData;
    public List<RecordResponse2> SubmitRespinseData
    {
        get
        {
            if (_SubmitRespinseData == null) { _SubmitRespinseData = new List<RecordResponse2>(); }
            return _SubmitRespinseData;
        }
    }
    public void AddSubmitRespinse(RecordResponse2 data)
    {
        SubmitRespinseData.Add(data);
    }
    public void DelSubmitRespinseData()
    {
        if (SubmitRespinseData.Count > 0)
        {
            List<string> photos = new List<string>();
            List<string> records = new List<string>();
            for (int j = 0; j < SubmitRespinseData.Count; j++)
            {
                photos.AddRange(SubmitRespinseData[j].photos);
                records.AddRange(SubmitRespinseData[j].records);
            }

            int num = submit_photo_parent.SelectNodes("item").Count;

            XmlNode node;
            for (int i = 0; i <submit_photo_parent.ChildNodes.Count; i++)
            {
                node = submit_photo_parent.ChildNodes[i];
                if (photos.Contains(node.Attributes["PhotoID"].Value))
                {
                    submit_photo_parent.RemoveChild(node);
                    i--;
                }
            }
            for (int i = 0; i < submit_record_parent.ChildNodes.Count; i++)
            {
                node = submit_record_parent.ChildNodes[i];
                if (records.Contains(node.Attributes["ID"].Value))
                {
                    submit_record_parent.RemoveChild(node);
                    i--;
                }
            }

            SubmitRespinseData.Clear();
            App.Instance.Data.Data_Submit_Xml.Save(App.Instance.Data.NowSubmitXml);
            SubmitDatas_New.photos.Clear();
            SubmitDatas_New.records.Clear();
            SetSubmitData(Data_Submit_Xml,ref SubmitDatas_New);
            App.Instance.Data.LocalCount = SubmitDatas_New.records.Count;
            OnValueChange();
        }
    }

    public bool Add(WJ_Photo wj_photo, System.DateTime dt, string GoodsName)
    {
        XmlElement node_photo = Data_Xml.CreateElement("item");
        node_photo.SetAttribute("CustomerID", wj_photo.CustomerID);
        node_photo.SetAttribute("WJID", wj_photo.WJID);
        node_photo.SetAttribute("PhotoID", wj_photo.PhotoID);
        node_photo.SetAttribute("PhotoPath", wj_photo.PhotoPath);
        node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
        node_photo.SetAttribute("AtTime", wj_photo.AtTime);
        data_photo_parent.AppendChild(node_photo);

        XmlElement node_photo_submit = Data_Submit_Xml.CreateElement("item");
        node_photo_submit.SetAttribute("CustomerID", wj_photo.CustomerID);
        node_photo_submit.SetAttribute("WJID", wj_photo.WJID);
        node_photo_submit.SetAttribute("PhotoID", wj_photo.PhotoID);
        node_photo_submit.SetAttribute("PhotoPath", wj_photo.PhotoPath);
        node_photo_submit.SetAttribute("AtTime", wj_photo.AtTime);
        submit_photo_parent.AppendChild(node_photo_submit);
        Photos.Add(wj_photo.PhotoID, wj_photo);
        if (Records.Count > 0 && string.IsNullOrEmpty(Records.Last().EndPhotoID))
        {
            WJ_Record wj_record = Records.Last();
            wj_record.EndPhotoID = wj_photo.PhotoID;
            wj_record.EndTime = wj_photo.AtTime;

            XmlNode node = submit_record_parent.LastChild;
            node.Attributes["EndPhotoID"].Value = wj_record.EndPhotoID;
            node.Attributes["EndTime"].Value = wj_record.EndTime;

            node = data_record_parent.LastChild;
            node.Attributes["EndPhotoID"].Value = wj_record.EndPhotoID;
            node.Attributes["EndTime"].Value = wj_record.EndTime;
            //===保存data
            Data_Xml.Save(App.Instance.Data.NowDataXmlPath);

            //===保存到xml
            if(submit_record_parent.ChildNodes.Count<1||App.Instance.Client.Events.ContainsKey("submit")&&App.Instance.Client.Events["submit"].RunState == 10)
            {//未满10条记录，或者有网络但是正在进行网络传输，保存到当前文件
                Data_Submit_Xml.Save(App.Instance.Data.NowSubmitXml);
                RecordRequest.WJ_Photo_Submit photo = new RecordRequest.WJ_Photo_Submit();
                photo.CustomerID = wj_photo.CustomerID;
                photo.WJID = wj_photo.WJID;
                photo.PhotoID = wj_photo.PhotoID;
                photo.PhotoPath = wj_photo.PhotoPath;
                photo.AtTime = wj_photo.AtTime;
                SubmitDatas_New.photos.Add(photo);
                RecordRequest.WJ_Record_Submit record = new RecordRequest.WJ_Record_Submit();
                record.CustomerID = wj_record.CustomerID;
                record.WJID = wj_record.WJID;
                record.ID = wj_record.ID;
                record.WorkSpace = wj_record.WorkSpace;
                record.GoodsName = wj_record.GoodsName;
                record.BeginTime = wj_record.BeginTime;
                record.EndTime = wj_record.EndTime;
                record.BgeinPhotoID = wj_record.BgeinPhotoID;
                record.EndPhotoID = wj_record.EndPhotoID;
                record.longitude = wj_record.longitude;
                record.Latitude = wj_record.Latitude;
                record.Mode = wj_record.Mode;
                SubmitDatas_New.records.Add(record);
            }
            else
            {//创建新文件保存文件
                string file_name = System.DateTime.Now.Ticks.ToString() + ".xml";
                Data_Submit_Xml.Save(App.Instance.Data.FilePath_Submit + file_name);
                //===上传整个文件的数据
                RecordRequest model = new RecordRequest();
                XmlDocument old = new XmlDocument();
                SetSubmitData(Data_Submit_Xml,ref model);
                model.id = file_name;
                SubmitDatas_Olds.Add(file_name, model);
                //====新文件=====
                Data_Submit_Xml.LoadXml("<root><photo></photo><record></record></root>");
                submit_photo_parent = App.Instance.Data.Data_Submit_Xml.SelectSingleNode("root/photo");
                submit_record_parent = App.Instance.Data.Data_Submit_Xml.SelectSingleNode("root/record");
                Data_Submit_Xml.Save(NowSubmitXml);
                SubmitDatas_New.photos.Clear();
                SubmitDatas_New.records.Clear();
            }
            LocalCount += 1;
            Total += 1;
            OnValueChange();
            return true;
        }
        else
        {
            RecordRequest.WJ_Photo_Submit photo = new RecordRequest.WJ_Photo_Submit();
            photo.CustomerID = wj_photo.CustomerID;
            photo.WJID = wj_photo.WJID;
            photo.PhotoID = wj_photo.PhotoID;
            photo.PhotoPath = wj_photo.PhotoPath;
            photo.AtTime = wj_photo.AtTime;
            SubmitDatas_New.photos.Add(photo);

            WJ_Record wj_record = new WJ_Record();
            wj_record.CustomerID = wj_photo.CustomerID;
            wj_record.WJID = wj_photo.WJID;
            wj_record.ID = dt.Ticks.ToString();
            wj_record.WorkSpace = App.Instance.Data.Set.Place;
            wj_record.GoodsName = GoodsName;
            wj_record.BeginTime = wj_photo.AtTime;
            wj_record.EndTime = "";
            wj_record.BgeinPhotoID = wj_photo.PhotoID;
            wj_record.EndPhotoID = "";
            wj_record.longitude = "";
            wj_record.Latitude = "";
            wj_record.Mode = "0";

            //======保存Data======
            Records.Add(wj_record);

            XmlElement node_data = Data_Xml.CreateElement("item");
            node_data.SetAttribute("CustomerID", wj_record.CustomerID);
            node_data.SetAttribute("WJID", wj_record.WJID);
            node_data.SetAttribute("ID", wj_record.ID);
            node_data.SetAttribute("WorkSpace", wj_record.WorkSpace);
            node_data.SetAttribute("GoodsName", wj_record.GoodsName);
            node_data.SetAttribute("BeginTime", wj_record.BeginTime);
            node_data.SetAttribute("EndTime", wj_record.EndTime);
            node_data.SetAttribute("BgeinPhotoID", wj_record.BgeinPhotoID);
            node_data.SetAttribute("EndPhotoID", wj_record.EndPhotoID);
            node_data.SetAttribute("longitude", wj_record.longitude);
            node_data.SetAttribute("Latitude", wj_record.Latitude);
            node_data.SetAttribute("Mode", wj_record.Mode);
            data_record_parent.AppendChild(node_data);
            //===保存到xml
            Data_Xml.Save(App.Instance.Data.NowDataXmlPath);

            //======保存Submit======

            XmlElement node_data_submit = Data_Submit_Xml.CreateElement("item");
            node_data_submit.SetAttribute("SeqID", dt.Ticks.ToString());
            node_data_submit.SetAttribute("CustomerID", wj_record.CustomerID);
            node_data_submit.SetAttribute("WJID", wj_record.WJID);
            node_data_submit.SetAttribute("ID", wj_record.ID);
            node_data_submit.SetAttribute("WorkSpace", wj_record.WorkSpace);
            node_data_submit.SetAttribute("GoodsName", wj_record.GoodsName);
            node_data_submit.SetAttribute("BeginTime", wj_record.BeginTime);
            node_data_submit.SetAttribute("EndTime", wj_record.EndTime);
            node_data_submit.SetAttribute("BgeinPhotoID", wj_record.BgeinPhotoID);
            node_data_submit.SetAttribute("EndPhotoID", wj_record.EndPhotoID);
            node_data_submit.SetAttribute("longitude", wj_record.longitude);
            node_data_submit.SetAttribute("Latitude", wj_record.Latitude);
            node_data_submit.SetAttribute("Mode", wj_record.Mode);
            submit_record_parent.AppendChild(node_data_submit);
            //===保存到xml
            Data_Submit_Xml.Save(App.Instance.Data.NowSubmitXml);
            return false;
        }
    }
    #endregion

    #region Goods
    public string GoodsFileName="goods.xml";
    public string GoodsFilePath;
    public XmlDocument Goods_Xml;
    public XmlNode Goods_parent;
    public List<GoodsResponse.WJ_Goods> Goods;
    public string GoodsTimeMax = "1000-00-00 00";
    private void InitGoods()
    {
        Goods_Xml = new XmlDocument();
        Goods = new List<GoodsResponse.WJ_Goods>();
        GoodsFilePath = string.Format("{0}/{1}", Application.persistentDataPath, GoodsFileName);
        if (!File.Exists(GoodsFilePath))
        {
            string StreamFilePath;
#if UNITY_EDITOR
            StreamFilePath = string.Format(@"Assets/StreamingAssets/{0}", GoodsFileName);
            File.Copy(StreamFilePath, GoodsFilePath);
#else
#if UNITY_ANDROID 
            var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + GoodsFileName);
            while (!loadDb.isDone) { }
            File.WriteAllBytes(GoodsFilePath, loadDb.bytes);
#endif
#endif
        }
        string str = File.ReadAllText(GoodsFilePath);
        Goods_Xml.LoadXml(str);
        Goods_parent = Goods_Xml.SelectSingleNode("root");
        XmlNodeList nodes = Goods_parent.SelectNodes("item");
        XmlNode node;
        for (int i = 0; i < nodes.Count; i++)
        {
            node = nodes[i];
            GoodsResponse.WJ_Goods good = new GoodsResponse.WJ_Goods();
            good.GoodsID = node.Attributes["GoodsID"].Value;
            good.GoodsName = node.Attributes["GoodsName"].Value;
            good.time = node.Attributes["time"].Value;
            if (String.Compare(good.time, GoodsTimeMax)>0)
            {
                GoodsTimeMax = good.time;
            }
            Goods.Add(good);
        }
        Debug.Log("Goods Max Time:" + GoodsTimeMax);
    }
    #endregion

    #region 图片
    public string ImgPath;
    public List<string> UpLoadFiles;
    public void InitImageFile()
    {
        UpLoadFiles = new List<string>();
        ImgPath = Application.persistentDataPath + "/img/";
        if (!System.IO.Directory.Exists(ImgPath))
        {
            System.IO.Directory.CreateDirectory(ImgPath);
            return;
        }
        else
        {
            GetLoadFiles();
        }
    }
    private void GetLoadFiles()
    {
        DirectoryInfo theFolder = new DirectoryInfo(ImgPath);
        FileInfo[] fileInfos = theFolder.GetFiles();
        if (fileInfos != null && fileInfos.Length > 0)
        {
            List<FileInfo> ffs = fileInfos.ToList();
            ffs.Sort((s1, s2) => DateTime.Compare(s1.CreationTime, s2.CreationTime));
            for (int i = 0; i < ffs.Count; i++)
            {
                if(ffs[i].Name[0]=='s')
                { continue; }
                UpLoadFiles.Add(ffs[i].Name);
            }
        }
    }
    #endregion

    #region 测试
    public static XmlDocument xmlDoc;
    public static void XmlInit()
    {
        xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<bookshop></bookshop>");
        XmlNode root = xmlDoc.SelectSingleNode("bookshop");
        for (int i = 0; i < 500; i++)
        {
            XmlElement xe1 = xmlDoc.CreateElement("book");
            for (int j = 0; j < 20; j++)
            {
                xe1.SetAttribute("k" + j, "1111111");
            }
            root.AppendChild(xe1);
        }
       
    }
    public static  void XmlTest()
    {
        string file = string.Format("{0}/{1}", Application.persistentDataPath, "1.xml");
        TimeSpan ts1 = new TimeSpan(System.DateTime.Now.Ticks);
        xmlDoc.Save(file);
        TimeSpan ts2 = new TimeSpan(System.DateTime.Now.Ticks);
        TimeSpan ts3 = ts1.Subtract(ts2).Duration();
        Debug.Log(ts3.ToString());
    }
    #endregion

    public event CallBack<int> ValueChange;
    public void ClearValueChange()
    {
        ValueChange = null;
    }
    public void OnValueChange()
    {
        if (ValueChange != null)
        {
            ValueChange(Total - LocalCount);
        }
    }
}