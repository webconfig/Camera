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
        InitSubmitImage();
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
    public XmlNode submit_record_parent;
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
            str = "<root></root>";
        }
        Data_Submit_Xml.LoadXml(str);
        submit_record_parent = Data_Submit_Xml.SelectSingleNode("record");
        SetSubmitData(Data_Submit_Xml,ref SubmitDatas_New);
        SubmitDatas_New.id = "new";
        LocalCount += SubmitDatas_New.records.Count;
        Debug.Log(SubmitDatas_New.records.Count);
    }
    public void SetSubmitData(XmlDocument old,ref RecordRequest SubmitDatas)
    {
        XmlNode node;
        string str;
        for (int j = 0; j < submit_record_parent.ChildNodes.Count; j++)
        {
            node = submit_record_parent.ChildNodes[j];
            str = node.Attributes["EndPhotoID"].Value;
            if (!string.IsNullOrEmpty(str))
            {//排除还没完成二次拍照的记录
                RecordRequest.WJ_Record_Submit record = new RecordRequest.WJ_Record_Submit();
                record.EndPhotoID = str;
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
    public bool Add(WJ_Photo wj_photo, System.DateTime dt, string GoodsName)
    {
        Photos.Add(wj_photo.PhotoID, wj_photo);
        //保存到图片上传表
        AddSubmitItem(wj_photo, dt);
        //保存图片到本地数据表
        XmlElement node_photo = Data_Xml.CreateElement("item");
        node_photo.SetAttribute("CustomerID", wj_photo.CustomerID);
        node_photo.SetAttribute("WJID", wj_photo.WJID);
        node_photo.SetAttribute("PhotoID", wj_photo.PhotoID);
        node_photo.SetAttribute("PhotoPath", wj_photo.PhotoPath);
        node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
        node_photo.SetAttribute("AtTime", wj_photo.AtTime);
        data_photo_parent.AppendChild(node_photo);

        if (Records.Count > 0 && string.IsNullOrEmpty(Records.Last().EndPhotoID))
        {
            WJ_Record wj_record = Records.Last();
            wj_record.EndPhotoID = wj_photo.PhotoID;
            wj_record.EndTime = wj_photo.AtTime;

            //修改本地数据表并保存
            XmlNode node = data_record_parent.LastChild;
            node.Attributes["EndPhotoID"].Value = wj_record.EndPhotoID;
            node.Attributes["EndTime"].Value = wj_record.EndTime;
            Data_Xml.Save(App.Instance.Data.NowDataXmlPath);

            //修改上传数据表
            node = submit_record_parent.LastChild;
            node.Attributes["EndPhotoID"].Value = wj_record.EndPhotoID;
            node.Attributes["EndTime"].Value = wj_record.EndTime;

            //===保存到xml
            if (submit_record_parent.ChildNodes.Count < 10 || App.Instance.DataServer.Events.ContainsKey("submit") && App.Instance.DataServer.Events["submit"].RunState == 1)
            {//未满10条记录，或者有网络但是正在进行网络传输，保存到当前文件
                Data_Submit_Xml.Save(App.Instance.Data.NowSubmitXml);

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
            { 
                //创建新文件
                string file_name = System.DateTime.Now.Ticks.ToString() + ".xml";
                Data_Submit_Xml.Save(App.Instance.Data.FilePath_Submit + file_name);
                //===上传整个文件的数据
                RecordRequest model = new RecordRequest();
                SetSubmitData(Data_Submit_Xml,ref model);
                model.id = file_name;
                SubmitDatas_Olds.Add(file_name, model);
                //====新文件=====
                Data_Submit_Xml.LoadXml("<root></root>");
                submit_record_parent = App.Instance.Data.Data_Submit_Xml.SelectSingleNode("root");
                Data_Submit_Xml.Save(NowSubmitXml);
                SubmitDatas_New.records.Clear();
            }
            LocalCount += 1;
            Total += 1;
            OnValueChange();
            return true;
        }
        else
        {
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
            //修改本地数据表并保存
            Data_Xml.Save(App.Instance.Data.NowDataXmlPath);

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
            //修改上传数据表并保存
            Data_Submit_Xml.Save(App.Instance.Data.NowSubmitXml);
            return false;
        }
    }
    #region 处理完成上传
    public List<RecordResponse2> RecvDatas = new List<RecordResponse2>();
    public void AddSubmitRespinse(RecordResponse2 data)
    {
        RecvDatas.Add(data);
    }
    /// <summary>
    /// 处理上传的数据
    /// </summary>
    public void DelSubmitRespinseData()
    {
        if (RecvDatas.Count > 0)
        {
            //获取所有上传的记录
            List<string> records = new List<string>();
            for (int j = 0; j < RecvDatas.Count; j++)
            {
                records.AddRange(RecvDatas[j].records);
            }
            RecvDatas.Clear();

            //删除掉本地xml的数据
            XmlNode node;
            for (int i = 0; i < submit_record_parent.ChildNodes.Count; i++)
            {
                node = submit_record_parent.ChildNodes[i];
                if (records.Contains(node.Attributes["ID"].Value))
                {
                    submit_record_parent.RemoveChild(node);
                    i--;
                }
            }
            App.Instance.Data.Data_Submit_Xml.Save(App.Instance.Data.NowSubmitXml);

            //====重新读取数据====
            SubmitDatas_New.records.Clear();
            SetSubmitData(Data_Submit_Xml, ref SubmitDatas_New);
            App.Instance.Data.LocalCount = SubmitDatas_New.records.Count;
            OnValueChange();
        }
        if(GoodsChange)
        {
            GoodsChange = false;
            if(GoodsChangeEvent!=null)
            {
                GoodsChangeEvent();
            }
        }
    }
    #endregion
    #endregion

    #region Goods
    public string GoodsFileName="goods.xml";
    public string GoodsFilePath;
    public XmlDocument Goods_Xml;
    public XmlNode Goods_parent;
    public List<GoodsResponse.WJ_Goods> Goods;
    public string GoodsTimeMax = "1000-00-00 00";
    public bool GoodsChange;
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

    public void AddGoods(GoodsResponse response)
    {
        GoodsResponse.WJ_Goods goods_item;
        for (int i = 0; i < response.result.Count; i++)
        {
            goods_item = response.result[i];
            XmlElement node_photo = App.Instance.Data.Goods_Xml.CreateElement("item");
            node_photo.SetAttribute("GoodsID", goods_item.GoodsID);
            node_photo.SetAttribute("GoodsName", goods_item.GoodsName);
            node_photo.SetAttribute("time", goods_item.time);
            Goods_parent.AppendChild(node_photo);
            Goods.Add(goods_item);
            if (String.Compare(goods_item.time, GoodsTimeMax) > 0)
            {
                GoodsTimeMax = goods_item.time;
            }
        }
        Goods_Xml.Save(GoodsFilePath);
    }

    #endregion

    #region 图片
    public string ImgPath;
    public string SubmitImageFileName = "image.xml";
    public string SubmitImageFilePath;
    public XmlDocument SubmitImage_Xml;
    public XmlNode SubmitImage_Parent;
    public Dictionary<string,FileRequest> FileRequestDatas;
    private void InitSubmitImage()
    {
        ImgPath = Application.persistentDataPath + "/img/";
        SubmitImage_Xml = new XmlDocument();
        FileRequestDatas = new Dictionary<string,FileRequest>();

        SubmitImageFilePath = string.Format("{0}/{1}", Application.persistentDataPath, SubmitImageFileName);
        if (File.Exists(SubmitImageFileName))
        {
            string str = File.ReadAllText(SubmitImageFileName);
            SubmitImage_Xml.LoadXml(str);
            SubmitImage_Parent = SubmitImage_Xml.SelectSingleNode("root");
            XmlNode node;
            for (int i = 0; i < SubmitImage_Parent.ChildNodes.Count; i++)
            {
                node = SubmitImage_Parent.ChildNodes[i];
                FileRequest item = new FileRequest();
                item.CustomerID = node.Attributes["CustomerID"].Value;
                item.WJID = node.Attributes["WJID"].Value;
                item.PhotoID = node.Attributes["PhotoID"].Value;
                item.PhotoPath = node.Attributes["PhotoPath"].Value;
                item.AtTime = node.Attributes["AtTime"].Value;
                item.SeqID = node.Attributes["SeqID"].Value;
                FileRequestDatas.Add(item.SeqID, item);
            }
        }
        else
        {
            SubmitImage_Xml.LoadXml("<root></root>");
            SubmitImage_Parent = SubmitImage_Xml.SelectSingleNode("root");
        }
    }

    /// <summary>
    /// 新加一个上传的图片
    /// </summary>
    /// <param name="wj_photo"></param>
    /// <param name="dt"></param>
    private void AddSubmitItem(WJ_Photo wj_photo,System.DateTime dt)
    {
        XmlElement node_photo_submit = SubmitImage_Xml.CreateElement("item");
        node_photo_submit.SetAttribute("CustomerID", wj_photo.CustomerID);
        node_photo_submit.SetAttribute("WJID", wj_photo.WJID);
        node_photo_submit.SetAttribute("PhotoID", wj_photo.PhotoID);
        node_photo_submit.SetAttribute("PhotoPath", wj_photo.PhotoPath);
        node_photo_submit.SetAttribute("AtTime", wj_photo.AtTime);
        node_photo_submit.SetAttribute("SeqID", dt.Ticks.ToString());
        SubmitImage_Parent.AppendChild(node_photo_submit);
        SubmitImage_Xml.Save(SubmitImageFilePath);

        FileRequest photo = new FileRequest();
        photo.CustomerID = wj_photo.CustomerID;
        photo.WJID = wj_photo.WJID;
        photo.PhotoID = wj_photo.PhotoID;
        photo.PhotoPath = wj_photo.PhotoPath;
        photo.AtTime = wj_photo.AtTime;
        FileRequestDatas.Add(dt.Ticks.ToString(), photo);
    }

    public void DelectSubmitItem(FileRequest item)
    {
        FileRequestDatas.Remove(item.SeqID);
        File.Delete(App.Instance.Data.ImgPath + item.PhotoPath);//删除本地的大图，保留小图
        XmlNode node;
        for (int i = 0; i < SubmitImage_Parent.ChildNodes.Count; i++)
        {
            node = SubmitImage_Parent.ChildNodes[i];
            if (String.Compare(node.Attributes["ID"].Value, item.SeqID)==0)
            {
                submit_record_parent.RemoveChild(node);
                break;
            }
        }
        SubmitImage_Xml.Save(SubmitImageFilePath);
    }
    #endregion

    public event CallBack GoodsChangeEvent;
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