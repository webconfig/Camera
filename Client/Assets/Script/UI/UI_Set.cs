using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 设置
/// </summary>
public class UI_Set : UI_Base
{
    public Button Btn_OK, Btn_Back;
    public InputField Input_WJ_Code, Input_Place, Input_FTP_Server, Input_FTP_Port,
        Input_Data_Server, Input_Data_Port, Input_CustomerID, Input_Password;
    public Toggle Tog_JC, Tog_JCJS;
    private WJ_Set Set;

    public override void UI_Start()
    {
        Btn_Back.onClick.AddListener(Back);
        Btn_OK.onClick.AddListener(Btn_OK_Click);
        Set = App.Instance.Data.Set;
        Input_WJ_Code.text = Set.WJ_Code;
        Input_Place.text = Set.Place;

        Input_FTP_Server.text = Set.FTPServer;
        Input_FTP_Port.text = Set.FTPPort.ToString();

        Input_Data_Server.text = Set.DataServer;
        Input_Data_Port.text = Set.DataPort.ToString();
        Input_CustomerID.text = Set.CustomerID.ToString();
        Input_Password.text = Set.Password;

        if (App.Instance.Data.Set.RunType == "1")
        {
            Tog_JC.isOn = true;
            Tog_JCJS.isOn = false;
        }
        else
        {
            Tog_JC.isOn = false;
            Tog_JCJS.isOn = true;
        }
    }

    public override void UI_End()
    {
        Btn_Back.onClick.RemoveAllListeners();
        Btn_OK.onClick.RemoveAllListeners();
    }


    private void Btn_OK_Click()
    {
        Set.WJ_Code = Input_WJ_Code.text;
        Set.Place = Input_Place.text;

        Set.FTPServer = Input_FTP_Server.text;
        Set.FTPPort = Input_FTP_Port.text;

        Set.DataServer = Input_Data_Server.text;
        Set.DataPort = Input_Data_Port.text;
        Set.CustomerID = Input_CustomerID.text;
        Set.Password = Input_Password.text;

        App.Instance.Data.SaveSet();
        TipsManager.Instance.RunItem("设置成功！");
        UI_Manager.Instance.Back();
    }
    private void Back()
    {
        UI_Manager.Instance.Back();
    }
}
