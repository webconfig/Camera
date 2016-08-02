using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
/// <summary>
/// 设置
/// </summary>
public class UI_Set : UI_Base
{
    public Button Btn_OK, Btn_Back;
    public InputField Input_WJ_Code, Input_Place, Input_Data_Server, Input_Data_Port, Input_CustomerID, Input_Password, Input_CD, Input_CD1, Input_JSCD,Input_Day;
    public Toggle Tog_JC, Tog_JCJS,Tog_JC_One,Tog_JC_Two;
    private WJ_Set Set;
    public override void UI_Init()
    {
        Btn_Back.onClick.AddListener(Back);
        Btn_OK.onClick.AddListener(Btn_OK_Click);
    }
    public override void UI_Start()
    {
       
        Set = App.Instance.Data.Set;
        Input_WJ_Code.text = Set.WJ_Code;
        Input_Place.text = Set.Place;
        Input_Data_Server.text = Set.DataServer;
        Input_Data_Port.text = Set.DataPort.ToString();

        Input_CustomerID.text = Set.CustomerID.ToString();
        Input_Password.text = Set.Password;
        Input_CD.text = Set.CD.ToString();
        Input_JSCD.text = (Set.JSCD / 60.0f).ToString();
        Input_Day.text = Set.Day.ToString();
        Input_CD1.text = Set.CD1.ToString();
        //Tog_Develop.isOn = Set.Develop;
        if (App.Instance.Data.Set.RunType == 0)
        {
            Tog_JC.isOn = true;
            Tog_JCJS.isOn = false;
        }
        else
        {
            Tog_JC.isOn = false;
            Tog_JCJS.isOn = true;
        }

        if (App.Instance.Data.Set.JCType == 1)
        {
            Tog_JC_One.isOn = true;
            Tog_JC_Two.isOn = false;
        }
        else
        {
            Tog_JC_One.isOn = false;
            Tog_JC_Two.isOn = true;
        }
    }

    private void Btn_OK_Click()
    {
        bool relogin = false;
        string WJ_Code,Place, DataServer,DataPort, CustomerIDStr, Password,CDStr, CD1Str, CDJSStr,DayStr;
        int data_port, Day;
        long CustomerID;
        float CD,CD1,CDJS;

        CDStr = Input_CD.text.Trim();
        if (string.IsNullOrEmpty(CDStr))
        {
            TipsManager.Instance.Error("两次计次间隔为空！");
            return;
        }
        if (!float.TryParse(CDStr, out CD))
        {
            TipsManager.Instance.Error("两次计次间隔填写错误！");
            return;
        }
        CD1Str = Input_CD1.text.Trim();
        if (string.IsNullOrEmpty(CD1Str))
        {
            TipsManager.Instance.Error("一次次计次间隔为空！");
            return;
        }
        if (!float.TryParse(CD1Str, out CD1))
        {
            TipsManager.Instance.Error("一次次计次间隔填写错误！");
            return;
        }
        CDJSStr = Input_JSCD.text.Trim();
        if (string.IsNullOrEmpty(CDJSStr))
        {
            TipsManager.Instance.Error("计时自动结束为空！");
            return;
        }
        if (!float.TryParse(CDJSStr, out CDJS))
        {
            TipsManager.Instance.Error("计时自动结束填写错误！");
            return;
        }

        #region 用户名和密码
        CustomerIDStr = Input_CustomerID.text.Trim();
        if (string.IsNullOrEmpty(CustomerIDStr))
        {
            TipsManager.Instance.Error("用户ID为空！");
            return;
        }
        if (!long.TryParse(CustomerIDStr, out CustomerID))
        {
            TipsManager.Instance.Error("用户ID填写错误！");
            return;
        }
        Password = Input_Password.text.Trim();
        if (string.IsNullOrEmpty(Password))
        {
            TipsManager.Instance.Error("密码为空！");
            return;
        }
        #endregion

        #region 挖机编号和作业地点
        WJ_Code = Input_WJ_Code.text.Trim();
        if (string.IsNullOrEmpty(WJ_Code))
        {
            TipsManager.Instance.Error("挖机编号为空！");
            return;
        }
        Place = Input_Place.text.Trim();
        if (string.IsNullOrEmpty(Place))
        {
            TipsManager.Instance.Error("作业地点为空！");
            return;
        }
        #endregion

        #region 服务器
        DataServer = Input_Data_Server.text;
        if (string.IsNullOrEmpty(DataServer))
        {
            TipsManager.Instance.Error("服务器为空！");
            return;
        }
        if (!IPCheck(DataServer))
        {
            TipsManager.Instance.Error("服务器格式错误！");
            return;
        }
        DataPort = Input_Data_Port.text;
        if (string.IsNullOrEmpty(DataPort))
        {
            TipsManager.Instance.Error("服务器端口为空！");
            return;
        }
        if (!int.TryParse(DataPort, out data_port))
        {
            TipsManager.Instance.Error("服务器端口格式错误！");
            return;
        }
        #endregion

        #region 自动删除天数
        DayStr = Input_Day.text;
        if (string.IsNullOrEmpty(DayStr))
        {
            TipsManager.Instance.Error("自动删除天数为空！");
            return;
        }
        if (!int.TryParse(DayStr, out Day))
        {
            TipsManager.Instance.Error("自动删除天数格式错误！");
            return;
        }
        #endregion

        Set.CD = CD;
        Set.JSCD = CDJS*60;
        Set.WJ_Code = WJ_Code;
        Set.Place = Place;

        Set.DataServer = DataServer;
        Set.DataPort = DataPort;
        if (!string.Equals(Set.CustomerID, CustomerID) || !string.Equals(Set.Password, Password))
        {
            relogin = true;
        }

        Set.CustomerID = CustomerID;
        Set.Password = Password;
        Set.Day = Day;
        Set.RunType = Tog_JC.isOn ? 0 : 1;
        Set.JCType = Tog_JC_One.isOn ? 1 : 2;
        Set.CD1 = CD1;
        App.Instance.Data.SaveSet();
        //App.Instance.Develop();
        TipsManager.Instance.Info("设置成功！");
        UI_Manager.Instance.Back();
        if(relogin)
        {
            App.Instance.DataServer.ReLogin();//重新登录
        }
    }
    private void Back()
    {
        UI_Manager.Instance.Back();
    }
    public bool IPCheck(string ip)
    {
        return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
    }
}
