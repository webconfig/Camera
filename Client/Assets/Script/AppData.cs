using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Xml;
using UnityEngine;
public class AppData
{
    public string FilePath;

    #region Set
    public string SetFileName="set.xml";
    public string SetFilePath;
    public XmlDocument Data_Set_Xml;
    public WJ_Set Set;
    private void InitSet()
    {
        Data_Set_Xml = new XmlDocument();
        Set = new WJ_Set();

        SetFilePath = string.Format("{0}/{1}", Application.persistentDataPath, SetFileName);
        if (File.Exists(SetFileName))
        {
            string str = File.ReadAllText(SetFileName);
            Data_Set_Xml.LoadXml(str);
            GetSet();
        }
    }
    public void SaveSet()
    {
        XmlElement node = Data_Set_Xml.SelectSingleNode("root/item") as XmlElement;
        node.SetAttribute("WJ_Code", Set.WJ_Code);
        node.SetAttribute("Place", Set.Place);
        node.SetAttribute("FTPPort", Set.FTPPort);
        node.SetAttribute("DataServer", Set.DataServer);
        node.SetAttribute("DataPort", Set.DataPort);
        node.SetAttribute("CustomerID", Set.CustomerID);
        node.SetAttribute("Password", Set.Password);
        node.SetAttribute("RunType", Set.RunType);
        Data_Set_Xml.Save(SetFilePath);
    }
    public void GetSet()
    {
        XmlNode node = Data_Set_Xml.SelectSingleNode("root/item");
        Set.WJ_Code = node.Attributes["WJ_Code"].Value;
        Set.Place = node.Attributes["Place"].Value;
        Set.FTPServer = node.Attributes["FTPServer"].Value;
        Set.FTPPort = node.Attributes["FTPPort"].Value;
        Set.DataServer = node.Attributes["DataServer"].Value;
        Set.DataPort = node.Attributes["DataPort"].Value;
        Set.CustomerID = node.Attributes["CustomerID"].Value;
        Set.Password = node.Attributes["Password"].Value;
        Set.RunType = node.Attributes["RunType"].Value;
    }
    #endregion

    #region Data
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

