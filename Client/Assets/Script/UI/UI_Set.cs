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
    private WJ_Set Data;
    private Toggle Tog_JC,Tog_JCJS;


    public override void UI_Start()
    {
        Btn_Back.onClick.AddListener(Back);
        Btn_OK.onClick.AddListener(Btn_OK_Click);
        Data = App.Instance.data.connection.Table<WJ_Set>().FirstOrDefault();
        if(Data!=null)
        {
            Input_WJ_Code.text = Data.WJ_Code;
            Input_Place.text = Data.Place;

            Input_FTP_Server.text = Data.FTPServer;
            Input_FTP_Port.text = Data.FTPPort.ToString();

            if (Data.DataServer != null)
            {
                Input_Data_Server.text = Data.DataServer;
            }
            //if (Data.DataPort > 0)
            //{
            //    Input_Data_Port.text = Data.DataPort.ToString();
            //}
            //if (Data.CustomerID > 0)
            //{
            //    Input_CustomerID.text = Data.CustomerID.ToString();
            //}
            if (Data.Password != null)
            {
                Input_Password.text = Data.Password;
            }

            //if (Data.RunType == 1)
            //{
            //    Tog_JC.isOn = true;
            //    Tog_JCJS.isOn = false;
            //}
            //else
            //{
            //    Tog_JC.isOn = false;
            //    Tog_JCJS.isOn = true;
            //}
        }
    }

    public override void UI_End()
    {
        Btn_Back.onClick.RemoveAllListeners();
        Btn_OK.onClick.RemoveAllListeners();
    }


    private void Btn_OK_Click()
    {
        if (Data == null)
        {
            //Data = new WJ_Set();
            //Data.SetID = 1;
            //Data.WJ_Code = Input_WJ_Code.text;
            //Data.Place = Input_Place.text;
            //Data.FTPServer=Input_FTP_Server.text;
            //Data.FTPPort=int.Parse(Input_FTP_Port.text);

            //Data.DataServer=Input_Data_Server.text;
            //Data.DataPort=int.Parse(Input_Data_Port.text);

            //Data.CustomerID = long.Parse(Input_CustomerID.text);
            //Data.Password = Input_Password.text;

            //Data.RunType = Tog_JC.isOn ? 1 : 2;


            App.Instance.data.connection.Insert(Data);
            TipsManager.Instance.RunItem("设置成功！");
        }
        else
        {
            if (App.Instance.data.connection.Execute(
                string.Format("UPDATE WJ_Set SET WJ_Code = '{0}', Place = '{1}',FTPServer='{2}',FTPPort={3},DataServer='{4}',DataPort={5},Password='{4}',CustomerID={5},RunType={6} WHERE SetID = 1",
                Input_WJ_Code.text, Input_Place.text, Input_FTP_Server.text, Input_FTP_Port.text, Input_Data_Server.text, Input_Data_Port.text,
                Input_Password.text, Input_CustomerID.text, Tog_JC.isOn ? "1" : "2")) > 0)
            {
                TipsManager.Instance.RunItem("设置成功！");
            }
        }
        UI_Manager.Instance.Back();
    }
    private void Back()
    {
        UI_Manager.Instance.Back();
    }
}
