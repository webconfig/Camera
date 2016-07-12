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
    public string SubmitDataXmlPath;
    /// <summary>
    /// 本地未上传记录数
    /// </summary>
    public int LocalCount;
    public List<RecordXmlData> SubmitDatas;
    public RecordXmlData SubmitDataNew;
    public void InitSubmitFile()
    {
        SubmitDatas = new List<RecordXmlData>();
        SubmitDataXmlPath = Application.persistentDataPath + "/submit/";
        bool IsNew = false;
        if (!Directory.Exists(SubmitDataXmlPath))
        {
            Directory.CreateDirectory(SubmitDataXmlPath);
        }
        else
        {
            DirectoryInfo theFolder = new DirectoryInfo(SubmitDataXmlPath);
            FileInfo[] fileInfos = theFolder.GetFiles();
            if (fileInfos != null && fileInfos.Length > 0)
            {
                List<FileInfo> ffs = fileInfos.ToList();
                ffs.Sort((s1, s2) => DateTime.Compare(s2.CreationTime, s1.CreationTime));
                for (int i = 0; i < ffs.Count; i++)
                {
                    RecordXmlData data_item = new RecordXmlData();
                    data_item.RequestDatas = new RecordRequest();
                    data_item.XmlPath = ffs[i].FullName;
                    data_item.Xml = new XmlDocument();
                    data_item.Xml.LoadXml(File.ReadAllText(ffs[i].FullName));
                    data_item.NodeParent = data_item.Xml.FirstChild;

                    data_item.RequestDatas = new RecordRequest();
                    string str;
                    XmlNode node;
                    int num = data_item.NodeParent.ChildNodes.Count;
                    for (int j = 0; j < num; j++)
                    {
                        node = data_item.NodeParent.ChildNodes[j];
                        str = node.Attributes["EndPhotoID"].Value;
                        if (string.IsNullOrEmpty(str))
                        {
                            IsNew = true;
                        }
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
                        data_item.RequestDatas.records.Add(record);

                    }
                    if(IsNew)
                    {
                        SubmitDataNew = data_item;
                    }
                    else
                    {
                        SubmitDatas.Add(data_item);
                    }
                    LocalCount += data_item.RequestDatas.records.Count;
                }

            }
        }
        if (!IsNew)
        {
            NewSubmitFile();
        }
    }
    /// <summary>
    /// 创建一个新文件
    /// </summary>
    private void NewSubmitFile()
    {
        SubmitDataNew = new RecordXmlData();
        SubmitDataNew.XmlName = System.DateTime.Now.Ticks.ToString();
        SubmitDataNew.XmlPath = string.Concat(SubmitDataXmlPath, SubmitDataNew.XmlName,".xml");
        SubmitDataNew.Xml = new XmlDocument();
        NowImgXml.Xml.LoadXml("<root></root>");
        NowImgXml.NodeParent = NowImgXml.Xml.FirstChild;
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
            node = SubmitDataNew.NodeParent.LastChild;
            node.Attributes["EndPhotoID"].Value = wj_record.EndPhotoID;
            node.Attributes["EndTime"].Value = wj_record.EndTime;
            SubmitDataNew.Xml.Save(SubmitDataNew.XmlPath);

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
            SubmitDataNew.RequestDatas.records.Add(record);

            if (SubmitDataNew.NodeParent.ChildNodes.Count>=10)
            {//超过10条记录，创建新文件
                //复制到旧数据
                SubmitDatas.Add(SubmitDataNew.Copy());
                //创建文件
                NewSubmitFile();
            }
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

            XmlElement node_data_submit =SubmitDataNew.Xml.CreateElement("item");
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
            SubmitDataNew.NodeParent.AppendChild(node_data_submit);
            //修改上传数据表并保存
            SubmitDataNew.Xml.Save(SubmitDataNew.XmlPath);

            LocalCount += 1;
            Total += 1;
            OnValueChange();
            return false;
        }
    }


    #region 处理完成上传
    public List<RecordResponse> RecvDatas = new List<RecordResponse>();
    public void AddSubmitRespinse(RecordResponse data)
    {
        RecvDatas.Add(data);
    }
    /// <summary>
    /// 处理上传的数据
    /// </summary>
    public void DelData()
    {
        List<string> remove_keys = new List<string>();
        if (RecvDatas.Count > 0)
        {
            //获取所有上传的记录
            List<string> records = new List<string>();
            for (int j = 0; j < RecvDatas.Count; j++)
            {
                records.AddRange(RecvDatas[j].records);
            }
            RecvDatas.Clear();

            //=========处理数据===========
            if (SubmitDatas.Count>0)
            {
                for (int k = 0; k < SubmitDatas.Count; k++)
                {
                    if (SubmitDatas[k].RemoveData(ref records))
                    {
                        SubmitDatas.RemoveAt(k);
                        k--;
                    }
                    if(records.Count==0)
                    {
                        break;
                    }
                }
            }

            //=====统计========
            LocalCount = 0;
            for (int k = 0; k < SubmitDatas.Count; k++)
            {
                LocalCount += SubmitDatas[k].RequestDatas.records.Count;
            }
            OnValueChange();
        }
        if (GoodsChange)
        {
            GoodsChange = false;
            if (GoodsChangeEvent != null)
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
    public string ImgXmlPath;
    public List<ImgXmlData> UpLoadImgXmls;
    public ImgXmlData NowImgXml;
    private void InitSubmitImage()
    {
        UpLoadImgXmls = new List<ImgXmlData>();
        ImgPath = Application.persistentDataPath + "/img/";
        ImgXmlPath = Application.persistentDataPath + "/img_xml/";
        if (!Directory.Exists(ImgXmlPath))
        {
            Directory.CreateDirectory(ImgXmlPath);
        }
        else
        {
            DirectoryInfo theFolder = new DirectoryInfo(ImgXmlPath);
            FileInfo[] fileInfos = theFolder.GetFiles();
            if (fileInfos != null && fileInfos.Length > 0)
            {
                List<FileInfo> ffs = fileInfos.ToList();
                ffs.Sort((s1, s2) => DateTime.Compare(s1.CreationTime, s2.CreationTime));
                for (int i = 0; i < ffs.Count; i++)
                {//便利文件夹里面的文件
                    ImgXmlData xml_data = new ImgXmlData();
                    xml_data.XmlPath = ffs[i].FullName;
                    xml_data.Xml = new XmlDocument();
                    xml_data.Xml.LoadXml(File.ReadAllText(ffs[i].FullName));
                    xml_data.NodeParent = xml_data.Xml.FirstChild;
                    xml_data.FileRequestDatas = new Dictionary<string, FileRequest>();
                    XmlNode node;
                    for (int j = 0; j < xml_data.NodeParent.ChildNodes.Count; j++)
                    {
                        node = xml_data.NodeParent.ChildNodes[j];
                        FileRequest item = new FileRequest();
                        item.CustomerID = node.Attributes["CustomerID"].Value;
                        item.WJID = node.Attributes["WJID"].Value;
                        item.PhotoID = node.Attributes["PhotoID"].Value;
                        item.PhotoPath = node.Attributes["PhotoPath"].Value;
                        item.AtTime = node.Attributes["AtTime"].Value;
                        item.SeqID = node.Attributes["SeqID"].Value;
                        xml_data.FileRequestDatas.Add(item.SeqID, item);

                    }
                    UpLoadImgXmls.Add(xml_data);
                }
            }
        }
        //==========创建最新xml========
        NewXml();
    }
    /// <summary>
    /// 创建一个新文件
    /// </summary>
    private void NewXml()
    {
        NowImgXml = new ImgXmlData();
        NowImgXml.XmlPath = string.Concat(FilePath_Submit, System.DateTime.Now.Ticks.ToString(), ".xml");
        NowImgXml.Xml = new XmlDocument();
        NowImgXml.Xml.LoadXml("<root></root>");
        NowImgXml.NodeParent = NowImgXml.Xml.FirstChild;
    }
    /// <summary>
    /// 新加一个上传的图片
    /// </summary>
    /// <param name="wj_photo"></param>
    /// <param name="dt"></param>
    private void AddSubmitItem(WJ_Photo wj_photo,System.DateTime dt)
    {
        XmlElement node_photo_submit = NowImgXml.Xml.CreateElement("item");
        node_photo_submit.SetAttribute("CustomerID", wj_photo.CustomerID);
        node_photo_submit.SetAttribute("WJID", wj_photo.WJID);
        node_photo_submit.SetAttribute("PhotoID", wj_photo.PhotoID);
        node_photo_submit.SetAttribute("PhotoPath", wj_photo.PhotoPath);
        node_photo_submit.SetAttribute("AtTime", wj_photo.AtTime);
        node_photo_submit.SetAttribute("SeqID", dt.Ticks.ToString());
        NowImgXml.NodeParent.AppendChild(node_photo_submit);
        NowImgXml.Xml.Save(NowImgXml.XmlPath);

        FileRequest photo = new FileRequest();
        photo.CustomerID = wj_photo.CustomerID;
        photo.WJID = wj_photo.WJID;
        photo.PhotoID = wj_photo.PhotoID;
        photo.PhotoPath = wj_photo.PhotoPath;
        photo.AtTime = wj_photo.AtTime;
        NowImgXml.FileRequestDatas.Add(dt.Ticks.ToString(), photo);

        if(NowImgXml.NodeParent.ChildNodes.Count>=10)
        {//超过10条，就创建一个新文件
            UpLoadImgXmls.Add(NowImgXml);
            NewXml();
        }
    }
    /// <summary>
    /// 删除一条记录
    /// </summary>
    /// <param name="item"></param>
    public void DelectSubmitItem(FileRequest item)
    {
        bool IsOld = false;
        int index = 0;
        ImgXmlData xml_item=null;
        if(UpLoadImgXmls.Count>0)
        {//查询老记录
            for (int i = 0; i < UpLoadImgXmls.Count; i++)
            {
                if (UpLoadImgXmls[i].FileRequestDatas.ContainsKey(item.SeqID))
                {//找到
                    xml_item = UpLoadImgXmls[i];
                    index = i;
                    IsOld = true;
                    break;
                }
            }
        }
        else if(NowImgXml.FileRequestDatas.Count>0)
        {//查询最新记录
            if (NowImgXml.FileRequestDatas.ContainsKey(item.SeqID))
            {//找到
                xml_item = NowImgXml;
                IsOld = false;
            }
        }

        if(xml_item!=null)
        {
            xml_item.FileRequestDatas.Remove(item.SeqID);
            File.Delete(App.Instance.Data.ImgPath + item.PhotoPath);//删除本地的大图，保留小图
            if (xml_item.FileRequestDatas.Count > 0)
            {//还有数据
                //====修改xml,并保存===
                XmlNode node;
                for (int k = 0; k < xml_item.NodeParent.ChildNodes.Count; k++)
                {
                    node = xml_item.NodeParent.ChildNodes[k];
                    if (String.Compare(node.Attributes["SeqID"].Value, item.SeqID) == 0)
                    {
                        submit_record_parent.RemoveChild(node);
                        break;
                    }
                }
                xml_item.Xml.Save(xml_item.XmlPath);
            }
            else if (IsOld)
            {//没有数据，是老记录，删除该文件
                File.Delete(xml_item.XmlPath);
                UpLoadImgXmls.RemoveAt(index);
            }
        }
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
public class ImgXmlData
{
    public string XmlPath;
    public Dictionary<string, FileRequest> FileRequestDatas;
    public XmlNode NodeParent;
    public XmlDocument Xml;
}
public class RecordXmlData
{
    public string XmlName;
    public string XmlPath;
    public RecordRequest RequestDatas;
    public XmlNode NodeParent;
    public XmlDocument Xml;
    public RecordXmlData Copy()
    {
        RecordXmlData result = new RecordXmlData();
        result.XmlName = this.XmlName;
        result.NodeParent = this.NodeParent;
        result.Xml = this.Xml;
        result.RequestDatas = new RecordRequest();
        for (int j = 0; j < this.RequestDatas.records.Count; j++)
        {
            RecordRequest.WJ_Record_Submit new_record = new RecordRequest.WJ_Record_Submit();
            RecordRequest.WJ_Record_Submit old_record = this.RequestDatas.records[j];
            new_record.EndPhotoID = old_record.EndPhotoID;
            new_record.SeqID = old_record.SeqID;
            new_record.CustomerID = old_record.CustomerID;
            new_record.WJID = old_record.WJID;
            new_record.ID = old_record.ID;
            new_record.WorkSpace = old_record.WorkSpace;
            new_record.GoodsName = old_record.GoodsName;
            new_record.BeginTime = old_record.BeginTime;
            new_record.EndTime = old_record.EndTime;
            new_record.BgeinPhotoID = old_record.BgeinPhotoID;
            new_record.longitude = old_record.longitude;
            new_record.Latitude = old_record.Latitude;
            new_record.Mode = old_record.Mode;
            result.RequestDatas.records.Add(new_record);
        }
        return result;
    }
    public List<string> remove_ids = new List<string>();


    public bool RemoveData(ref List<string> datas)
    {
        for (int i = 0; i < RequestDatas.records.Count; i++)
        {
            for (int j = 0; j < datas.Count; j++)
            {
                if (String.Compare(RequestDatas.records[i].SeqID, datas[j]) == 0)
                {
                    remove_ids.Add(datas[j]);
                    datas.RemoveAt(j);
                    RequestDatas.records.RemoveAt(i);
                    i--;
                    break;
                }   
            }
        }

        if (RequestDatas.records.Count > 0)
        {
            if (remove_ids.Count > 0)
            {
                for (int i = 0; i < NodeParent.ChildNodes.Count; i++)
                {
                    if (remove_ids.Contains(NodeParent.ChildNodes[i].Attributes["SeqID"].Value))
                    {
                        NodeParent.RemoveChild(NodeParent.ChildNodes[i]);
                    }
                }
                Xml.Save(XmlPath);
                remove_ids.Clear();
            }
            return false;
        }
        else
        {
            File.Delete(XmlPath);
            NodeParent = null;
            Xml = null;
            RequestDatas = null;
            remove_ids = null;
            return true;
        }
    }
}