        string FilePath_Data = FilePath + "/data/";
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
            }
        }
        //==========最新xml========
        NowDataXmlPath = string.Format("{0}{1}.xml", FilePath_Data, System.DateTime.Now.ToString("yyyy-MM-dd"));
        string str;
        if (File.Exists(NowDataXmlPath))
        {
            str = File.ReadAllText(NowDataXmlPath);
            Data_Submit_Xml.LoadXml(str);
            data_photo_parent = Data_Submit_Xml.SelectSingleNode("root/photo");
            data_record_parent = Data_Submit_Xml.SelectSingleNode("root/record");

            XmlNodeList nodes = data_photo_parent.SelectNodes("/item");
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
                Photos.Add(photo.PhotoID,photo);
            }

            nodes = data_record_parent.SelectNodes("/item");
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
            Data_Submit_Xml.LoadXml(str);
            data_photo_parent = Data_Submit_Xml.SelectSingleNode("root/photo");
            data_record_parent = Data_Submit_Xml.SelectSingleNode("root/record");
        }
    }
    public void AddDataItem(WJ_Photo phot, WJ_Record recode)
    {
       
    }

    public void Update(WJ_Record recode)
    {
        XmlNode node = data_record_parent.LastChild;
        node.Attributes["EndTime"].Value= recode.EndTime;
        node.Attributes["EndPhotoID"].Value = recode.EndPhotoID;
    }
    #endregion

    #region Submit
    public XmlDocument Data_Submit_Xml;
    public XmlNode submit_photo_parent, submit_record_parent;
    public bool HasSubmit;
    public List<string> SubmitDatas;
    public string NowSubmitXml;
    public string FilePath_Submit;
    public void InitSubmitFile()
    {
        Data_Submit_Xml = new XmlDocument();
        FilePath_Submit = FilePath + "/submit/";
        DirectoryInfo theFolder = new DirectoryInfo(FilePath_Submit);
        FileInfo[] fileInfos = theFolder.GetFiles();
        if (fileInfos != null && fileInfos.Length > 0)
        {
            List<FileInfo> ffs = fileInfos.ToList();
            ffs.Sort((s1, s2) => DateTime.Compare(s1.CreationTime, s2.CreationTime));
            HasSubmit= true;
            for (int i = 0; i < ffs.Count; i++)
            {
                SubmitDatas.Add(File.ReadAllText(ffs[i].FullName));
            }
        }
        else
        {
            HasSubmit=false;
        }
        //==========最新xml========
        NowSubmitXml = FilePath_Submit + "new.xml";
        string str;
        if (File.Exists(FilePath_Submit))
        {
            str = File.ReadAllText(FilePath_Submit);
        }
        else
        {
            str = "<root><photo></photo><record></record></root>";
        }
        Data_Submit_Xml.LoadXml(str);
        submit_photo_parent = Data_Submit_Xml.SelectSingleNode("root/photo");
        submit_record_parent = Data_Submit_Xml.SelectSingleNode("root/record");
    }
    //public void AddItem(WJ_Photo_Submit phot,WJ_Record_Submit recode)
    //{
    //    XmlElement node = Data_Submit_Xml.CreateElement("item");
    //    node.SetAttribute("CustomerID", phot.CustomerID);
    //    node.SetAttribute("WJID", phot.WJID);
    //    node.SetAttribute("PhotoID", phot.PhotoID);
    //    node.SetAttribute("PhotoPath", phot.PhotoPath);
    //    node.SetAttribute("AtTime", phot.AtTime);
    //    photo_parent.AppendChild(node);

    //    node = Data_Submit_Xml.CreateElement("item");
    //    node.SetAttribute("SeqID", recode.SeqID);
    //    node.SetAttribute("CustomerID", recode.CustomerID);
    //    node.SetAttribute("WJID", recode.WJID);
    //    node.SetAttribute("ID", recode.ID);
    //    node.SetAttribute("WorkSpace", recode.WorkSpace);
    //    node.SetAttribute("GoodsName", recode.GoodsName);
    //    node.SetAttribute("BeginTime", recode.BeginTime);
    //    node.SetAttribute("EndTime", recode.EndTime);
    //    node.SetAttribute("BgeinPhotoID", recode.BgeinPhotoID);
    //    node.SetAttribute("EndPhotoID", recode.EndPhotoID);
    //    node.SetAttribute("longitude", recode.longitude);
    //    node.SetAttribute("Latitude", recode.Latitude);
    //    node.SetAttribute("Mode", recode.Mode);
    //    record_parent.AppendChild(node);

    //    if(record_parent.ChildNodes.Count>=20)
    //    {//一个xml只记录20条记录
    //        Data_Submit_Xml.Save(System.DateTime.Now.Ticks.ToString() + ".xml");
    //    }
    //    else
    //    {
    //        Data_Submit_Xml.Save(NowXml);
    //    }
        

    //}
    #endregion

    #region Goods
    public string GoodsFileName="goods.xml";
    public string GoodsFilePath;
    public XmlDocument Goods_Xml;
    public List<WJ_Goods> Goods;
    private void InitGoods()
    {
        Data_Set_Xml = new XmlDocument();
        Goods = new List<WJ_Goods>();

        GoodsFilePath = string.Format("{0}/{1}", Application.persistentDataPath, GoodsFileName);
        if (File.Exists(GoodsFilePath))
        {
            string str = File.ReadAllText(GoodsFilePath);
            Data_Set_Xml.LoadXml(str);
            XmlNodeList nodes = Data_Set_Xml.SelectNodes("root/item");
            XmlNode node;
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                WJ_Goods good = new WJ_Goods();
                good.GoodsID = node.Attributes["GoodsID"].Value;
                good.GoodsName = node.Attributes["GoodsName"].Value;
                Goods.Add(good);
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
}