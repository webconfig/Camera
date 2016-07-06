using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.IO;

public class FtpClientTest : MonoBehaviour
{
    FTPClient client;
    private void Run(string ftpFolder, string ftpFileName)
    {
        client = getFtpClient();
        client.Connect();
        client.ChDir(ftpFolder);
        client.Put(ftpFileName);
    }

    /// <summary>
    ///得到FTP传输对象
    /// </summary>
    public FTPClient getFtpClient()
    {
        FTPClient ft = new FTPClient();
        ft.RemoteHost = "192.168.2.113";
        ft.RemoteUser = "admin";
        ft.RemotePass = "admin";
        return ft;
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(100, 150, 100, 50), "开始")) { Run(@"\", @"C:\test\1.gif"); }
    }
}
