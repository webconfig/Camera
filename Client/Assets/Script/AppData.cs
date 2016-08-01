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
        Set.JCType = int.Parse(SetNode.Attributes["JCType"].Value);
        Set.CD1 = float.Parse(SetNode.Attributes["CD1"].Value);
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
        SetNode.Attributes["JCType"].Value = Set.JCType.ToString();
        SetNode.Attributes["CD1"].Value = Set.CD1.ToString();
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
    /// <summary>
    /// 未上传的数据量
    /// </summary>
    public int LocalCount = 0;
    public string LocalDataParentPath;
    public string ImgMinPath;
    public string ImgPath;
    public List<LocalXmlData> OldDatas;
    /// <summary>
    /// 本地当天记录
    /// </summary>
    public LocalXmlData CurrentData;
    public List<LocalXmlData> HistoryData=new List<LocalXmlData>();
    public void InitDataFile()
    {
        ImgPath = Application.persistentDataPath + "/img/";
        ImgMinPath = Application.persistentDataPath + "/img_min/";
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
        OldDatas = new List<LocalXmlData>();
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
                    TimeSpan ts = DateTime.Now.Subtract(ffs[i].CreationTime);
                    if (ts.Days >= Set.Day)
                    {//超过很久的数据
                        LocalXmlData lxd = Read(ffs[i].FullName, true,true,true);
                        if (lxd.IsOver)
                        {//该文件的数据都上传完毕，可以删除
                            File.Delete(ffs[i].FullName);
                            ffs.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            HistoryData.Add(lxd);
                            OldDatas.Add(lxd);
                        }
                    }
                    else if(ts.Days >1)
                    {
                        LocalXmlData lxd = Read(ffs[i].FullName, true, false,true);
                        HistoryData.Add(lxd);
                        if (!lxd.IsOver)
                        {//还有未上传的数据
                            OldDatas.Add(lxd);
                        }
                    }
                    else
                    {//当天记录
                        CurrentData = Read(ffs[i].FullName, false, false,false);
                        HistoryData.Add(CurrentData);
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
            NewLocalData(NowDataXmlPath, "<root over=''><photo></photo><record></record></root>");
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
        if (CurrentData != null && !CurrentData.IsOver)
        {
            OldDatas.Add(CurrentData);
        }
        CurrentData = new LocalXmlData();
        CurrentData.XmlPath = path;
        CurrentData.Xml = new XmlDocument();
        CurrentData.Xml.LoadXml(xmldata);
        CurrentData.photo_parent = CurrentData.Xml.SelectSingleNode("root/photo");
        CurrentData.record_parent = CurrentData.Xml.SelectSingleNode("root/record");
        CurrentData.Photos = new Dictionary<string, WJ_Photo_Local>();
        CurrentData.PhotosSubmit = new Dictionary<string, WJ_Photo_Local>();
        CurrentData.Records = new Dictionary<long, WJ_Record_Local>();
        CurrentData.Records_JS = new Dictionary<long, WJ_Record_Local>();
        CurrentData.Records_Submit = new Dictionary<long, WJ_Record_Local>();
        CurrentData.AllRecords = new List<WJ_Record_Local>();
        HistoryData.Add(CurrentData);
    }

    private LocalXmlData Read(string path, bool IsOld, bool IsDel, bool IsSave)
    {
        LocalXmlData result = new LocalXmlData();
        result.XmlPath = path;
        result.Xml = new XmlDocument();
        result.Xml.LoadXml(File.ReadAllText(path));
        //是否完成所有数据的上传
        result.IsOver = IsOld && result.Xml.FirstChild.Attributes["over"].Value == "true";
        if (IsDel && result.IsOver)
        {
            return result;
        }
        result.photo_parent = result.Xml.SelectSingleNode("root/photo");
        result.record_parent = result.Xml.SelectSingleNode("root/record");
        result.Photos = new Dictionary<string, WJ_Photo_Local>();
        result.PhotosSubmit = new Dictionary<string, WJ_Photo_Local>();
        result.Records_Submit = new Dictionary<long, WJ_Record_Local>();
        result.Records = new Dictionary<long, WJ_Record_Local>();
        result.Records_JS = new Dictionary<long, WJ_Record_Local>();
        result.AllRecords = new List<WJ_Record_Local>();

        #region 统计未上传的图片
        XmlNodeList nodes = result.photo_parent.SelectNodes("item");
        XmlNode node;
        for (int i = 0; i < nodes.Count; i++)
        {
            node = nodes[i];
            WJ_Photo_Local photo = new WJ_Photo_Local();
            photo.Data = new WJ_Photo();
            photo.SeqID = node.Attributes["SeqID"].InnerText;
            photo.Data.CustomerID = long.Parse(node.Attributes["CustomerID"].InnerText);
            photo.Data.WJID = node.Attributes["WJID"].InnerText;
            photo.Data.PhotoID = node.Attributes["PhotoID"].InnerText;
            photo.Data.PhotoPath = node.Attributes["PhotoPath"].InnerText;
            photo.Data.AtTime = node.Attributes["AtTime"].InnerText;
            photo.PhotoMiniPath = node.Attributes["PhotoMiniPath"].InnerText;
            photo.State = int.Parse(node.Attributes["State"].InnerText);
            result.Photos.Add(photo.Data.PhotoID, photo);
            if (photo.State != 1)
            {
                result.PhotosSubmit.Add(photo.SeqID, photo);
            }
        }
        #endregion
        #region 统计未上传的数据
        nodes = result.record_parent.SelectNodes("item");
        for (int i = 0; i < nodes.Count; i++)
        {
            node = nodes[i];
            WJ_Record_Local record = new WJ_Record_Local();
            record.Data = new WJ_Record();
            record.Data.SeqID = long.Parse(node.Attributes["SeqID"].InnerText);
            record.Data.CustomerID = long.Parse(node.Attributes["CustomerID"].InnerText);
            record.Data.WJID = node.Attributes["WJID"].InnerText;
            record.Data.ID = long.Parse(node.Attributes["ID"].InnerText);
            record.Data.WorkSpace = node.Attributes["WorkSpace"].InnerText;
            record.Data.GoodsName = node.Attributes["GoodsName"].InnerText;

            record.Data.BeginTime = node.Attributes["BeginTime"].InnerText;
            record.Data.EndTime = node.Attributes["EndTime"].InnerText;
            record.Data.BgeinPhotoID = node.Attributes["BgeinPhotoID"].InnerText;
            record.Data.EndPhotoID = node.Attributes["EndPhotoID"].InnerText;
            record.Data.longitude = float.Parse(node.Attributes["longitude"].InnerText);
            record.Data.Latitude = float.Parse(node.Attributes["Latitude"].InnerText);
            record.Data.Mode = int.Parse(node.Attributes["Mode"].InnerText);
            record.AddTime = int.Parse(node.Attributes["add_time"].InnerText);
            record.State = int.Parse(node.Attributes["State"].InnerText);

            result.AllRecords.Insert(0, record);
            if (IsOld)
            {//老数据
                if (record.State == 0)
                {//只添加为上传的数据
                    result.Records_Submit.Add(record.Data.SeqID, record);
                }
            }
            else
            {//当天数据
                if (record.State == 0 && !string.IsNullOrEmpty(record.Data.EndPhotoID))
                {
                    result.Records_Submit.Add(record.Data.SeqID, record);
                }
                if (record.Data.Mode == 1)
                {
                    result.Records_JS.Add(record.Data.SeqID, record);
                }
                else
                {
                    result.Records.Add(record.Data.SeqID, record);
                }
            }
        }
        #endregion

        if (result.PhotosSubmit.Count == 0 && result.Records_Submit.Count == 0)
        {//没有上传的图片
            if (IsSave)
            {
                result.Xml.FirstChild.Attributes["over"].Value = "true";
                result.Xml.Save(path);
            }
        }

        return result;
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
        if (RecvDatas.Count > 0)
        {
            #region 获取所有上传的记录
            List<long> records = new List<long>();
            for (int j = 0; j < RecvDatas.Count; j++)
            {
                records.Add(RecvDatas[j].record_id);
            }
            RecvDatas.Clear();
            #endregion
            try
            {
                #region 处理老数据
                for (int i = 0; i < OldDatas.Count; i++)
                {
                    if(OldDatas[i].DealRecord(records,true))
                    {
                        OldDatas.RemoveAt(i);
                        i--;
                    }
                    if(records.Count==0)
                    {//已经处理完数据
                        break;
                    }
                }
                #endregion

                #region 处理当天数据
                if (records.Count != 0)
                {
                    CurrentData.DealRecord(records, false);
                }
                #endregion

                GetTotal();
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
            if (OldDatas[i].DealPhoto(wj_photo,true,out find))
            {
                OldDatas.RemoveAt(i);
                i--;
            }
            if(find)
            {
                break;
            }
        }
        if(!find)
        {
            CurrentData.DealPhoto(wj_photo, false, out find) ;
        }
    }
    #endregion

    public void GetTotal()
    {
        LocalCount = 0;
        for (int k = 0; k < OldDatas.Count; k++)
        {
            LocalCount += OldDatas[k].Records.Count;
        }
        var enumerator = CurrentData.Records.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if(enumerator.Current.Value.State!=1)
            {
                LocalCount++;
            }
        }
        OnValueChange();
    }

    #region 计次
    public bool Add(WJ_Photo_Local wj_photo, DateTime dt, string GoodsName)
    {
        bool need_save = false;
        #region 修改计时
        WJ_Record_Local last_record_js = null;
        if (CurrentData.Records_JS.Count > 0) { last_record_js = CurrentData.Records_JS.Values.Last(); }
        if (last_record_js != null && string.IsNullOrEmpty(last_record_js.Data.EndPhotoID))
        {
            //修改record
            System.TimeSpan ts = dt - last_record_js.BeginTime_T;
            last_record_js.AddTime = (int)ts.TotalSeconds;
            need_save = true;
        }
        #endregion

        if (App.Instance.Data.Set.JCType == 1)
        {
            #region 修改计时
            if (need_save)
            {
                XmlNodeList local_nodes = CurrentData.record_parent.ChildNodes;
                XmlNode local_node;
                for (int i = local_nodes.Count - 1; i >= 0; i--)
                {
                    local_node = local_nodes[i];
                    if (string.Equals(local_node.Attributes["SeqID"].InnerText, last_record_js.Data.SeqID))
                    {
                        local_node.Attributes["add_time"].Value = last_record_js.AddTime.ToString();
                        break;
                    }
                }
            }
            #endregion
            #region 时间判断
            System.TimeSpan ts = System.DateTime.Now.Date - StartTime;
            if (ts.Days >= 1)
            {//超过一天
                if (CurrentData.PhotosSubmit.Count == 0 && CurrentData.Records_Submit.Count == 0)
                {//没有上传的的数据
                    CurrentData.Xml.FirstChild.Attributes["over"].Value = "true";
                    CurrentData.IsOver = true;
                    need_save = true;
                }
                if (need_save)
                {
                    CurrentData.Xml.Save(CurrentData.XmlPath);//保存到硬盘
                }
                //新的本地记录
                string file_name = DateTime.Now.ToString("yyyy-MM-dd");
                string NowDataXmlPath = string.Format("{0}{1}.xml", LocalDataParentPath, file_name);
                NewLocalData(NowDataXmlPath, "<root over=''><photo></photo><record></record></root>");
            }
            #endregion
            #region 添加本地记录
            CurrentData.Photos.Add(wj_photo.Data.PhotoID, wj_photo);//图片集合
            CurrentData.PhotosSubmit.Add(wj_photo.SeqID, wj_photo);
            WJ_Record_Local wj_record = new WJ_Record_Local();
            wj_record.Data = new WJ_Record();
            wj_record.Data.SeqID = dt.Ticks;
            wj_record.Data.CustomerID = wj_photo.Data.CustomerID;
            wj_record.Data.WJID = wj_photo.Data.WJID;
            wj_record.Data.ID = dt.Ticks;
            wj_record.Data.WorkSpace = App.Instance.Data.Set.Place;
            wj_record.Data.GoodsName = GoodsName;
            wj_record.Data.BeginTime = wj_photo.Data.AtTime;
            wj_record.Data.EndTime = wj_photo.Data.AtTime;
            wj_record.Data.BgeinPhotoID = wj_photo.Data.PhotoID;
            wj_record.Data.EndPhotoID = wj_photo.Data.PhotoID;
            wj_record.Data.longitude = 0;
            wj_record.Data.Latitude = 0;
            wj_record.Data.Mode = 0;
            CurrentData.Records.Add(wj_record.Data.SeqID, wj_record);//record 集合
            CurrentData.AllRecords.Insert(0, wj_record);
            //photo xml
            XmlElement node_photo = CurrentData.Xml.CreateElement("item");
            node_photo.SetAttribute("SeqID", wj_photo.SeqID);
            node_photo.SetAttribute("CustomerID", wj_photo.Data.CustomerID.ToString());
            node_photo.SetAttribute("WJID", wj_photo.Data.WJID);
            node_photo.SetAttribute("PhotoID", wj_photo.Data.PhotoID);
            node_photo.SetAttribute("PhotoPath", wj_photo.Data.PhotoPath);
            node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
            node_photo.SetAttribute("AtTime", wj_photo.Data.AtTime);
            node_photo.SetAttribute("State", "0");//未上传
            CurrentData.photo_parent.AppendChild(node_photo);
            //record xml
            XmlElement node_data = CurrentData.Xml.CreateElement("item");
            node_data.SetAttribute("SeqID", wj_record.Data.SeqID.ToString());
            node_data.SetAttribute("CustomerID", wj_record.Data.CustomerID.ToString());
            node_data.SetAttribute("WJID", wj_record.Data.WJID);
            node_data.SetAttribute("ID", wj_record.Data.ID.ToString());
            node_data.SetAttribute("WorkSpace", wj_record.Data.WorkSpace);
            node_data.SetAttribute("GoodsName", wj_record.Data.GoodsName);
            node_data.SetAttribute("BeginTime", wj_record.Data.BeginTime);
            node_data.SetAttribute("EndTime", wj_record.Data.EndTime);
            node_data.SetAttribute("BgeinPhotoID", wj_record.Data.BgeinPhotoID);
            node_data.SetAttribute("EndPhotoID", wj_record.Data.EndPhotoID);
            node_data.SetAttribute("longitude", wj_record.Data.longitude.ToString());
            node_data.SetAttribute("Latitude", wj_record.Data.Latitude.ToString());
            node_data.SetAttribute("Mode", wj_record.Data.Mode.ToString());
            node_data.SetAttribute("add_time", wj_record.AddTime.ToString());
            node_data.SetAttribute("State", "0");//未上传
            CurrentData.record_parent.AppendChild(node_data);
            CurrentData.Xml.Save(CurrentData.XmlPath);//存储到本地
            #endregion

            CurrentData.Records_Submit.Add(wj_record.Data.SeqID, wj_record);//添加到上传列表
            LocalCount += 1;
            Set.Total += 1;
            SaveTotal();
            OnValueChange();
            return true;
        }
        else
        {
            WJ_Record_Local last_record = null;
            if (CurrentData.Records.Count > 0) { last_record = CurrentData.Records.Values.Last(); }
            if (last_record == null || !string.IsNullOrEmpty(last_record.Data.EndPhotoID))
            {// 新纪录
                #region 修改计时
                if (need_save)
                {
                    XmlNodeList local_nodes = CurrentData.record_parent.ChildNodes;
                    XmlNode local_node;
                    for (int i = local_nodes.Count - 1; i >= 0; i--)
                    {
                        local_node = local_nodes[i];
                        if (string.Equals(local_node.Attributes["SeqID"].InnerText, last_record_js.Data.SeqID))
                        {
                            local_node.Attributes["add_time"].Value = last_record_js.AddTime.ToString();
                            break;
                        }
                    }
                }
                #endregion
                #region 时间判断
                System.TimeSpan ts = System.DateTime.Now.Date - StartTime;
                if (ts.Days >= 1)
                {//超过一天
                    if (CurrentData.PhotosSubmit.Count == 0 && CurrentData.Records_Submit.Count == 0)
                    {//没有上传的的数据
                        CurrentData.Xml.FirstChild.Attributes["over"].Value = "true";
                        CurrentData.IsOver = true;
                        need_save = true;
                    }
                    if (need_save)
                    {
                        CurrentData.Xml.Save(CurrentData.XmlPath);//保存到硬盘
                    }
                    //新的本地记录
                    string file_name = DateTime.Now.ToString("yyyy-MM-dd");
                    string NowDataXmlPath = string.Format("{0}{1}.xml", LocalDataParentPath, file_name);
                    NewLocalData(NowDataXmlPath, "<root over=''><photo></photo><record></record></root>");
                }
                #endregion
                #region 添加本地记录
                CurrentData.Photos.Add(wj_photo.Data.PhotoID, wj_photo);//图片集合
                CurrentData.PhotosSubmit.Add(wj_photo.SeqID, wj_photo);
                WJ_Record_Local wj_record = new WJ_Record_Local();
                wj_record.Data = new WJ_Record();
                wj_record.Data.SeqID = dt.Ticks;
                wj_record.Data.CustomerID = wj_photo.Data.CustomerID;
                wj_record.Data.WJID = wj_photo.Data.WJID;
                wj_record.Data.ID = dt.Ticks;
                wj_record.Data.WorkSpace = App.Instance.Data.Set.Place;
                wj_record.Data.GoodsName = GoodsName;
                wj_record.Data.BeginTime = wj_photo.Data.AtTime;
                wj_record.Data.EndTime = wj_photo.Data.AtTime;
                //wj_record.EndTime_T = DateTime.MinValue;
                wj_record.Data.BgeinPhotoID = wj_photo.Data.PhotoID;
                wj_record.Data.EndPhotoID = "";
                wj_record.Data.longitude = 0;
                wj_record.Data.Latitude = 0;
                wj_record.Data.Mode = 0;
                CurrentData.Records.Add(wj_record.Data.SeqID, wj_record);//record 集合
                CurrentData.AllRecords.Insert(0, wj_record);
                //photo xml
                XmlElement node_photo = CurrentData.Xml.CreateElement("item");
                node_photo.SetAttribute("SeqID", wj_photo.SeqID);
                node_photo.SetAttribute("CustomerID", wj_photo.Data.CustomerID.ToString());
                node_photo.SetAttribute("WJID", wj_photo.Data.WJID);
                node_photo.SetAttribute("PhotoID", wj_photo.Data.PhotoID);
                node_photo.SetAttribute("PhotoPath", wj_photo.Data.PhotoPath);
                node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
                node_photo.SetAttribute("AtTime", wj_photo.Data.AtTime);
                node_photo.SetAttribute("State", "0");//未上传
                CurrentData.photo_parent.AppendChild(node_photo);
                //record xml
                XmlElement node_data = CurrentData.Xml.CreateElement("item");
                node_data.SetAttribute("SeqID", wj_record.Data.SeqID.ToString());
                node_data.SetAttribute("CustomerID", wj_record.Data.CustomerID.ToString());
                node_data.SetAttribute("WJID", wj_record.Data.WJID);
                node_data.SetAttribute("ID", wj_record.Data.ID.ToString());
                node_data.SetAttribute("WorkSpace", wj_record.Data.WorkSpace);
                node_data.SetAttribute("GoodsName", wj_record.Data.GoodsName);
                node_data.SetAttribute("BeginTime", wj_record.Data.BeginTime);
                node_data.SetAttribute("EndTime", wj_record.Data.EndTime);
                node_data.SetAttribute("BgeinPhotoID", wj_record.Data.BgeinPhotoID);
                node_data.SetAttribute("EndPhotoID", wj_record.Data.EndPhotoID);
                node_data.SetAttribute("longitude", wj_record.Data.longitude.ToString());
                node_data.SetAttribute("Latitude", wj_record.Data.Latitude.ToString());
                node_data.SetAttribute("Mode", wj_record.Data.Mode.ToString());
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
                CurrentData.PhotosSubmit.Add(wj_photo.SeqID, wj_photo);
                CurrentData.Photos.Add(wj_photo.Data.PhotoID, wj_photo);
                //photo xml
                XmlElement node_photo = CurrentData.Xml.CreateElement("item");
                node_photo.SetAttribute("SeqID", wj_photo.SeqID);
                node_photo.SetAttribute("CustomerID", wj_photo.Data.CustomerID.ToString());
                node_photo.SetAttribute("WJID", wj_photo.Data.WJID);
                node_photo.SetAttribute("PhotoID", wj_photo.Data.PhotoID);
                node_photo.SetAttribute("PhotoPath", wj_photo.Data.PhotoPath);
                node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
                node_photo.SetAttribute("AtTime", wj_photo.Data.AtTime);
                node_photo.SetAttribute("State", "0");//未上传
                CurrentData.photo_parent.AppendChild(node_photo);
                //修改record
                last_record.Data.EndPhotoID = wj_photo.Data.PhotoID;
                last_record.Data.EndTime = wj_photo.Data.AtTime;
                string record_SeqID = last_record.Data.SeqID.ToString();
                //===便利xml==
                XmlNodeList local_nodes = CurrentData.record_parent.ChildNodes;
                XmlNode local_node;
                string SeqIDStr;
                bool save_self = false, save_js = false;
                if (!need_save) { save_js = true; }
                for (int i = local_nodes.Count - 1; i >= 0; i--)
                {
                    local_node = local_nodes[i];
                    SeqIDStr = local_node.Attributes["SeqID"].InnerText;
                    if (!save_self)
                    {
                        if (string.Equals(SeqIDStr, record_SeqID))
                        {
                            local_node.Attributes["EndPhotoID"].Value = last_record.Data.EndPhotoID;
                            local_node.Attributes["EndTime"].Value = last_record.Data.EndTime;
                            save_self = true;
                        }
                    }
                    if (!save_js)
                    {
                        if (string.Equals(SeqIDStr, record_SeqID))
                        {
                            local_node.Attributes["add_time"].Value = last_record_js.AddTime.ToString();
                            save_js = true;
                        }
                    }
                    if (save_js && save_js)
                    {//都完成跳出
                        break;
                    }
                }
                CurrentData.Xml.Save(CurrentData.XmlPath);//保存到硬盘
                #endregion

                CurrentData.Records_Submit.Add(last_record.Data.SeqID, last_record);//添加到上传列表
                LocalCount += 1;
                Set.Total += 1;
                SaveTotal();
                OnValueChange();
                return true;
            }
        }
    }
    #endregion

    #region 计时
    public bool Add_JS(WJ_Photo_Local wj_photo, System.DateTime dt, string GoodsName)
    {
        #region 时间判断
        TimeSpan ts = System.DateTime.Now.Date - StartTime;
        if (ts.Days >= 1)
        {//超过一天

            if (CurrentData.PhotosSubmit.Count == 0 && CurrentData.Records_Submit.Count == 0)
            {//没有上传的的数据
                CurrentData.Xml.FirstChild.Attributes["over"].Value = "true";
                CurrentData.IsOver = true;
                CurrentData.Xml.Save(CurrentData.XmlPath);//存储到本地
            }


            //新的本地记录
            StartTime = System.DateTime.Now.Date;
            string file_name = StartTime.ToString("yyyy-MM-dd");
            string NowDataXmlPath = string.Format("{0}{1}.xml", LocalDataParentPath, file_name);
            NewLocalData(NowDataXmlPath, "<root over=''><photo></photo><record></record></root>");
        }
        #endregion

        WJ_Record_Local last_record = null;
        if (App.Instance.Data.CurrentData.Records_JS.Count > 0) { last_record = App.Instance.Data.CurrentData.Records_JS.Values.Last(); }
        if (last_record == null || !string.IsNullOrEmpty(last_record.Data.EndPhotoID))
        {//新纪录
            #region 添加本地记录
            CurrentData.PhotosSubmit.Add(wj_photo.SeqID, wj_photo);//上传列表
            CurrentData.Photos.Add(wj_photo.Data.PhotoID, wj_photo);//图片集合
            WJ_Record_Local wj_record = new WJ_Record_Local();
            wj_record.Data = new WJ_Record();
            wj_record.Data.SeqID = dt.Ticks;
            wj_record.Data.CustomerID = wj_photo.Data.CustomerID;
            wj_record.Data.WJID = wj_photo.Data.WJID;
            wj_record.Data.ID = dt.Ticks;
            wj_record.Data.WorkSpace = App.Instance.Data.Set.Place;
            wj_record.Data.GoodsName = GoodsName;
            wj_record.Data.BgeinPhotoID = wj_photo.Data.PhotoID;
            wj_record.BeginTime_T = dt;
            wj_record.StartTime_T = dt;
            wj_record.Data.BeginTime = wj_photo.Data.AtTime;
            wj_record.Data.EndPhotoID = "";
            wj_record.Data.EndTime = wj_photo.Data.AtTime;
            wj_record.Data.longitude = 0;
            wj_record.Data.Latitude = 0;
            wj_record.Data.Mode = 1;
            CurrentData.Records_JS.Add(wj_record.Data.SeqID, wj_record);//添加到 Records_JS 集合
            CurrentData.AllRecords.Insert(0, wj_record);
            //photo xml
            XmlElement node_photo = CurrentData.Xml.CreateElement("item");
            node_photo.SetAttribute("SeqID", wj_photo.SeqID);
            node_photo.SetAttribute("CustomerID", wj_photo.Data.CustomerID.ToString());
            node_photo.SetAttribute("WJID", wj_photo.Data.WJID);
            node_photo.SetAttribute("PhotoID", wj_photo.Data.PhotoID);
            node_photo.SetAttribute("PhotoPath", wj_photo.Data.PhotoPath);
            node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
            node_photo.SetAttribute("AtTime", wj_photo.Data.AtTime);
            node_photo.SetAttribute("State", "0");//未上传
            CurrentData.photo_parent.AppendChild(node_photo);
            //record xml
            XmlElement node_data = CurrentData.Xml.CreateElement("item");
            node_data.SetAttribute("SeqID", wj_record.Data.SeqID.ToString());
            node_data.SetAttribute("CustomerID", wj_record.Data.CustomerID.ToString());
            node_data.SetAttribute("WJID", wj_record.Data.WJID);
            node_data.SetAttribute("ID", wj_record.Data.ID.ToString());
            node_data.SetAttribute("WorkSpace", wj_record.Data.WorkSpace);
            node_data.SetAttribute("GoodsName", wj_record.Data.GoodsName);
            node_data.SetAttribute("BeginTime", wj_record.Data.BeginTime);
            node_data.SetAttribute("EndTime", wj_record.Data.EndTime);
            node_data.SetAttribute("BgeinPhotoID", wj_record.Data.BgeinPhotoID);
            node_data.SetAttribute("EndPhotoID", wj_record.Data.EndPhotoID);
            node_data.SetAttribute("longitude", wj_record.Data.longitude.ToString());
            node_data.SetAttribute("Latitude", wj_record.Data.Latitude.ToString());
            node_data.SetAttribute("Mode", wj_record.Data.Mode.ToString());
            node_data.SetAttribute("add_time", wj_record.AddTime.ToString());
            node_data.SetAttribute("State", "0");//未上传
            CurrentData.record_parent.AppendChild(node_data);
            CurrentData.Xml.Save(CurrentData.XmlPath);//存储到本地
            #endregion
            return true;
        }
        else
        {
            #region 修改本地记录
            //添加图片
            CurrentData.PhotosSubmit.Add(wj_photo.SeqID, wj_photo);//上传列表
            CurrentData.Photos.Add(wj_photo.Data.PhotoID, wj_photo);//图片集合
            //photo xml
            XmlElement node_photo = CurrentData.Xml.CreateElement("item");
            node_photo.SetAttribute("SeqID", wj_photo.SeqID);
            node_photo.SetAttribute("CustomerID", wj_photo.Data.CustomerID.ToString());
            node_photo.SetAttribute("WJID", wj_photo.Data.WJID);
            node_photo.SetAttribute("PhotoID", wj_photo.Data.PhotoID);
            node_photo.SetAttribute("PhotoPath", wj_photo.Data.PhotoPath);
            node_photo.SetAttribute("PhotoMiniPath", wj_photo.PhotoMiniPath);
            node_photo.SetAttribute("AtTime", wj_photo.Data.AtTime);
            node_photo.SetAttribute("State", "0");//未上传
            CurrentData.photo_parent.AppendChild(node_photo);
            //修改record
            last_record.Data.EndPhotoID = wj_photo.Data.PhotoID;
            last_record.Data.EndTime = wj_photo.Data.AtTime;
            string record_SeqID = last_record.Data.SeqID.ToString();
            XmlNodeList local_nodes = CurrentData.record_parent.ChildNodes;
            XmlNode local_node;
            for (int i = local_nodes.Count - 1; i >= 0; i--)
            {
                local_node = local_nodes[i];
                if (string.Equals(local_node.Attributes["SeqID"].InnerText, record_SeqID))
                {
                    local_node.Attributes["EndPhotoID"].Value = last_record.Data.EndPhotoID;
                    local_node.Attributes["EndTime"].Value = last_record.Data.EndTime;
                    break;
                }
            }
            CurrentData.Xml.Save(CurrentData.XmlPath);//保存到硬盘
            #endregion

            CurrentData.Records_Submit.Add(last_record.Data.SeqID, last_record);//添加到上传列表
            return false;
        }
    }
    public void SaveJs()
    {
        System.DateTime dt = System.DateTime.Now;
        WJ_Record_Local last_record = CurrentData.Records_JS.Values.Last();
        string record_SeqID = last_record.Data.SeqID.ToString();
        if (!string.IsNullOrEmpty(last_record.Data.EndPhotoID)) { return; }
        #region 修改本地记录
        //修改record
        last_record.Data.EndTime = dt.ToString("yyyy-MM-dd HH:mm:ss");
        XmlNodeList local_nodes = CurrentData.record_parent.ChildNodes;
        XmlNode local_node;
        for (int i = local_nodes.Count - 1; i >= 0; i--)
        {
            local_node = local_nodes[i];
            if (string.Equals(local_node.Attributes["SeqID"].InnerText, record_SeqID))
            {
                local_node.Attributes["EndTime"].Value = last_record.Data.EndTime;
                break;
            }
        }
        CurrentData.Xml.Save(CurrentData.XmlPath);//保存到硬盘
        #endregion
        if (CurrentData.Records_Submit.ContainsKey(last_record.Data.SeqID))
        {
            CurrentData.Records_Submit[last_record.Data.SeqID] = last_record;
        }
        else
        {
            CurrentData.Records_Submit.Add(last_record.Data.SeqID, last_record);//添加到上传数据
        }
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
    public Dictionary<long, WJ_Record_Local> Records;
    public Dictionary<long, WJ_Record_Local> Records_JS;
    /// <summary>
    /// 所有图片
    /// </summary>
    public Dictionary<string, WJ_Photo_Local> Photos;
    /// <summary>
    /// 上传的图片
    /// </summary>
    public Dictionary<string, WJ_Photo_Local> PhotosSubmit;
    /// <summary>
    /// 上传的数据
    /// </summary>
    public Dictionary<long, WJ_Record_Local> Records_Submit;
    /// <summary>
    /// 所有数据
    /// </summary>
    public List<WJ_Record_Local> AllRecords;
    public bool DealRecord(List<long> ids, bool IsOld)
    {
        bool find = false;
        long id;
        string id_str;
        for (int k = 0; k < ids.Count; k++)
        {
            id = ids[k];
            id_str = id.ToString();
            if (Records_Submit.ContainsKey(id))
            {
                Records_Submit.Remove(id);
                if (IsOld)
                {
                    if (Records.ContainsKey(id))
                    {
                        Records.Remove(id);
                    }
                }
                else
                {
                    if (Records.ContainsKey(id))
                    {
                        Records[id].State = 1;
                    }
                    else if (Records_JS.ContainsKey(id))
                    {
                        if (string.IsNullOrEmpty(Records_JS[id].Data.EndPhotoID))
                        {//是临时计时数据
                            ids.RemoveAt(k);
                            k--;
                            continue;
                        }
                        else
                        {
                            Records_JS[id].State = 1;
                        }
                    }
                }

                XmlNodeList nodes = record_parent.SelectNodes("item");
                XmlNode node;
                for (int j = 0; j < nodes.Count; j++)
                {
                    node = nodes[j];
                    if (!string.IsNullOrEmpty(node.Attributes["EndPhotoID"].Value))
                    {
                        if (string.Equals(node.Attributes["SeqID"].Value, id_str))
                        {
                            node.Attributes["State"].Value = "1";
                            break;
                        }
                    }
                }
                find = true;
                ids.RemoveAt(k);
                k--;
            }
        }


        if (Records_Submit.Count == 0 && PhotosSubmit.Count == 0)
        {
            if(IsOld)
            {
                Xml.FirstChild.Attributes["over"].Value = "true";
                Xml.Save(XmlPath);
            }
            else
            {
                if (find)
                {
                    Xml.Save(XmlPath);
                }
            }
            return true;
        }
        else
        {
            if (find)
            {
                Xml.Save(XmlPath);
            }
            return false;
        }
    }
    public bool DealPhoto(WJ_Photo_Local wj_photo, bool IsOld,out bool find)
    {
        if (PhotosSubmit.ContainsKey(wj_photo.SeqID))
        {
            PhotosSubmit.Remove(wj_photo.SeqID);
            XmlNodeList nodes = photo_parent.SelectNodes("item");
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
            Xml.Save(XmlPath);
            find = true;


            if (Records_Submit.Count == 0 && PhotosSubmit.Count == 0)
            {//没有上传记录
                return true;
            }

        }
        find= false;
        return false;

    }
    /// <summary>
    /// 是否完成上传所有记录
    /// </summary>
    public bool IsOver = false;
}


