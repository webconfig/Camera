using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Xml;
using UnityEngine;
using google.protobuf;
public class AppData
{
    public DateTime StartTime;
    public void Init()
    {
        StartTime = System.DateTime.Now.Date;
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
        if (!File.Exists(SetFilePath))
        {
            string StreamFilePath;
#if UNITY_EDITOR
            StreamFilePath = string.Format(@"Assets/StreamingAssets/{0}", SetFileName);
            File.Copy(StreamFilePath, SetFilePath);
#else
#if UNITY_ANDROID 
            var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + SetFileName);
            while (!loadDb.isDone) { }
            File.WriteAllBytes(SetFilePath, loadDb.bytes);
#endif
#endif
        }
        Data_Set_Xml.LoadXml(File.ReadAllText(SetFilePath));
        SetNode = Data_Set_Xml.SelectSingleNode("root");
        Set.WJ_Code = SetNode.Attributes["WJ_Code"].Value;
        Set.Place = SetNode.Attributes["Place"].Value;
        Set.DataServer = SetNode.Attributes["DataServer"].Value;
        Set.DataPort = SetNode.Attributes["DataPort"].Value;
        Set.CustomerID =long.Parse(SetNode.Attributes["CustomerID"].Value);
        Set.Password = SetNode.Attributes["Password"].Value;
        Set.RunType =int.Parse(SetNode.Attributes["RunType"].Value);
        Set.Total = int.Parse(SetNode.Attributes["Total"].Value);
        Set.CD = float.Parse(SetNode.Attributes["CD"].Value);
    }
    public void SaveSet()
    {
        SetNode.Attributes["WJ_Code"].Value = Set.WJ_Code;
        SetNode.Attributes["Place"].Value = Set.Place;
        SetNode.Attributes["DataServer"].Value = Set.DataServer;
        SetNode.Attributes["DataPort"].Value = Set.DataPort;
        SetNode.Attributes["CustomerID"].Value = Set.CustomerID.ToString();
        SetNode.Attributes["Password"].Value = Set.Password;
        SetNode.Attributes["RunType"].Value = Set.RunType.ToString();
        SetNode.Attributes["CD"].Value = Set.CD.ToString();
        Data_Set_Xml.Save(SetFilePath);
    }
    public void SaveTotal()
    {
        SetNode.Attributes["Total"].Value = Set.Total.ToString();
        Data_Set_Xml.Save(SetFilePath);
    }
    public void SaveItem(string key,string value)
    {
        SetNode.Attributes[key].Value = value;
        Data_Set_Xml.Save(SetFilePath);
    }
    #endregion

    #region Data
    public string LocalDataParentPath;
    /// <summary>
    /// 本地当天记录
    /// </summary>
    public LocalXmlData LocalData;
    public void InitDataFile()
    {
        LocalDataParentPath = Application.persistentDataPath + "/data/";
        if (!Directory.Exists(LocalDataParentPath))
        {
            Directory.CreateDirectory(LocalDataParentPath);
        }
        else
        {
            //删除7天前的数据
            DirectoryInfo theFolder = new DirectoryInfo(LocalDataParentPath);
            FileInfo[] fileInfos = theFolder.GetFiles();
            if (fileInfos != null && fileInfos.Length > 0)
            {
                List<FileInfo> ffs = fileInfos.ToList();
                ffs.Sort((s1, s2) => DateTime.Compare(s1.CreationTime, s2.CreationTime));
                for (int i = 0; i < ffs.Count; i++)
                {
                    TimeSpan ts = System.DateTime.Now.Subtract(ffs[i].CreationTime);
                    if (ts.Days >= 7)
                    {
                        System.IO.File.Delete(ffs[i].FullName);
                        ffs.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        //获取当天记录
        string NowDataXmlPath = string.Format("{0}{1}.xml", LocalDataParentPath, System.DateTime.Now.ToString("yyyy-MM-dd"));
        if (File.Exists(NowDataXmlPath))
        {
            NewLocalData(NowDataXmlPath, File.ReadAllText(NowDataXmlPath));

            XmlNodeList nodes = LocalData.photo_parent.SelectNodes("item");
            XmlNode node;
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                WJ_Photo photo = new WJ_Photo();
                photo.CustomerID =long.Parse(node.Attributes["CustomerID"].InnerText);
                photo.WJID = node.Attributes["WJID"].InnerText;
                photo.PhotoID = node.Attributes["PhotoID"].InnerText;
                photo.PhotoPath = node.Attributes["PhotoPath"].InnerText;
                photo.AtTime = node.Attributes["AtTime"].InnerText;
                photo.PhotoMiniPath = node.Attributes["PhotoMiniPath"].InnerText;
                LocalData.Photos.Add(photo.PhotoID, photo);
            }

            nodes = LocalData.record_parent.SelectNodes("item");
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                WJ_Record record = new WJ_Record();
                record.CustomerID = long.Parse(node.Attributes["CustomerID"].InnerText);
                record.WJID = node.Attributes["WJID"].InnerText;
                record.ID =long.Parse( node.Attributes["ID"].InnerText);
                record.WorkSpace = node.Attributes["WorkSpace"].InnerText;
                record.GoodsName = node.Attributes["GoodsName"].InnerText;

                record.BeginTime = node.Attributes["BeginTime"].InnerText;
                record.EndTime = node.Attributes["EndTime"].InnerText;
                record.BgeinPhotoID = node.Attributes["BgeinPhotoID"].InnerText;
                record.EndPhotoID = node.Attributes["EndPhotoID"].InnerText;
                record.longitude =float.Parse(node.Attributes["longitude"].InnerText);
                record.Latitude = float.Parse(node.Attributes["Latitude"].InnerText);
                record.Mode = int.Parse(node.Attributes["Mode"].InnerText);
                LocalData.Records.Add(record);
            }
        }
        else
        {//创建新记录
            NewLocalData(NowDataXmlPath, "<root><photo></photo><record></record></root>");
        }

    }
    private void NewLocalData(string path, string xmldata)
    {
        LocalData = new LocalXmlData();
        LocalData.XmlPath = path;
        LocalData.Xml = new XmlDocument();
        LocalData.Xml.LoadXml(xmldata);
        LocalData.photo_parent = LocalData.Xml.SelectSingleNode("root/photo");
        LocalData.record_parent = LocalData.Xml.SelectSingleNode("root/record");
        LocalData.Photos = new Dictionary<string, WJ_Photo>();
        LocalData.Records = new List<WJ_Record>();
    }
    public WJ_Record SaveItem(WJ_Photo wj_photo, System.DateTime dt, string GoodsName)
    {
        LocalData.Photos.Add(wj_photo.PhotoID, wj_photo);
        WJ_Record wj_record = new WJ_Record();
        wj_record.CustomerID = wj_photo.CustomerID;
        wj_record.WJID = wj_photo.WJID;
        wj_record.ID = dt.Ticks;
        wj_record.WorkSpace = App.Instance.Data.Set.Place;
        wj_record.GoodsName = GoodsName;
        wj_record.BeginTime = wj_photo.AtTime;
        wj_record.EndTime = "";
        wj_record.BgeinPhotoID = wj_photo.PhotoID;
        wj_record.EndPhotoID = "";
        wj_record.longitude = 0;
        wj_record.Latitude = 0;
        wj_record.Mode = App.Instance.Data.Set.RunType;
        LocalData.Records.Add(wj_record);

        //添加图片
        XmlElement node_photo = LocalData.Xml.CreateElement("item");
        node_photo.SetAttribute("CustomerID", wj_photo.CustomerID.ToString());
        node_photo.SetAttribute("WJID", wj_photo.WJID);
        node_photo.SetAttribute("PhotoID", wj_photo.PhotoID);
        node_photo.SetAttribute("PhotoPath", wj_photo.PhotoPath);
        node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
        node_photo.SetAttribute("AtTime", wj_photo.AtTime);
        LocalData.photo_parent.AppendChild(node_photo);

        XmlElement node_data = LocalData.Xml.CreateElement("item");
        node_data.SetAttribute("CustomerID", wj_record.CustomerID.ToString());
        node_data.SetAttribute("WJID", wj_record.WJID);
        node_data.SetAttribute("ID", wj_record.ID.ToString());
        node_data.SetAttribute("WorkSpace", wj_record.WorkSpace);
        node_data.SetAttribute("GoodsName", wj_record.GoodsName);
        node_data.SetAttribute("BeginTime", wj_record.BeginTime);
        node_data.SetAttribute("EndTime", wj_record.EndTime);
        node_data.SetAttribute("BgeinPhotoID", wj_record.BgeinPhotoID);
        node_data.SetAttribute("EndPhotoID", wj_record.EndPhotoID);
        node_data.SetAttribute("longitude", wj_record.longitude.ToString());
        node_data.SetAttribute("Latitude", wj_record.Latitude.ToString());
        node_data.SetAttribute("Mode", wj_record.Mode.ToString());
        LocalData.record_parent.AppendChild(node_data);
        LocalData.Xml.Save(LocalData.XmlPath);
        return wj_record;
    }
    public void UpdateItem(WJ_Photo wj_photo)
    {
        LocalData.Photos.Add(wj_photo.PhotoID, wj_photo);
        WJ_Record wj_record = LocalData.Records.Last();
        wj_record.EndPhotoID = wj_photo.PhotoID;
        wj_record.EndTime = wj_photo.AtTime;

        //添加图片
        XmlElement node_photo = LocalData.Xml.CreateElement("item");
        node_photo.SetAttribute("CustomerID",wj_photo.CustomerID.ToString());
        node_photo.SetAttribute("WJID", wj_photo.WJID);
        node_photo.SetAttribute("PhotoID", wj_photo.PhotoID);
        node_photo.SetAttribute("PhotoPath", wj_photo.PhotoPath);
        node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
        node_photo.SetAttribute("AtTime", wj_photo.AtTime);
        LocalData.photo_parent.AppendChild(node_photo);
        //===修改Record
        XmlNode node = LocalData.record_parent.LastChild;
        node.Attributes["EndPhotoID"].Value = wj_record.EndPhotoID;
        node.Attributes["EndTime"].Value = wj_record.EndTime;
        LocalData.Xml.Save(LocalData.XmlPath);
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
                ffs.Sort((s1, s2) => DateTime.Compare(s1.CreationTime, s2.CreationTime));
                for (int i = 0; i < ffs.Count; i++)
                {
                    //Debug.Log(ffs[i].CreationTime);
                    RecordXmlData data_item = new RecordXmlData();
                    data_item.RequestDatas = new RecordRequest();
                    data_item.XmlPath = ffs[i].FullName;
                    data_item.Xml = new XmlDocument();
                    data_item.Xml.LoadXml(File.ReadAllText(ffs[i].FullName));
                    data_item.NodeParent = data_item.Xml.FirstChild;
                    data_item.Remove = true;
                    data_item.RequestDatas = new RecordRequest();
                    string str;
                    XmlNode node;
                    int num = data_item.NodeParent.ChildNodes.Count;
                    for (int j = 0; j < num; j++)
                    {
                        node = data_item.NodeParent.ChildNodes[j];
                        str = node.Attributes["EndPhotoID"].Value;
                        if (string.IsNullOrEmpty(str) && (i == (ffs.Count - 1)))
                        {//最后一条没有结束图片，继续写这个文件
                            IsNew = true;
                        }
                        else
                        {
                            RecordRequest.WJ_Record_Submit record = new RecordRequest.WJ_Record_Submit();
                            record.EndPhotoID = str;
                            //record.SeqID = node.Attributes["SeqID"].Value;
                            record.CustomerID = long.Parse(node.Attributes["CustomerID"].Value);
                            record.WJID = node.Attributes["WJID"].Value;
                            record.ID = long.Parse(node.Attributes["ID"].Value);
                            record.WorkSpace = node.Attributes["WorkSpace"].Value;
                            record.GoodsName = node.Attributes["GoodsName"].Value;
                            record.BeginTime = node.Attributes["BeginTime"].Value;
                            record.EndTime = node.Attributes["EndTime"].Value;
                            record.BgeinPhotoID = node.Attributes["BgeinPhotoID"].Value;
                            record.longitude = float.Parse(node.Attributes["longitude"].Value);
                            record.Latitude = float.Parse(node.Attributes["Latitude"].Value);
                            record.Mode = int.Parse(node.Attributes["Mode"].Value);
                            data_item.RequestDatas.records.Add(record);
                        }

                    }
                    if(IsNew)
                    {
                        data_item.Remove = false;
                        SubmitDataNew = data_item;
                    }
                    else
                    {
                        data_item.Remove = true;
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
        SubmitDataNew.Xml.LoadXml("<root></root>");
        SubmitDataNew.NodeParent = SubmitDataNew.Xml.FirstChild;
        SubmitDataNew.RequestDatas = new RecordRequest();
        SubmitDataNew.Remove = false;
    }

    /// <summary>
    /// 添加上传数据表并保存
    /// </summary>
    /// <param name="wj_record"></param>
    /// <param name="dt"></param>
    public void AddSubmit(WJ_Record wj_record, System.DateTime dt)
    {
        XmlElement node_data_submit = SubmitDataNew.Xml.CreateElement("item");
        //node_data_submit.SetAttribute("SeqID", dt.Ticks.ToString());
        node_data_submit.SetAttribute("CustomerID", wj_record.CustomerID.ToString());
        node_data_submit.SetAttribute("WJID", wj_record.WJID);
        node_data_submit.SetAttribute("ID", wj_record.ID.ToString());
        node_data_submit.SetAttribute("WorkSpace", wj_record.WorkSpace);
        node_data_submit.SetAttribute("GoodsName", wj_record.GoodsName);
        node_data_submit.SetAttribute("BeginTime", wj_record.BeginTime);
        node_data_submit.SetAttribute("EndTime", wj_record.EndTime);
        node_data_submit.SetAttribute("BgeinPhotoID", wj_record.BgeinPhotoID);
        node_data_submit.SetAttribute("EndPhotoID", wj_record.EndPhotoID);
        node_data_submit.SetAttribute("longitude", wj_record.longitude.ToString());
        node_data_submit.SetAttribute("Latitude", wj_record.Latitude.ToString());
        node_data_submit.SetAttribute("Mode", wj_record.Mode.ToString());
        SubmitDataNew.NodeParent.AppendChild(node_data_submit);
        SubmitDataNew.Xml.Save(SubmitDataNew.XmlPath);
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
            List<long> records = new List<long>();
            for (int j = 0; j < RecvDatas.Count; j++)
            {
                records.AddRange(RecvDatas[j].records);
            }
            RecvDatas.Clear();
            //=========处理数据===========
            if (SubmitDatas.Count>0)
            {//查询老数据
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
            if (records.Count > 0)
            {//查询新记录
                SubmitDataNew.RemoveData(ref records);
            }

            //=====统计========
            LocalCount = 0;
            for (int k = 0; k < SubmitDatas.Count; k++)
            {
                LocalCount += SubmitDatas[k].RequestDatas.records.Count;
            }
            LocalCount += SubmitDataNew.RequestDatas.records.Count;
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

    public bool Add(WJ_Photo wj_photo, System.DateTime dt, string GoodsName)
    {
        //保存到图片上传表
        AddSubmitItem(wj_photo, dt);

        if (LocalData.Records.Count > 0 && string.IsNullOrEmpty(LocalData.Records.Last().EndPhotoID))
        {
            //=====================================================
            System.TimeSpan ts = System.DateTime.Now.Date - StartTime;
            if (ts.Days >= 1)
            {//超过一天,创建新的本地记录
                WJ_Record last_record = LocalData.Records.Last();
                WJ_Photo last_photo = LocalData.Photos[last_record.BgeinPhotoID];
                last_record.EndPhotoID = wj_photo.PhotoID;
                last_record.EndTime = wj_photo.AtTime;

                //修改老xml文件
                XmlNode last_node = LocalData.record_parent.LastChild;
                last_node.Attributes["EndPhotoID"].Value = last_record.EndPhotoID;
                last_node.Attributes["EndTime"].Value = last_record.EndTime;
                string nodestr = last_node.OuterXml;
                LocalData.record_parent.RemoveChild(last_node);
                XmlNode image = LocalData.photo_parent.LastChild;
                string imgstr = image.OuterXml;
                LocalData.photo_parent.RemoveChild(image);
                LocalData.Xml.Save(LocalData.XmlPath);

                //====创建新xml====
                StartTime = System.DateTime.Now.Date;
                string NowDataXmlPath = string.Format("{0}{1}.xml", LocalDataParentPath, StartTime.ToString("yyyy-MM-dd"));
                NewLocalData(NowDataXmlPath, "<root><photo></photo><record></record></root>");
                LocalData.photo_parent.InnerXml = imgstr;
                //添加图片
                XmlElement node_photo = LocalData.Xml.CreateElement("item");
                node_photo.SetAttribute("CustomerID", wj_photo.CustomerID.ToString());
                node_photo.SetAttribute("WJID", wj_photo.WJID);
                node_photo.SetAttribute("PhotoID", wj_photo.PhotoID);
                node_photo.SetAttribute("PhotoPath", wj_photo.PhotoPath);
                node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
                node_photo.SetAttribute("AtTime", wj_photo.AtTime);
                LocalData.photo_parent.AppendChild(node_photo);

                LocalData.Photos.Add(last_photo.PhotoID, last_photo);
                LocalData.Photos.Add(wj_photo.PhotoID, wj_photo);

                LocalData.record_parent.InnerXml = nodestr;
                LocalData.Records.Add(last_record);
                LocalData.Xml.Save(LocalData.XmlPath);
            }
            else
            {
                //修改本地数据
                UpdateItem(wj_photo);
            }
            //修改上传数据表
            XmlNode node = SubmitDataNew.NodeParent.LastChild;
            node.Attributes["EndPhotoID"].Value = wj_photo.PhotoID;
            node.Attributes["EndTime"].Value = wj_photo.AtTime;
            SubmitDataNew.Xml.Save(SubmitDataNew.XmlPath);
            //添加到上传队列
            WJ_Record wj_record = LocalData.Records.Last();
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

            if (SubmitDataNew.NodeParent.ChildNodes.Count >= 10)
            {//超过10条记录，创建新文件
                //复制到旧数据
                RecordXmlData newdata = SubmitDataNew.Copy();
                newdata.Remove = true;
                SubmitDatas.Add(newdata);
                //创建文件
                NewSubmitFile();
            }
            LocalCount += 1;
            Set.Total += 1;
            SaveTotal();
            OnValueChange();
            return true;
        }
        else
        {
            System.TimeSpan ts = System.DateTime.Now.Date - StartTime;
            if (ts.Days >= 1)
            {//超过一天,创建新的本地记录
                StartTime = System.DateTime.Now.Date;
                string NowDataXmlPath = string.Format("{0}{1}.xml", LocalDataParentPath, StartTime.ToString("yyyy-MM-dd"));
                NewLocalData(NowDataXmlPath, "<root><photo></photo><record></record></root>");
            }

            //添加本地记录
            WJ_Record wj_record = SaveItem(wj_photo, dt, GoodsName);
            //添加上传记录
            AddSubmit(wj_record, dt);
            return false;
        }
    }

    #region Goods
    public string GoodsFileName="goods.xml";
    public string GoodsFilePath;
    public XmlDocument Goods_Xml;
    public XmlNode Goods_parent;
    public List<GoodsResponse.WJ_Goods> Goods;
    //public string GoodsTimeMax = "1000-00-00 00";
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
            //good.time = node.Attributes["time"].Value;
            //if (String.Compare(good.time, GoodsTimeMax)>0)
            //{
            //    GoodsTimeMax = good.time;
            //}
            Goods.Add(good);
        }
        //Debug.Log("Goods Max Time:" + GoodsTimeMax);
    }

    public void AddGoods(GoodsResponse response)
    {
        Goods.Clear();
        Goods_parent.RemoveAll();
        GoodsResponse.WJ_Goods goods_item;
        for (int i = 0; i < response.result.Count; i++)
        {
            goods_item = response.result[i];
            XmlElement node_photo = App.Instance.Data.Goods_Xml.CreateElement("item");
            node_photo.SetAttribute("GoodsID", goods_item.GoodsID);
            node_photo.SetAttribute("GoodsName", goods_item.GoodsName);
            Goods_parent.AppendChild(node_photo);
            Goods.Add(goods_item);
        }
        Goods_Xml.Save(GoodsFilePath);
        GoodsChange = true;
    }

    #endregion

    #region 图片
    public string ImgMinPath;
    public string ImgPath;
    public string ImgXmlPath;
    public List<ImgXmlData> UpLoadImgXmls;
    public ImgXmlData NowImgXml;
    private void InitSubmitImage()
    {
        UpLoadImgXmls = new List<ImgXmlData>();
        ImgMinPath = Application.persistentDataPath + "/img_min/";
        ImgPath = Application.persistentDataPath + "/img/";
        ImgXmlPath = Application.persistentDataPath + "/img_xml/";

        if (!Directory.Exists(ImgPath))
        {
            Directory.CreateDirectory(ImgPath);
        }

        #region 缩略图
        if (!Directory.Exists(ImgMinPath))
        {
            Directory.CreateDirectory(ImgMinPath);
        }
        else
        {
            //只保留最近400张记录
            DirectoryInfo theFolder = new DirectoryInfo(ImgMinPath);
            FileInfo[] fileInfos = theFolder.GetFiles();
            if (fileInfos != null && fileInfos.Length > 0)
            {
                List<FileInfo> ffs = fileInfos.ToList();
                ffs.Sort((s1, s2) => DateTime.Compare(s1.CreationTime, s2.CreationTime));
                for (int i = 0; i < ffs.Count - 400; i++)
                {
                    System.IO.File.Delete(ffs[i].FullName);
                    ffs.RemoveAt(i);
                    i--;
                }
            }
        }
        #endregion
        #region xml
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
                        item.CustomerID =long.Parse(node.Attributes["CustomerID"].Value);
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
        #endregion
        //==========创建最新xml========
        NewXml();
    }
    /// <summary>
    /// 创建一个新文件
    /// </summary>
    private void NewXml()
    {
        NowImgXml = new ImgXmlData();
        NowImgXml.XmlPath = string.Concat(ImgXmlPath, System.DateTime.Now.Ticks.ToString(), ".xml");
        NowImgXml.Xml = new XmlDocument();
        NowImgXml.Xml.LoadXml("<root></root>");
        NowImgXml.NodeParent = NowImgXml.Xml.FirstChild;
        NowImgXml.FileRequestDatas = new Dictionary<string, FileRequest>();
    }
    /// <summary>
    /// 新加一个上传的图片
    /// </summary>
    /// <param name="wj_photo"></param>
    /// <param name="dt"></param>
    private void AddSubmitItem(WJ_Photo wj_photo,System.DateTime dt)
    {
        XmlElement node_photo_submit = NowImgXml.Xml.CreateElement("item");
        node_photo_submit.SetAttribute("CustomerID", wj_photo.CustomerID.ToString());
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
        photo.SeqID = dt.Ticks.ToString();
        NowImgXml.FileRequestDatas.Add(photo.SeqID, photo);

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
        if (UpLoadImgXmls.Count > 0)
        {//查询老记录
            ImgXmlData xml_item = null;
            for (int i = 0; i < UpLoadImgXmls.Count; i++)
            {
                xml_item = UpLoadImgXmls[i];
                if (xml_item.FileRequestDatas.ContainsKey(item.SeqID))
                {//找到
                    xml_item.FileRequestDatas.Remove(item.SeqID);
                    File.Delete(App.Instance.Data.ImgPath + item.PhotoPath);//删除本地的大图，保留小图
                    if (xml_item.FileRequestDatas.Count > 0)
                    {//还有数据，修改xml,并保存===
                        XmlNode node;
                        for (int k = 0; k < xml_item.NodeParent.ChildNodes.Count; k++)
                        {
                            node = xml_item.NodeParent.ChildNodes[k];
                            if (String.Compare(node.Attributes["SeqID"].Value, item.SeqID) == 0)
                            {
                                xml_item.NodeParent.RemoveChild(node);
                                break;
                            }
                        }
                        xml_item.Xml.Save(xml_item.XmlPath);
                    }
                    else
                    {//没有数据，删除该文件
                        File.Delete(xml_item.XmlPath);
                        UpLoadImgXmls.RemoveAt(i);
                    }
                    break;
                }
            }
        }
        else if (NowImgXml.FileRequestDatas.Count > 0)
        {//查询最新记录
            if (NowImgXml.FileRequestDatas.ContainsKey(item.SeqID))
            {//找到
                NowImgXml.FileRequestDatas.Remove(item.SeqID);
                File.Delete(App.Instance.Data.ImgPath + item.PhotoPath);//删除本地的大图，保留小图

                XmlNode node;
                for (int k = 0; k < NowImgXml.NodeParent.ChildNodes.Count; k++)
                {
                    node = NowImgXml.NodeParent.ChildNodes[k];
                    if (String.Compare(node.Attributes["SeqID"].Value, item.SeqID) == 0)
                    {
                        NowImgXml.NodeParent.RemoveChild(node);
                        break;
                    }
                }
                NowImgXml.Xml.Save(NowImgXml.XmlPath);
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
            ValueChange(Set.Total - LocalCount);
        }
    }
}
[System.Serializable]
public class ImgXmlData
{
    public string XmlPath;
    public Dictionary<string, FileRequest> FileRequestDatas;
    public XmlNode NodeParent;
    public XmlDocument Xml;
}
[System.Serializable]
public class RecordXmlData
{
    public string XmlName;
    public string XmlPath;
    public RecordRequest RequestDatas;
    public XmlNode NodeParent;
    public XmlDocument Xml;
    /// <summary>
    /// 是否需要当记录为0的时候删除该记录
    /// </summary>
    public bool Remove = false;
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
            //new_record.SeqID = old_record.SeqID;
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
    public List<long> remove_ids = new List<long>();
    public bool RemoveData(ref List<long> datas)
    {
        for (int i = 0; i < RequestDatas.records.Count; i++)
        {
            for (int j = 0; j < datas.Count; j++)
            {
                if (RequestDatas.records[i].ID== datas[j])
                {
                    remove_ids.Add(datas[j]);
                    datas.RemoveAt(j);
                    RequestDatas.records.RemoveAt(i);
                    i--;
                    break;
                }   
            }
        }


        if (RequestDatas.records.Count<=0&&Remove)
        {
            File.Delete(XmlPath);
            NodeParent = null;
            Xml = null;
            RequestDatas = null;
            remove_ids = null;
            return true;
        }
        else
        {
            if (remove_ids.Count > 0)
            {
                long xml_id = -1;
                for (int i = 0; i < NodeParent.ChildNodes.Count; i++)
                {
                    xml_id = long.Parse(NodeParent.ChildNodes[i].Attributes["ID"].Value);
                    if (remove_ids.Contains(xml_id))
                    {
                        NodeParent.RemoveChild(NodeParent.ChildNodes[i]);
                        i--;
                    }
                }
                Xml.Save(XmlPath);
                remove_ids.Clear();
            }
            return false;
        }
    }
}
[System.Serializable]
public class LocalXmlData
{
    public XmlDocument Xml;
    public XmlNode photo_parent, record_parent;
    public string XmlPath;
    public Dictionary<string, WJ_Photo> Photos;
    public List<WJ_Record> Records;
}


