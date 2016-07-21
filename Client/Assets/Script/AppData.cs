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
        InitSet();
        InitDataFile();
        InitSubmitImage();
        InitSubmitFile();
        InitGoods();
    }

    #region 配置
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
        Set.JSCD = float.Parse(SetNode.Attributes["JSCD"].Value);
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
        SetNode.Attributes["JSCD"].Value = Set.JSCD.ToString();
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

    #region 本地数据
    public string LocalDataParentPath;
    /// <summary>
    /// 本地当天记录
    /// </summary>
    public LocalXmlData LocalData;
    public void InitDataFile()
    {
        LocalDataParentPath = Application.persistentDataPath + "/data/";
        #region 删除历史记录
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
        #endregion

        #region 获取上一天记录
        List<WJ_Record> TodayDatas = null;
        Dictionary<string, WJ_Photo> TodayPhoto = null;
        string NowDataXmlPath = string.Format("{0}{1}.xml", LocalDataParentPath, System.DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"));
        if (File.Exists(NowDataXmlPath))
        {
            TodayDatas = new List<WJ_Record>();
            TodayPhoto = new Dictionary<string, WJ_Photo>();
            XmlDocument Xml = new XmlDocument();
            Xml.LoadXml(File.ReadAllText(NowDataXmlPath));

            #region 图片
            XmlNodeList nodes = Xml.SelectNodes("root/photo/item");
            XmlNode node;
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                string str = node.Attributes["AtTime"].InnerText;
                System.TimeSpan sp = DateTime.Now.Date - Convert.ToDateTime(str).Date;
                if (sp.TotalDays < 1)
                {
                    WJ_Photo photo = new WJ_Photo();
                    photo.CustomerID = long.Parse(node.Attributes["CustomerID"].InnerText);
                    photo.WJID = node.Attributes["WJID"].InnerText;
                    photo.PhotoID = node.Attributes["PhotoID"].InnerText;
                    photo.PhotoPath = node.Attributes["PhotoPath"].InnerText;
                    photo.PhotoMiniPath = node.Attributes["PhotoMiniPath"].InnerText;
                    TodayPhoto.Add(photo.PhotoID, photo);
                }
            }
            #endregion

            #region record
            nodes = Xml.SelectNodes("root/record/item");
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                string str = node.Attributes["EndTime"].InnerText;
                System.TimeSpan sp = DateTime.Now.Date - Convert.ToDateTime(str).Date;
                if (sp.TotalDays < 1)
                {
                    WJ_Record record = new WJ_Record();
                    record.CustomerID = long.Parse(node.Attributes["CustomerID"].InnerText);
                    record.WJID = node.Attributes["WJID"].InnerText;
                    record.ID = long.Parse(node.Attributes["ID"].InnerText);
                    record.WorkSpace = node.Attributes["WorkSpace"].InnerText;
                    record.GoodsName = node.Attributes["GoodsName"].InnerText;

                    record.BeginTime = node.Attributes["BeginTime"].InnerText;
                    record.EndTime = node.Attributes["EndTime"].InnerText;
                    record.BgeinPhotoID = node.Attributes["BgeinPhotoID"].InnerText;
                    record.EndPhotoID = node.Attributes["EndPhotoID"].InnerText;
                    record.longitude = float.Parse(node.Attributes["longitude"].InnerText);
                    record.Latitude = float.Parse(node.Attributes["Latitude"].InnerText);
                    record.Mode = int.Parse(node.Attributes["Mode"].InnerText);
                    TodayDatas.Add(record);
                }
            }
            #endregion
        }
        #endregion

        #region 获取当天记录
        NowDataXmlPath = string.Format("{0}{1}.xml", LocalDataParentPath, System.DateTime.Now.ToString("yyyy-MM-dd"));
        if (File.Exists(NowDataXmlPath))
        {
            NewLocalData(NowDataXmlPath, File.ReadAllText(NowDataXmlPath));
            #region 存入前天的今天记录
            if (TodayDatas != null && TodayDatas.Count > 0)
            {
                for (int i = 0; i < TodayDatas.Count; i++)
                {
                    if(TodayDatas[i].Mode==0)
                    {
                        LocalData.Records.Add(TodayDatas[i]);
                    }
                    else
                    {
                        LocalData.Records_JS.Add(TodayDatas[i]);
                    }
                }
            }
            if (TodayPhoto != null && TodayDatas.Count > 0)
            {
                var enumerator = TodayPhoto.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    LocalData.Photos.Add(enumerator.Current.Key, enumerator.Current.Value);
                }
                TodayPhoto.Clear();
            }
            #endregion

            XmlNodeList nodes = LocalData.photo_parent.SelectNodes("item");
            XmlNode node;
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                WJ_Photo photo = new WJ_Photo();
                photo.CustomerID = long.Parse(node.Attributes["CustomerID"].InnerText);
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
                record.ID = long.Parse(node.Attributes["ID"].InnerText);
                record.WorkSpace = node.Attributes["WorkSpace"].InnerText;
                record.GoodsName = node.Attributes["GoodsName"].InnerText;

                record.BeginTime = node.Attributes["BeginTime"].InnerText;
                record.EndTime = node.Attributes["EndTime"].InnerText;
                record.BgeinPhotoID = node.Attributes["BgeinPhotoID"].InnerText;
                record.EndPhotoID = node.Attributes["EndPhotoID"].InnerText;
                record.longitude = float.Parse(node.Attributes["longitude"].InnerText);
                record.Latitude = float.Parse(node.Attributes["Latitude"].InnerText);
                record.Mode = int.Parse(node.Attributes["Mode"].InnerText);
                if (record.Mode == 1)
                {
                    LocalData.Records_JS.Add(record);
                }
                else
                {
                    LocalData.Records.Add(record);
                }
            }
        }
        else
        {//创建新记录
            NewLocalData(NowDataXmlPath, "<root><photo></photo><record></record></root>");
            #region 存入前天的今天记录
            if (TodayDatas != null && TodayDatas.Count > 0)
            {
                for (int i = 0; i < TodayDatas.Count; i++)
                {
                    if (TodayDatas[i].Mode == 0)
                    {
                        LocalData.Records.Add(TodayDatas[i]);
                    }
                    else
                    {
                        LocalData.Records_JS.Add(TodayDatas[i]);
                    }
                }
            }
            if (TodayPhoto != null && TodayDatas.Count > 0)
            {
                var enumerator = TodayPhoto.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    LocalData.Photos.Add(enumerator.Current.Key, enumerator.Current.Value);
                }
                TodayPhoto.Clear();
            }
            #endregion
        }
        #endregion
    }
    /// <summary>
    /// 新的本地记录
    /// </summary>
    /// <param name="path"></param>
    /// <param name="xmldata"></param>
    private void NewLocalData(string path, string xmldata)
    {
        List<WJ_Record> TodayDatas = null;
        Dictionary<string, WJ_Photo> TodayPhoto = null;
        if (LocalData != null)
        {//获取以前记录里面的当天记录
            TodayDatas = new List<WJ_Record>();
            TodayPhoto = new Dictionary<string, WJ_Photo>();
            WJ_Record item;
            for (int i = LocalData.Records.Count-1; i >=0 ; i--)
            {
                item = LocalData.Records[i];
                TimeSpan ts = System.DateTime.Now.Date - item.EndTime_T;
                if (ts.TotalDays < 1)
                {//是现在的当天记录
                    TodayDatas.Add(item);
                    TodayPhoto.Add(item.BgeinPhotoID, LocalData.Photos[item.BgeinPhotoID]);
                    TodayPhoto.Add(item.EndPhotoID, LocalData.Photos[item.EndPhotoID]);
                }
                else
                {
                    break;
                }
            }
        }


        LocalData = new LocalXmlData();
        LocalData.XmlPath = path;
        LocalData.Xml = new XmlDocument();
        LocalData.Xml.LoadXml(xmldata);
        LocalData.photo_parent = LocalData.Xml.SelectSingleNode("root/photo");
        LocalData.record_parent = LocalData.Xml.SelectSingleNode("root/record");
        LocalData.Photos = new Dictionary<string, WJ_Photo>();
        LocalData.Records = new List<WJ_Record>();
        LocalData.Records_JS = new List<WJ_Record>();

        if (TodayPhoto != null)
        {
            var enumerator = TodayPhoto.GetEnumerator();
            while (enumerator.MoveNext())
            {
                LocalData.Photos.Add(enumerator.Current.Key, enumerator.Current.Value);
            }
            TodayPhoto.Clear();
        }
        if (TodayDatas != null && TodayDatas.Count > 0)
        {
            for (int i = 0; i < TodayDatas.Count; i++)
            {
                if (TodayDatas[i].Mode == 0)
                {
                    LocalData.Records.Add(TodayDatas[i]);
                }
                else
                {
                    LocalData.Records_JS.Add(TodayDatas[i]);
                }
            }
        }
    }
    #endregion

    #region 上传数据
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
                    RecordXmlData data_item = NewSubmitFile(File.ReadAllText(ffs[i].FullName),ffs[i].FullName);
                    SubmitDatas.Add(data_item);
                    LocalCount += data_item.RequestDatas.records.Count;
                }
                System.TimeSpan ts = System.DateTime.Now.Date - ffs.Last().CreationTime.Date;
                if(ts.TotalDays<1)
                {//是当天记录
                    SubmitDataNew = SubmitDatas.Last();
                    SubmitDatas.RemoveAt(SubmitDatas.Count - 1);
                }

            }
        }
        if(SubmitDataNew==null)
        {
            string NowDataXmlPath = string.Format("{0}{1}.xml", SubmitDataXmlPath, System.DateTime.Now.ToString("yyyy-MM-dd"));
            SubmitDataNew = NewSubmitFile(@"<root></root>", NowDataXmlPath);
        }
    }
    private RecordXmlData NewSubmitFile(string xml,string fullpath)
    {
        RecordXmlData data_item = new RecordXmlData();
        data_item.RequestDatas = new RecordRequest();
        data_item.XmlPath = fullpath;
        data_item.Xml = new XmlDocument();
        data_item.Xml.LoadXml(xml);
        data_item.NodeParent = data_item.Xml.FirstChild;
        data_item.RequestDatas = new RecordRequest();
        XmlNode node;
        int num = data_item.NodeParent.ChildNodes.Count;
        for (int j = 0; j < num; j++)
        {
            node = data_item.NodeParent.ChildNodes[j];
            RecordRequest.WJ_Record_Submit record = new RecordRequest.WJ_Record_Submit();
            record.EndPhotoID = node.Attributes["EndPhotoID"].Value;
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
        return data_item;
    }
    #endregion

    #region 处理完成上传的记录
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
            #region 获取所有上传的记录
            List<long> records = new List<long>();
            for (int j = 0; j < RecvDatas.Count; j++)
            {
                records.AddRange(RecvDatas[j].records);
            }
            RecvDatas.Clear();
            #endregion

            #region 处理老数据
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
            #endregion

            #region 处理当天记录
            if (records.Count > 0)
            {
                SubmitDataNew.RemoveNowData(ref records);
            }
            #endregion

            #region  统计
            LocalCount = 0;
            for (int k = 0; k < SubmitDatas.Count; k++)
            {
                LocalCount += SubmitDatas[k].RequestDatas.records.Count;
            }
            LocalCount += SubmitDataNew.RequestDatas.records.Count;
            OnValueChange();
            #endregion
        }
        #region 货物类型改变
        if (GoodsChange)
        {
            GoodsChange = false;
            if (GoodsChangeEvent != null)
            {
                GoodsChangeEvent();
            }
        }
        #endregion
    }
    #endregion

    #region 计次
    public bool Add(WJ_Photo wj_photo, System.DateTime dt, string GoodsName)
    {
        WJ_Record last_record = null;
        if (LocalData.Records.Count > 0) { last_record = LocalData.Records.Last(); }
        if (last_record == null || !string.IsNullOrEmpty(last_record.EndPhotoID))
        {// 新纪录
            #region 时间判断
            System.TimeSpan ts = System.DateTime.Now.Date - StartTime;
            if (ts.Days >= 1)
            {//超过一天
                //新的本地记录
                StartTime = System.DateTime.Now.Date;
                string file_name = StartTime.ToString("yyyy-MM-dd");
                string NowDataXmlPath = string.Format("{0}{1}.xml", LocalDataParentPath, file_name);
                NewLocalData(NowDataXmlPath, "<root><photo></photo><record></record></root>");
                //新的上传文件
                string NowSubmitXmlPath = string.Format("{0}{1}.xml", SubmitDataXmlPath, file_name);
                RecordXmlData newdata = SubmitDataNew.Copy();
                SubmitDatas.Add(newdata);
                NewSubmitFile(file_name, NowSubmitXmlPath);
            }
            #endregion

            #region 添加本地记录
            LocalData.Photos.Add(wj_photo.PhotoID, wj_photo);//图片集合
            WJ_Record wj_record = new WJ_Record();
            wj_record.SeqID = dt.Ticks.ToString();
            wj_record.CustomerID = wj_photo.CustomerID;
            wj_record.WJID = wj_photo.WJID;
            wj_record.ID = dt.Ticks;
            wj_record.WorkSpace = App.Instance.Data.Set.Place;
            wj_record.GoodsName = GoodsName;
            wj_record.BeginTime = wj_photo.AtTime;
            wj_record.EndTime = "";
            wj_record.EndTime_T = DateTime.MinValue;
            wj_record.BgeinPhotoID = wj_photo.PhotoID;
            wj_record.EndPhotoID = "";
            wj_record.longitude = 0;
            wj_record.Latitude = 0;
            wj_record.Mode = 0;
            LocalData.Records.Add(wj_record);//record 集合
            //photo xml
            XmlElement node_photo = LocalData.Xml.CreateElement("item");
            node_photo.SetAttribute("CustomerID", wj_photo.CustomerID.ToString());
            node_photo.SetAttribute("WJID", wj_photo.WJID);
            node_photo.SetAttribute("PhotoID", wj_photo.PhotoID);
            node_photo.SetAttribute("PhotoPath", wj_photo.PhotoPath);
            node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
            node_photo.SetAttribute("AtTime", wj_photo.AtTime);
            LocalData.photo_parent.AppendChild(node_photo);
            //record xml
            XmlElement node_data = LocalData.Xml.CreateElement("item");
            node_data.SetAttribute("SeqID", wj_record.SeqID);
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
            LocalData.Xml.Save(LocalData.XmlPath);//存储到本地
            #endregion

            #region 添加上传记录
            XmlElement node_data_submit = SubmitDataNew.Xml.CreateElement("item");
            node_data_submit.SetAttribute("SeqID", wj_record.SeqID);
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
            #endregion

            #region 添加图片上传表
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

            if (NowImgXml.NodeParent.ChildNodes.Count >= 10)
            {//超过10条，就创建一个新文件
                UpLoadImgXmls.Add(NowImgXml);
                NewXml();
            }
            #endregion
            return false;
        }
        else
        {
            #region 修改本地记录
            //添加图片
            LocalData.Photos.Add(wj_photo.PhotoID, wj_photo);
            //photo xml
            XmlElement node_photo = LocalData.Xml.CreateElement("item");
            node_photo.SetAttribute("CustomerID", wj_photo.CustomerID.ToString());
            node_photo.SetAttribute("WJID", wj_photo.WJID);
            node_photo.SetAttribute("PhotoID", wj_photo.PhotoID);
            node_photo.SetAttribute("PhotoPath", wj_photo.PhotoPath);
            node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
            node_photo.SetAttribute("AtTime", wj_photo.AtTime);
            LocalData.photo_parent.AppendChild(node_photo);
            //修改record
            last_record.EndPhotoID = wj_photo.PhotoID;
            last_record.EndTime = wj_photo.AtTime;
            last_record.EndTime_T = dt.Date;
            XmlNodeList local_nodes = LocalData.record_parent.ChildNodes;
            XmlNode local_node;
            for (int i = local_nodes.Count - 1; i >= 0; i--)
            {
                local_node = local_nodes[i];
                if (string.Equals(local_node.Attributes["SeqID"].InnerText, last_record.SeqID))
                {
                    local_node.Attributes["EndPhotoID"].Value = last_record.EndPhotoID;
                    local_node.Attributes["EndTime"].Value = last_record.EndTime;
                    break;
                }
            }
            LocalData.Xml.Save(LocalData.XmlPath);//保存到硬盘
            #endregion

            #region 修改上传数据表
            XmlNodeList submit_nodes = SubmitDataNew.NodeParent.ChildNodes;
            XmlNode submit_node;
            for (int i = submit_nodes.Count - 1; i >= 0; i--)
            {
                submit_node = submit_nodes[i];
                if (string.Equals(submit_node.Attributes["SeqID"].InnerText, last_record.SeqID))
                {
                    submit_node.Attributes["EndPhotoID"].Value = last_record.EndPhotoID;
                    submit_node.Attributes["EndTime"].Value = last_record.EndTime;
                    break;
                }
            }
            SubmitDataNew.Xml.Save(SubmitDataNew.XmlPath);//保存到硬盘
            //添加到上传队列
            RecordRequest.WJ_Record_Submit record = new RecordRequest.WJ_Record_Submit();
            record.CustomerID = last_record.CustomerID;
            record.WJID = last_record.WJID;
            record.ID = last_record.ID;
            record.WorkSpace = last_record.WorkSpace;
            record.GoodsName = last_record.GoodsName;
            record.BeginTime = last_record.BeginTime;
            record.EndTime = last_record.EndTime;
            record.BgeinPhotoID = last_record.BgeinPhotoID;
            record.EndPhotoID = last_record.EndPhotoID;
            record.longitude = last_record.longitude;
            record.Latitude = last_record.Latitude;
            record.Mode = 0;
            SubmitDataNew.RequestDatas.records.Add(record);
            #endregion

            #region 添加图片上传表
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

            if (NowImgXml.NodeParent.ChildNodes.Count >= 10)
            {//超过10条，就创建一个新文件
                UpLoadImgXmls.Add(NowImgXml);
                NewXml();
            }
            #endregion

            LocalCount += 1;
            Set.Total += 1;
            SaveTotal();
            OnValueChange();
            return true;
        }
    }
    #endregion

    #region 计时
    public bool Add_JS(WJ_Photo wj_photo, System.DateTime dt, string GoodsName)
    {
        #region 时间判断
        System.TimeSpan ts = System.DateTime.Now.Date - StartTime;
        if (ts.Days >= 1)
        {//超过一天
            //新的本地记录
            StartTime = System.DateTime.Now.Date;
            string file_name = StartTime.ToString("yyyy-MM-dd");
            string NowDataXmlPath = string.Format("{0}{1}.xml", LocalDataParentPath, file_name);
            NewLocalData(NowDataXmlPath, "<root><photo></photo><record></record></root>");
            //新的上传文件
            string NowSubmitXmlPath = string.Format("{0}{1}.xml", SubmitDataXmlPath, file_name);
            RecordXmlData newdata = SubmitDataNew.Copy();
            SubmitDatas.Add(newdata);
            NewSubmitFile(file_name, NowSubmitXmlPath);
        }
        #endregion

        WJ_Record last_record = null;
        if (App.Instance.Data.LocalData.Records_JS.Count > 0) { last_record = App.Instance.Data.LocalData.Records_JS.Last(); }
        if (last_record == null || !string.IsNullOrEmpty(last_record.EndPhotoID))
        {//新纪录
            #region 添加本地记录
            LocalData.Photos.Add(wj_photo.PhotoID, wj_photo);//图片集合
            WJ_Record wj_record = new WJ_Record();
            wj_record.SeqID = dt.Ticks.ToString();
            wj_record.CustomerID = wj_photo.CustomerID;
            wj_record.WJID = wj_photo.WJID;
            wj_record.ID = dt.Ticks;
            wj_record.WorkSpace = App.Instance.Data.Set.Place;
            wj_record.GoodsName = GoodsName;
            wj_record.BgeinPhotoID = wj_photo.PhotoID;
            wj_record.Time_T = dt;
            wj_record.BeginTime_T = dt;
            wj_record.BeginTime = wj_photo.AtTime;
            wj_record.EndPhotoID = "";
            wj_record.EndTime = "";
            wj_record.EndTime_T = DateTime.MinValue;
            wj_record.longitude = 0;
            wj_record.Latitude = 0;
            wj_record.Mode = 1;
            LocalData.Records_JS.Add(wj_record);//添加到 Records_JS 集合
            //photo xml
            XmlElement node_photo = LocalData.Xml.CreateElement("item");
            node_photo.SetAttribute("CustomerID", wj_photo.CustomerID.ToString());
            node_photo.SetAttribute("WJID", wj_photo.WJID);
            node_photo.SetAttribute("PhotoID", wj_photo.PhotoID);
            node_photo.SetAttribute("PhotoPath", wj_photo.PhotoPath);
            node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
            node_photo.SetAttribute("AtTime", wj_photo.AtTime);
            LocalData.photo_parent.AppendChild(node_photo);
            //record xml
            XmlElement node_data = LocalData.Xml.CreateElement("item");
            node_data.SetAttribute("SeqID", wj_record.SeqID);
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
            LocalData.Xml.Save(LocalData.XmlPath);//存储到本地
            #endregion

            #region 添加上传记录
            XmlElement node_data_submit = SubmitDataNew.Xml.CreateElement("item");
            node_data_submit.SetAttribute("SeqID", wj_record.SeqID);
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
            #endregion

            #region 添加图片上传表
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

            if (NowImgXml.NodeParent.ChildNodes.Count >= 10)
            {//超过10条，就创建一个新文件
                UpLoadImgXmls.Add(NowImgXml);
                NewXml();
            }
            #endregion

            return true;
        }
        else
        {
            #region 修改本地记录
            //添加图片
            LocalData.Photos.Add(wj_photo.PhotoID, wj_photo);
            //photo xml
            XmlElement node_photo = LocalData.Xml.CreateElement("item");
            node_photo.SetAttribute("CustomerID", wj_photo.CustomerID.ToString());
            node_photo.SetAttribute("WJID", wj_photo.WJID);
            node_photo.SetAttribute("PhotoID", wj_photo.PhotoID);
            node_photo.SetAttribute("PhotoPath", wj_photo.PhotoPath);
            node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
            node_photo.SetAttribute("AtTime", wj_photo.AtTime);
            LocalData.photo_parent.AppendChild(node_photo);
            //修改record
            last_record.EndPhotoID = wj_photo.PhotoID;
            last_record.EndTime = wj_photo.AtTime;
            last_record.EndTime_T = dt.Date;
            XmlNodeList local_nodes = LocalData.record_parent.ChildNodes;
            XmlNode local_node;
            for (int i = local_nodes.Count - 1; i >= 0; i--)
            {
                local_node = local_nodes[i];
                if (string.Equals(local_node.Attributes["SeqID"].InnerText, last_record.SeqID))
                {
                    local_node.Attributes["EndPhotoID"].Value = last_record.EndPhotoID;
                    local_node.Attributes["EndTime"].Value = last_record.EndTime;
                    break;
                }
            }
            LocalData.Xml.Save(LocalData.XmlPath);//保存到硬盘
            #endregion

            #region 修改上传数据表
            XmlNodeList submit_nodes = SubmitDataNew.NodeParent.ChildNodes;
            XmlNode submit_node;
            for (int i = submit_nodes.Count - 1; i >= 0; i--)
            {
                submit_node = submit_nodes[i];
                if (string.Equals(submit_node.Attributes["SeqID"].InnerText, last_record.SeqID))
                {
                    submit_node.Attributes["EndPhotoID"].Value = last_record.EndPhotoID;
                    submit_node.Attributes["EndTime"].Value = last_record.EndTime;
                    break;
                }
            }
            SubmitDataNew.Xml.Save(SubmitDataNew.XmlPath);//保存到硬盘
            //添加到上传队列
            RecordRequest.WJ_Record_Submit record = new RecordRequest.WJ_Record_Submit();
            record.CustomerID = last_record.CustomerID;
            record.WJID = last_record.WJID;
            record.ID = last_record.ID;
            record.WorkSpace = last_record.WorkSpace;
            record.GoodsName = last_record.GoodsName;
            record.BeginTime = last_record.BeginTime;
            record.EndTime = last_record.EndTime;
            record.BgeinPhotoID = last_record.BgeinPhotoID;
            record.EndPhotoID = last_record.EndPhotoID;
            record.longitude = last_record.longitude;
            record.Latitude = last_record.Latitude;
            record.Mode = 1;
            SubmitDataNew.RequestDatas.records.Add(record);
            #endregion

            #region 添加图片上传表
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

            if (NowImgXml.NodeParent.ChildNodes.Count >= 10)
            {//超过10条，就创建一个新文件
                UpLoadImgXmls.Add(NowImgXml);
                NewXml();
            }
            #endregion

            return false;
        }
    }
    public void SaveJs()
    {
        System.DateTime dt = System.DateTime.Now;
        WJ_Record last_record  = LocalData.Records_JS.Last();
        if (!string.IsNullOrEmpty(last_record.EndPhotoID)) { return; }
        #region 修改本地记录
        //修改record
        last_record.EndTime = dt.ToString("yyyy-MM-dd HH:mm:ss");
        last_record.EndTime_T = dt.Date;
        XmlNodeList local_nodes = LocalData.record_parent.ChildNodes;
        XmlNode local_node;
        for (int i = local_nodes.Count - 1; i >= 0; i--)
        {
            local_node = local_nodes[i];
            if (string.Equals(local_node.Attributes["SeqID"].InnerText, last_record.SeqID))
            {
                local_node.Attributes["EndTime"].Value = last_record.EndTime;
                break;
            }
        }
        LocalData.Xml.Save(LocalData.XmlPath);//保存到硬盘
        #endregion
        #region 修改上传数据表
        XmlNodeList submit_nodes = SubmitDataNew.NodeParent.ChildNodes;
        XmlNode submit_node;
        for (int i = submit_nodes.Count - 1; i >= 0; i--)
        {
            submit_node = submit_nodes[i];
            if (string.Equals(submit_node.Attributes["SeqID"].InnerText, last_record.SeqID))
            {
                submit_node.Attributes["EndTime"].Value = last_record.EndTime;
                break;
            }
        }
        SubmitDataNew.Xml.Save(SubmitDataNew.XmlPath);//保存到硬盘
        #endregion
        #region 添加到上传队列
        RecordRequest.WJ_Record_Submit record = new RecordRequest.WJ_Record_Submit();
        record.CustomerID = last_record.CustomerID;
        record.WJID = last_record.WJID;
        record.ID = last_record.ID;
        record.WorkSpace = last_record.WorkSpace;
        record.GoodsName = last_record.GoodsName;
        record.BeginTime = last_record.BeginTime;
        record.EndTime = last_record.EndTime;
        record.BgeinPhotoID = last_record.BgeinPhotoID;
        record.EndPhotoID = last_record.EndPhotoID;
        record.longitude = last_record.longitude;
        record.Latitude = last_record.Latitude;
        record.Mode = 1;
        SubmitDataNew.RequestDatas.records.Add(record);
        #endregion
    }
    #endregion

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
        if (RequestDatas.records.Count<=0)
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
    public void RemoveNowData(ref List<long> datas)
    {
        for (int i = 0; i < RequestDatas.records.Count; i++)
        {
            for (int j = 0; j < datas.Count; j++)
            {
                if (RequestDatas.records[i].ID == datas[j])
                {
                    if (RequestDatas.records[i].Mode == 0)
                    {//只删除计时记录
                        remove_ids.Add(datas[j]);
                    }
                    datas.RemoveAt(j);
                    RequestDatas.records.RemoveAt(i);
                    i--;
                    break;
                }
            }
        }
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
    public List<WJ_Record> Records_JS;
}


