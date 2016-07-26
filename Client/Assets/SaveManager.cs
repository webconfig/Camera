using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class SaveManager : MonoBehaviour
{
    public Dictionary<string, GameObject> datas = new Dictionary<string, GameObject>();

    public void Save(string key,GameObject obj)
    {
        if (datas.ContainsKey(key))
        {
            datas[key] = obj;
        }
        else
        {
            datas.Add(key, obj);
        }
    }
}
