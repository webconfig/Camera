using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using google.protobuf;
public class AppData
{
    public string Code = "1.0";
    public DateTime StartTime;
    public void Init()
    {
        StartTime = System.DateTime.Now.Date;
        Debug.Log(Application.persistentDataPath);
        InitSet();
        InitDataFile();
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
        Set.CheckCode = SetNode.Attributes["CheckCode"].Value=="1";
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
        Set.Day=int.Parse(SetNode.Attributes["Day"].Value);
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
        SetNode.Attributes["Day"].Value = Set.Day.ToString();
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
    public int LocalCount = 0;
    public string LocalDataParentPath;
    public string ImgMinPath;
    public string ImgPath;

    public List<LocalXmlData> OldDatas;
    /// <summary>
    /// 本地当天记录
    /// </summary>
    public LocalXmlData CurrentData;
    public void InitDataFile()
    {
        //===图片存放目录
        if (!Directory.Exists(ImgPath))
        {
            Directory.CreateDirectory(ImgPath);
        }
        if (!Directory.Exists(ImgMinPath))
        {
            Directory.CreateDirectory(ImgMinPath);
        }

        LocalDataParentPath = Application.persistentDataPath + "/data/";
        #region 处理历史记录
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
                    if (ts.Days >= Set.Day)
                    {//超过很久的数据
                        LocalXmlData lxd = Read(ffs[i].FullName, false, false,true);
                        if (lxd == null)
                        {//该文件的数据都上传完毕，可以删除
                            System.IO.File.Delete(ffs[i].FullName);
                            ffs.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            OldDatas.Add(lxd);
                        }
                    }
                    else if(ts.Days <= 1)
                    {//当天记录
                        CurrentData= Read(ffs[i].FullName, false, false,false);
                    }
                    else
                    {
                        LocalXmlData lxd = Read(ffs[i].FullName, false, false,true);
                        if (lxd != null)
                        {//还有未上传的数据
                            OldDatas.Add(lxd);
                        }
                    }
                }
            }
        }
        #endregion

        #region 获取当天记录
        if (CurrentData==null)
        {
            string NowDataXmlPath = string.Format("{0}{1}.xml", LocalDataParentPath, System.DateTime.Now.ToString("yyyy-MM-dd"));
            //创建新记录
            NewLocalData(NowDataXmlPath, "<root><photo></photo><record></record></root>");
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
        CurrentData = new LocalXmlData();
        CurrentData.XmlPath = path;
        CurrentData.Xml = new XmlDocument();
        CurrentData.Xml.LoadXml(xmldata);
        CurrentData.photo_parent = CurrentData.Xml.SelectSingleNode("root/photo");
        CurrentData.record_parent = CurrentData.Xml.SelectSingleNode("root/record");
        CurrentData.Photos = new Dictionary<string, WJ_Photo_Local>();
        CurrentData.PhotosSubmit = new Dictionary<string, WJ_Photo_Local>();
        CurrentData.Records = new Dictionary<string, WJ_Record_Local>();
        CurrentData.Records_JS = new Dictionary<string, WJ_Record_Local>();
        CurrentData.Records_Submit = new Dictionary<string, WJ_Record_Local>();
    }

    private LocalXmlData Read(string path,bool save,bool read_all,bool IsOld)
    {
        LocalXmlData result = new LocalXmlData();
        result.XmlPath = path;
        result.Xml = new XmlDocument();
        result.Xml.LoadXml(File.ReadAllText(path));
        if (result.Xml.FirstChild.Attributes["over"].Value == "true")
        {//该文件的数据已经上传完成
            return null;
        }
        else
        {
            result.photo_parent = result.Xml.SelectSingleNode("root/photo");
            result.record_parent = result.Xml.SelectSingleNode("root/record");
            result.Photos = new Dictionary<string, WJ_Photo_Local>();
            result.PhotosSubmit = new Dictionary<string, WJ_Photo_Local>();
            result.Records_Submit = new Dictionary<string, WJ_Record_Local>();
            result.Records = new Dictionary<string, WJ_Record_Local>();
            result.Records_JS = new Dictionary<string, WJ_Record_Local>();

            #region 统计未上传的图片
            XmlNodeList nodes = result.photo_parent.SelectNodes("item");
            XmlNode node;
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                if (!read_all)
                {//不需要读取所有数据
                    if (node.Attributes["State"].Value == "1")
                    {//该图片已经上传完成
                        continue;
                    }
                }
                WJ_Photo_Local photo = new WJ_Photo_Local();
                photo.SeqID = node.Attributes["SeqID"].InnerText;
                photo.CustomerID = long.Parse(node.Attributes["CustomerID"].InnerText);
                photo.WJID = node.Attributes["WJID"].InnerText;
                photo.PhotoID = node.Attributes["PhotoID"].InnerText;
                photo.PhotoPath = node.Attributes["PhotoPath"].InnerText;
                photo.AtTime = node.Attributes["AtTime"].InnerText;
                photo.PhotoMiniPath = node.Attributes["PhotoMiniPath"].InnerText;
                photo.State=int.Parse(node.Attributes["State"].InnerText);
                result.Photos.Add(photo.PhotoID, photo);
                result.PhotosSubmit.Add(photo.SeqID, photo);
            }
            #endregion
            #region 统计未上传的数据
            nodes = result.record_parent.SelectNodes("item");
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                if (!read_all)
                {//不需要读取所有数据
                    if (node.Attributes["State"].Value == "1")
                    {//该图片已经上传完成
                        continue;
                    }
                }
                WJ_Record_Local record = new WJ_Record_Local();
                record.SeqID = node.Attributes["SeqID"].InnerText;
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
                record.AddTime = int.Parse(node.Attributes["add_time"].InnerText);
                record.State = int.Parse(node.Attributes["State"].InnerText);
                result.Records_Submit.Add(record.SeqID, record);

                if (record.Mode == 1)
                {
                    result.Records_JS.Add(record.SeqID, record);
                }
                else
                {
                    result.Records.Add(record.SeqID, record);
                }

            }
            #endregion

            if(result.Photos.Count==0&&result.Records_Submit.Count==0)
            {//没有上传的图片
                result.Xml.FirstChild.Attributes["over"].Value = "true";
                if(save)
                {
                    result.Xml.Save(path);
                }
            }
            return result;
        }
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
            try
            {
                bool find = false;
                for (int i = 0; i < OldDatas.Count; i++)
                {
                    for (int k = 0; k < records.Count; k++)
                    {


                        if (OldDatas[i].PhotosSubmit.ContainsKey(wj_photo.SeqID))
                        {
                            XmlNodeList nodes = OldDatas[i].photo_parent.SelectNodes("item");
                            XmlNode node;
                            for (int j = 0; j < nodes.Count; j++)
                            {
                                node = nodes[j];
                                if (string.Equals(node.Attributes["SeqID"].InnerText, wj_photo.SeqID))
                                {
                                    node.Attributes["State"].Value = "1";
                                    break;
                                }
                            }
                            OldDatas[i].Xml.Save(OldDatas[i].XmlPath);
                            find = true;
                            break;
                        }
                    }
                }
                if (!find)
                {
                    if (CurrentData.PhotosSubmit.ContainsKey(wj_photo.SeqID))
                    {
                        XmlNodeList nodes = CurrentData.photo_parent.SelectNodes("item");
                        XmlNode node;
                        for (int j = 0; j < nodes.Count; j++)
                        {
                            node = nodes[j];
                            if (string.Equals(node.Attributes["SeqID"].InnerText, wj_photo.SeqID))
                            {
                                node.Attributes["State"].Value = "1";
                                break;
                            }
                        }
                        CurrentData.Xml.Save(CurrentData.XmlPath);
                    }
                }
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
            catch
            {
                App.Instance.DataServer.AddStr("处理Record数据异常！");
            }

            App.Instance.DataServer.State = ClientStat.SendData;
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
    public void PhotoSubmitOver(WJ_Photo_Local wj_photo)
    {
        bool find = false;
        for (int i = 0; i < OldDatas.Count; i++)
        {
            if (OldDatas[i].PhotosSubmit.ContainsKey(wj_photo.SeqID))
            {
                XmlNodeList nodes = OldDatas[i].photo_parent.SelectNodes("item");
                XmlNode node;
                for (int j = 0; j < nodes.Count; j++)
                {
                    node = nodes[j];
                    if (string.Equals(node.Attributes["SeqID"].InnerText, wj_photo.SeqID))
                    {
                        node.Attributes["State"].Value = "1";
                        break;
                    }
                }
                OldDatas[i].Xml.Save(OldDatas[i].XmlPath);
                find = true;
                break;
            }
        }
        if(!find)
        {
            if (CurrentData.PhotosSubmit.ContainsKey(wj_photo.SeqID))
            {
                XmlNodeList nodes = CurrentData.photo_parent.SelectNodes("item");
                XmlNode node;
                for (int j = 0; j < nodes.Count; j++)
                {
                    node = nodes[j];
                    if (string.Equals(node.Attributes["SeqID"].InnerText, wj_photo.SeqID))
                    {
                        node.Attributes["State"].Value = "1";
                        break;
                    }
                }
                CurrentData.Xml.Save(CurrentData.XmlPath);
            }
        }
    }
    #endregion

    #region 计次
    public bool Add(WJ_Photo_Local wj_photo, DateTime dt, string GoodsName)
    {
        #region 修改计时
        WJ_Record_Local last_record_js = null;
        if (CurrentData.Records_JS.Count > 0) { last_record_js = CurrentData.Records_JS.Values.Last(); }
        if (last_record_js != null && string.IsNullOrEmpty(last_record_js.EndPhotoID))
        {
            #region 修改本地记录
            //修改record
            System.TimeSpan ts = dt - last_record_js.BeginTime_T;
            last_record_js.AddTime = (int)ts.TotalSeconds;
            XmlNodeList local_nodes = CurrentData.record_parent.ChildNodes;
            XmlNode local_node;
            for (int i = local_nodes.Count - 1; i >= 0; i--)
            {
                local_node = local_nodes[i];
                if (string.Equals(local_node.Attributes["SeqID"].InnerText, last_record_js.SeqID))
                {
                    local_node.Attributes["add_time"].Value = last_record_js.AddTime.ToString();
                    break;
                }
            }
            CurrentData.Xml.Save(CurrentData.XmlPath);//保存到硬盘
            #endregion
        }
        #endregion

        WJ_Record_Local last_record = null;
        if (CurrentData.Records.Count > 0) { last_record = CurrentData.Records.Values.Last(); }
        if (last_record == null || !string.IsNullOrEmpty(last_record.EndPhotoID))
        {// 新纪录
            #region 时间判断
            System.TimeSpan ts = System.DateTime.Now.Date - StartTime;
            if (ts.Days >= 1)
            {//超过一天
                //新的本地记录
                string file_name = DateTime.Now.ToString("yyyy-MM-dd");
                string NowDataXmlPath = string.Format("{0}{1}.xml", LocalDataParentPath, file_name);
                NewLocalData(NowDataXmlPath, "<root><photo></photo><record></record></root>");
            }
            #endregion
            #region 添加本地记录
            CurrentData.Photos.Add(wj_photo.PhotoID, wj_photo);//图片集合
            WJ_Record_Local wj_record = new WJ_Record_Local();
            wj_record.SeqID = wj_photo.SeqID;
            wj_record.CustomerID = wj_photo.CustomerID;
            wj_record.WJID = wj_photo.WJID;
            wj_record.ID = dt.Ticks;
            wj_record.WorkSpace = App.Instance.Data.Set.Place;
            wj_record.GoodsName = GoodsName;
            wj_record.BeginTime = wj_photo.AtTime;
            wj_record.EndTime = wj_photo.AtTime;
            //wj_record.EndTime_T = DateTime.MinValue;
            wj_record.BgeinPhotoID = wj_photo.PhotoID;
            wj_record.EndPhotoID = "";
            wj_record.longitude = 0;
            wj_record.Latitude = 0;
            wj_record.Mode = 0;
            CurrentData.Records.Add(wj_record.SeqID, wj_record);//record 集合
            //photo xml
            XmlElement node_photo = CurrentData.Xml.CreateElement("item");
            node_photo.SetAttribute("SeqID", wj_photo.SeqID);
            node_photo.SetAttribute("CustomerID", wj_photo.CustomerID.ToString());
            node_photo.SetAttribute("WJID", wj_photo.WJID);
            node_photo.SetAttribute("PhotoID", wj_photo.PhotoID);
            node_photo.SetAttribute("PhotoPath", wj_photo.PhotoPath);
            node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
            node_photo.SetAttribute("AtTime", wj_photo.AtTime);
            node_photo.SetAttribute("State", "0");//未上传
            CurrentData.photo_parent.AppendChild(node_photo);
            //record xml
            XmlElement node_data = CurrentData.Xml.CreateElement("item");
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
            node_data.SetAttribute("add_time", wj_record.AddTime.ToString());
            node_data.SetAttribute("State", "0");//未上传
            CurrentData.record_parent.AppendChild(node_data);
            CurrentData.Xml.Save(CurrentData.XmlPath);//存储到本地
            #endregion
            return false;
        }
        else
        {
            #region 修改本地记录
            //添加图片
            CurrentData.Photos.Add(wj_photo.PhotoID, wj_photo);
            //photo xml
            XmlElement node_photo = CurrentData.Xml.CreateElement("item");
            node_photo.SetAttribute("CustomerID", wj_photo.CustomerID.ToString());
            node_photo.SetAttribute("WJID", wj_photo.WJID);
            node_photo.SetAttribute("PhotoID", wj_photo.PhotoID);
            node_photo.SetAttribute("PhotoPath", wj_photo.PhotoPath);
            node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
            node_photo.SetAttribute("AtTime", wj_photo.AtTime);
            CurrentData.photo_parent.AppendChild(node_photo);
            //修改record
            last_record.EndPhotoID = wj_photo.PhotoID;
            last_record.EndTime = wj_photo.AtTime;
            XmlNodeList local_nodes = CurrentData.record_parent.ChildNodes;
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
            CurrentData.Xml.Save(CurrentData.XmlPath);//保存到硬盘
            #endregion

            CurrentData.Records_Submit.Add(last_record.SeqID,last_record);//添加到上传列表

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
        return true;
        //#region 时间判断
        //System.TimeSpan ts = System.DateTime.Now.Date - StartTime;
        //if (ts.Days >= 1)
        //{//超过一天
        //    //新的本地记录
        //    StartTime = System.DateTime.Now.Date;
        //    string file_name = StartTime.ToString("yyyy-MM-dd");
        //    string NowDataXmlPath = string.Format("{0}{1}.xml", LocalDataParentPath, file_name);
        //    NewLocalData(NowDataXmlPath, "<root><photo></photo><record></record></root>");
        //    //新的上传文件
        //    string NowSubmitXmlPath = string.Format("{0}{1}.xml", SubmitDataXmlPath, file_name);
        //    RecordXmlData newdata = SubmitDataNew.Copy();
        //    SubmitDatas.Add(newdata);
        //    NewSubmitFile(file_name, NowSubmitXmlPath);
        //}
        //#endregion

        //WJ_Record last_record = null;
        //if (App.Instance.Data.LocalData.Records_JS.Count > 0) { last_record = App.Instance.Data.LocalData.Records_JS.Last(); }
        //if (last_record == null || !string.IsNullOrEmpty(last_record.EndPhotoID))
        //{//新纪录
        //    #region 添加本地记录
        //    LocalData.Photos.Add(wj_photo.PhotoID, wj_photo);//图片集合
        //    WJ_Record wj_record = new WJ_Record();
        //    wj_record.SeqID = dt.Ticks.ToString();
        //    wj_record.CustomerID = wj_photo.CustomerID;
        //    wj_record.WJID = wj_photo.WJID;
        //    wj_record.ID = dt.Ticks;
        //    wj_record.WorkSpace = App.Instance.Data.Set.Place;
        //    wj_record.GoodsName = GoodsName;
        //    wj_record.BgeinPhotoID = wj_photo.PhotoID;
        //    wj_record.Time_T = dt;
        //    wj_record.BeginTime_T = dt;
        //    wj_record.BeginTime = wj_photo.AtTime;
        //    wj_record.EndPhotoID = "";
        //    wj_record.EndTime = wj_photo.AtTime;
        //    wj_record.EndTime_T = DateTime.MinValue;
        //    wj_record.longitude = 0;
        //    wj_record.Latitude = 0;
        //    wj_record.Mode = 1;
        //    LocalData.Records_JS.Add(wj_record);//添加到 Records_JS 集合
        //    //photo xml
        //    XmlElement node_photo = LocalData.Xml.CreateElement("item");
        //    node_photo.SetAttribute("CustomerID", wj_photo.CustomerID.ToString());
        //    node_photo.SetAttribute("WJID", wj_photo.WJID);
        //    node_photo.SetAttribute("PhotoID", wj_photo.PhotoID);
        //    node_photo.SetAttribute("PhotoPath", wj_photo.PhotoPath);
        //    node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
        //    node_photo.SetAttribute("AtTime", wj_photo.AtTime);
        //    LocalData.photo_parent.AppendChild(node_photo);
        //    //record xml
        //    XmlElement node_data = LocalData.Xml.CreateElement("item");
        //    node_data.SetAttribute("SeqID", wj_record.SeqID);
        //    node_data.SetAttribute("CustomerID", wj_record.CustomerID.ToString());
        //    node_data.SetAttribute("WJID", wj_record.WJID);
        //    node_data.SetAttribute("ID", wj_record.ID.ToString());
        //    node_data.SetAttribute("WorkSpace", wj_record.WorkSpace);
        //    node_data.SetAttribute("GoodsName", wj_record.GoodsName);
        //    node_data.SetAttribute("BeginTime", wj_record.BeginTime);
        //    node_data.SetAttribute("EndTime", wj_record.EndTime);
        //    node_data.SetAttribute("BgeinPhotoID", wj_record.BgeinPhotoID);
        //    node_data.SetAttribute("EndPhotoID", wj_record.EndPhotoID);
        //    node_data.SetAttribute("longitude", wj_record.longitude.ToString());
        //    node_data.SetAttribute("Latitude", wj_record.Latitude.ToString()); 
        //    node_data.SetAttribute("Mode", wj_record.Mode.ToString());
        //    node_data.SetAttribute("add_time", wj_record.AddTime.ToString());
        //    LocalData.record_parent.AppendChild(node_data);
        //    LocalData.Xml.Save(LocalData.XmlPath);//存储到本地
        //    #endregion

        //    #region 添加上传记录
        //    XmlElement node_data_submit = SubmitDataNew.Xml.CreateElement("item");
        //    node_data_submit.SetAttribute("SeqID", wj_record.SeqID);
        //    node_data_submit.SetAttribute("CustomerID", wj_record.CustomerID.ToString());
        //    node_data_submit.SetAttribute("WJID", wj_record.WJID);
        //    node_data_submit.SetAttribute("ID", wj_record.ID.ToString());
        //    node_data_submit.SetAttribute("WorkSpace", wj_record.WorkSpace);
        //    node_data_submit.SetAttribute("GoodsName", wj_record.GoodsName);
        //    node_data_submit.SetAttribute("BeginTime", wj_record.BeginTime);
        //    node_data_submit.SetAttribute("EndTime", wj_record.EndTime);
        //    node_data_submit.SetAttribute("BgeinPhotoID", wj_record.BgeinPhotoID);
        //    node_data_submit.SetAttribute("EndPhotoID", wj_record.EndPhotoID);
        //    node_data_submit.SetAttribute("longitude", wj_record.longitude.ToString());
        //    node_data_submit.SetAttribute("Latitude", wj_record.Latitude.ToString());
        //    node_data_submit.SetAttribute("Mode", wj_record.Mode.ToString());
        //    SubmitDataNew.NodeParent.AppendChild(node_data_submit);
        //    SubmitDataNew.Xml.Save(SubmitDataNew.XmlPath);
        //    #endregion

        //    #region 添加图片上传表
        //    XmlElement node_photo_submit = NowImgXml.Xml.CreateElement("item");
        //    node_photo_submit.SetAttribute("CustomerID", wj_photo.CustomerID.ToString());
        //    node_photo_submit.SetAttribute("WJID", wj_photo.WJID);
        //    node_photo_submit.SetAttribute("PhotoID", wj_photo.PhotoID);
        //    node_photo_submit.SetAttribute("PhotoPath", wj_photo.PhotoPath);
        //    node_photo_submit.SetAttribute("AtTime", wj_photo.AtTime);
        //    node_photo_submit.SetAttribute("SeqID", dt.Ticks.ToString());
        //    NowImgXml.NodeParent.AppendChild(node_photo_submit);
        //    NowImgXml.Xml.Save(NowImgXml.XmlPath);

        //    FileRequest photo = new FileRequest();
        //    photo.CustomerID = wj_photo.CustomerID;
        //    photo.WJID = wj_photo.WJID;
        //    photo.PhotoID = wj_photo.PhotoID;
        //    photo.PhotoPath = wj_photo.PhotoPath;
        //    photo.AtTime = wj_photo.AtTime;
        //    photo.SeqID = dt.Ticks.ToString();
        //    NowImgXml.FileRequestDatas.Add(photo.SeqID, photo);

        //    if (NowImgXml.NodeParent.ChildNodes.Count >= 10)
        //    {//超过10条，就创建一个新文件
        //        UpLoadImgXmls.Add(NowImgXml);
        //        NewXml();
        //    }
        //    #endregion

        //    return true;
        //}
        //else
        //{
        //    #region 修改本地记录
        //    //添加图片
        //    LocalData.Photos.Add(wj_photo.PhotoID, wj_photo);
        //    //photo xml
        //    XmlElement node_photo = LocalData.Xml.CreateElement("item");
        //    node_photo.SetAttribute("CustomerID", wj_photo.CustomerID.ToString());
        //    node_photo.SetAttribute("WJID", wj_photo.WJID);
        //    node_photo.SetAttribute("PhotoID", wj_photo.PhotoID);
        //    node_photo.SetAttribute("PhotoPath", wj_photo.PhotoPath);
        //    node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
        //    node_photo.SetAttribute("AtTime", wj_photo.AtTime);
        //    LocalData.photo_parent.AppendChild(node_photo);
        //    //修改record
        //    last_record.EndPhotoID = wj_photo.PhotoID;
        //    last_record.EndTime = wj_photo.AtTime;
        //    last_record.EndTime_T = dt.Date;
        //    XmlNodeList local_nodes = LocalData.record_parent.ChildNodes;
        //    XmlNode local_node;
        //    for (int i = local_nodes.Count - 1; i >= 0; i--)
        //    {
        //        local_node = local_nodes[i];
        //        if (string.Equals(local_node.Attributes["SeqID"].InnerText, last_record.SeqID))
        //        {
        //            local_node.Attributes["EndPhotoID"].Value = last_record.EndPhotoID;
        //            local_node.Attributes["EndTime"].Value = last_record.EndTime;
        //            break;
        //        }
        //    }
        //    LocalData.Xml.Save(LocalData.XmlPath);//保存到硬盘
        //    #endregion

        //    #region 修改上传数据表
        //    XmlNodeList submit_nodes = SubmitDataNew.NodeParent.ChildNodes;
        //    XmlNode submit_node;
        //    for (int i = submit_nodes.Count - 1; i >= 0; i--)
        //    {
        //        submit_node = submit_nodes[i];
        //        if (string.Equals(submit_node.Attributes["SeqID"].InnerText, last_record.SeqID))
        //        {
        //            submit_node.Attributes["EndPhotoID"].Value = last_record.EndPhotoID;
        //            submit_node.Attributes["EndTime"].Value = last_record.EndTime;
        //            break;
        //        }
        //    }
        //    SubmitDataNew.Xml.Save(SubmitDataNew.XmlPath);//保存到硬盘
        //    //添加到上传队列
        //    RecordRequest.WJ_Record_Submit record = new RecordRequest.WJ_Record_Submit();
        //    record.CustomerID = last_record.CustomerID;
        //    record.WJID = last_record.WJID;
        //    record.ID = last_record.ID;
        //    record.WorkSpace = last_record.WorkSpace;
        //    record.GoodsName = last_record.GoodsName;
        //    record.BeginTime = last_record.BeginTime;
        //    record.EndTime = last_record.EndTime;
        //    record.BgeinPhotoID = last_record.BgeinPhotoID;
        //    record.EndPhotoID = last_record.EndPhotoID;
        //    record.longitude = last_record.longitude;
        //    record.Latitude = last_record.Latitude;
        //    record.Mode = 1;
        //    SubmitDataNew.RequestDatas.records.Add(record);
        //    #endregion

        //    #region 添加图片上传表
        //    XmlElement node_photo_submit = NowImgXml.Xml.CreateElement("item");
        //    node_photo_submit.SetAttribute("CustomerID", wj_photo.CustomerID.ToString());
        //    node_photo_submit.SetAttribute("WJID", wj_photo.WJID);
        //    node_photo_submit.SetAttribute("PhotoID", wj_photo.PhotoID);
        //    node_photo_submit.SetAttribute("PhotoPath", wj_photo.PhotoPath);
        //    node_photo_submit.SetAttribute("AtTime", wj_photo.AtTime);
        //    node_photo_submit.SetAttribute("SeqID", dt.Ticks.ToString());
        //    NowImgXml.NodeParent.AppendChild(node_photo_submit);
        //    NowImgXml.Xml.Save(NowImgXml.XmlPath);

        //    FileRequest photo = new FileRequest();
        //    photo.CustomerID = wj_photo.CustomerID;
        //    photo.WJID = wj_photo.WJID;
        //    photo.PhotoID = wj_photo.PhotoID;
        //    photo.PhotoPath = wj_photo.PhotoPath;
        //    photo.AtTime = wj_photo.AtTime;
        //    photo.SeqID = dt.Ticks.ToString();
        //    NowImgXml.FileRequestDatas.Add(photo.SeqID, photo);

        //    if (NowImgXml.NodeParent.ChildNodes.Count >= 10)
        //    {//超过10条，就创建一个新文件
        //        UpLoadImgXmls.Add(NowImgXml);
        //        NewXml();
        //    }
        //    #endregion

        //    return false;
        //}
    }
    public void SaveJs()
    {
        //System.DateTime dt = System.DateTime.Now;
        //WJ_Record last_record  = LocalData.Records_JS.Last();
        //if (!string.IsNullOrEmpty(last_record.EndPhotoID)) { return; }
        //#region 修改本地记录
        ////修改record
        //last_record.EndTime = dt.ToString("yyyy-MM-dd HH:mm:ss");
        //last_record.EndTime_T = dt.Date;
        //XmlNodeList local_nodes = LocalData.record_parent.ChildNodes;
        //XmlNode local_node;
        //for (int i = local_nodes.Count - 1; i >= 0; i--)
        //{
        //    local_node = local_nodes[i];
        //    if (string.Equals(local_node.Attributes["SeqID"].InnerText, last_record.SeqID))
        //    {
        //        local_node.Attributes["EndTime"].Value = last_record.EndTime;
        //        break;
        //    }
        //}
        //LocalData.Xml.Save(LocalData.XmlPath);//保存到硬盘
        //#endregion
        //#region 修改上传数据表
        //XmlNodeList submit_nodes = SubmitDataNew.NodeParent.ChildNodes;
        //XmlNode submit_node;
        //for (int i = submit_nodes.Count - 1; i >= 0; i--)
        //{
        //    submit_node = submit_nodes[i];
        //    if (string.Equals(submit_node.Attributes["SeqID"].InnerText, last_record.SeqID))
        //    {
        //        submit_node.Attributes["EndTime"].Value = last_record.EndTime;
        //        break;
        //    }
        //}
        //SubmitDataNew.Xml.Save(SubmitDataNew.XmlPath);//保存到硬盘
        //#endregion
        //#region 添加到上传队列
        //RecordRequest.WJ_Record_Submit record = new RecordRequest.WJ_Record_Submit();
        //record.CustomerID = last_record.CustomerID;
        //record.WJID = last_record.WJID;
        //record.ID = last_record.ID;
        //record.WorkSpace = last_record.WorkSpace;
        //record.GoodsName = last_record.GoodsName;
        //record.BeginTime = last_record.BeginTime;
        //record.EndTime = last_record.EndTime;
        //record.BgeinPhotoID = last_record.BgeinPhotoID;
        //record.EndPhotoID = last_record.EndPhotoID;
        //record.longitude = last_record.longitude;
        //record.Latitude = last_record.Latitude;
        //record.Mode = 1;
        //SubmitDataNew.RequestDatas.records.Add(record);
        //#endregion
    }
    #endregion

    #region Goods
    public string GoodsFileName="goods.xml";
    public string GoodsFilePath;
    public XmlDocument Goods_Xml;
    public XmlNode Goods_parent;
    public List<GoodsResponse.WJ_Goods> Goods;
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
            Goods.Add(good);
        }
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
public class LocalXmlData
{
    public XmlDocument Xml;
    public XmlNode photo_parent, record_parent;
    public string XmlPath;
    public Dictionary<string, WJ_Photo_Local> Photos;
    public Dictionary<string, WJ_Photo_Local> PhotosSubmit;
    public Dictionary<string, WJ_Record_Local> Records;
    public Dictionary<string, WJ_Record_Local> Records_JS;
    public Dictionary<string, WJ_Record_Local> Records_Submit;
}


