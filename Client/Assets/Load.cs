using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Xml;

public class Load : MonoBehaviour
{
    public XmlDocument xml;
    public XmlNode xml_parent;
    public string xml_path;
    public string xml_name = "app.xml";
    public Dictionary<string, string> datas = new Dictionary<string, string>();
    private void Init()
    {

        xml = new XmlDocument();
        //Set = new WJ_Set();

        xml_path = string.Format("{0}/{1}", Application.persistentDataPath, xml_name);
        if (!File.Exists(xml_path))
        {
            string StreamFilePath;
#if UNITY_EDITOR
            StreamFilePath = string.Format(@"Assets/StreamingAssets/{0}", xml_path);
            File.Copy(StreamFilePath, xml_path);
#else
#if UNITY_ANDROID 
            var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + xml_name);
            while (!loadDb.isDone) { }
            File.WriteAllBytes(xml_path, loadDb.bytes);
#endif
#endif
        }
        xml.LoadXml(File.ReadAllText(xml_path));
        xml_parent = xml.FirstChild;
        XmlNode node;
        XmlNodeList node_list = xml_parent.ChildNodes;
        for (int i = 0; i < node_list.Count; i++)
        {
            node = node_list[i];
            datas.Add(node.Attributes["key"].Value, node.Attributes["value"].Value);
        }
    }
}
