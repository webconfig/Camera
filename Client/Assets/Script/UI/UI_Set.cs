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
    public Button Btn_OK, Btn_Back, Btn_Login,Btn_Goods_Add, Btn_Volum_Add, Btn_Place_Add;
    public InputField Input_Login_Pwd, Input_WJ_Code,
        Input_Data_Server, Input_Data_Port,
        Input_CustomerID, Input_Password, Input_Password_Local,
        Input_CD, Input_CD1, Input_Day;
    public Toggle Tog_JC, Tog_JCJS, Tog_JC_One, Tog_JC_Two;
    private WJ_Set Set;
    public GameObject Content_Login, Content_Set;
    public override void UI_Init()
    {
        Btn_Back.onClick.AddListener(Back);
        Btn_OK.onClick.AddListener(Btn_OK_Click);
        Btn_Login.onClick.AddListener(Btn_Set_Login);
        Btn_Goods_Add.onClick.AddListener(AddGoods);
        Btn_Volum_Add.onClick.AddListener(AddVolume);
        Btn_Place_Add.onClick.AddListener(Addplace);
        table_goods.RowDeleteEvent += Table_goods_RowDeleteEvent;
        table_Volume.RowDeleteEvent += Table_Volume_RowDeleteEvent;
        table_place.RowDeleteEvent += Table_place_RowDeleteEvent;
        InitTab();
    }

    public override void UI_Start()
    {
        Set = App.Instance.Data.Set;
        Input_Login_Pwd.text = "";
        if (string.IsNullOrEmpty(Set.Password_Local))
        {
            Content_Login.gameObject.SetActive(false);
            Content_Set.gameObject.SetActive(true);
            Btn_OK.gameObject.SetActive(true);
        }
        else
        {
            Content_Login.gameObject.SetActive(true);
            Content_Set.gameObject.SetActive(false);
            Btn_OK.gameObject.SetActive(false);
        }
        Input_WJ_Code.text = Set.WJ_Code;
        Input_Data_Server.text = Set.DataServer;
        Input_Data_Port.text = Set.DataPort.ToString();
        if (Set.CustomerID != -1)
        {
            Input_CustomerID.text = Set.CustomerID.ToString();
        }
        Input_Password.text = Set.Password;
        Input_CD.text = Set.CD.ToString();
        //Input_JSCD.text = (Set.JSCD / 60.0f).ToString();
        Input_Day.text = Set.Day.ToString();
        Input_CD1.text = Set.CD1.ToString();
        Input_Password_Local.text = Set.Password_Local;
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
        ShowGoods();
        ShowVolume();
        Showplace();
    }
    private void Btn_Set_Login()
    {
        if (string.Equals(Input_Login_Pwd.text, App.Instance.Data.Set.Password_Local))
        {
            Btn_OK.gameObject.SetActive(true);
            Content_Login.gameObject.SetActive(false);
            Content_Set.gameObject.SetActive(true);
        }
        else
        {
            TipsManager.Instance.Error("密码错误");
        }
    }
    private void Btn_OK_Click()
    {
        bool relogin = false;
        string WJ_Code, DataServer, DataPort, CustomerIDStr, Password, CDStr, CD1Str, DayStr, Password_Local;
        int data_port, Day;
        long CustomerID;
        float CD, CD1;

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
        Password_Local = Input_Password_Local.text.Trim();
        if (string.IsNullOrEmpty(Password_Local))
        {
            TipsManager.Instance.Error("本地密码为空！");
            return;
        }
        #endregion

        #region 挖机编号
        WJ_Code = Input_WJ_Code.text.Trim();
        if (string.IsNullOrEmpty(WJ_Code))
        {
            TipsManager.Instance.Error("挖机编号为空！");
            return;
        }
        //Place = Input_Place.text.Trim();
        //if (string.IsNullOrEmpty(Place))
        //{
        //    TipsManager.Instance.Error("作业地点为空！");
        //    return;
        //}
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
        Set.WJ_Code = WJ_Code;

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
        Set.Password_Local = Password_Local;
        App.Instance.Data.SaveSet();
        //App.Instance.Develop();
        TipsManager.Instance.Info("设置成功！");
        UI_Manager.Instance.Back();
        if (relogin)
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

    //==============
    public Toggle Tog_UserInfo, Top_Work, Tog_Server, Top_Goods,Tog_Volum, Tog_Place;
    public GameObject Content_UserInfo, Content_Work, Content_Server, Content_Goods, Content_Volum, Content_Place, Content_Current;
    public void InitTab()
    {
        Tog_UserInfo.onValueChanged.AddListener(ShowUserInfo);
        Top_Work.onValueChanged.AddListener(ShowWork);
        Tog_Server.onValueChanged.AddListener(ShowServer);
        Top_Goods.onValueChanged.AddListener(ShowGoods);
        Tog_Volum.onValueChanged.AddListener(ShowVolum);
        Tog_Place.onValueChanged.AddListener(ShowPlace);
    }
    public void ShowUserInfo(bool _select)
    {
        if(Content_Current!=null)
        {
            Content_Current.SetActive(false);
        }
        Content_UserInfo.SetActive(true);
        Content_Current = Content_UserInfo;
    }
    public void ShowWork(bool _select)
    {
        if (Content_Current != null)
        {
            Content_Current.SetActive(false);
        }
        Content_Work.SetActive(true);
        Content_Current = Content_Work;
    }
    public void ShowServer(bool _select)
    {
        if (Content_Current != null)
        {
            Content_Current.SetActive(false);
        }
        Content_Server.SetActive(true);
        Content_Current = Content_Server;
    }
    public void ShowGoods(bool _select)
    {
        if (Content_Current != null)
        {
            Content_Current.SetActive(false);
        }
        Content_Goods.SetActive(true);
        Content_Current = Content_Goods;
    }
    public void ShowVolum(bool _select)
    {
        if (Content_Current != null)
        {
            Content_Current.SetActive(false);
        }
        Content_Volum.SetActive(true);
        Content_Current = Content_Volum;
    }
    public void ShowPlace(bool _select)
    {
        if (Content_Current != null)
        {
            Content_Current.SetActive(false);
        }
        Content_Place.SetActive(true);
        Content_Current = Content_Place;
    }
    //=====物料=====
    public UI_Control_Table table_goods;
    public InputField input_goods;
    private void ShowGoods()
    {
        table_goods.Clear();
        for (int i = 0; i < App.Instance.Data.Goods.Count; i++)
        {
            Dictionary<string, string> data_row = new Dictionary<string, string>();
            data_row.Add("id", App.Instance.Data.Goods[i].GoodsID);
            data_row.Add("name", App.Instance.Data.Goods[i].GoodsName);
            table_goods.AddRow(data_row, null);
        }
    }
    private void AddGoods()
    {
        string str = input_goods.text;
        if (string.IsNullOrEmpty(str))
        {
            TipsManager.Instance.Error("物料类型为空！");
            return;
        }
        App.Instance.Data.AddGoods(str, str);
        Dictionary<string, string> data_row = new Dictionary<string, string>();
        data_row.Add("id", str);
        data_row.Add("name", str);
        table_goods.AddRow(data_row, null);
    }
    private void Table_goods_RowDeleteEvent(UI_Control_Table_Row row)
    {
        App.Instance.Data.RemoveGoods(row.datas["id"]);
        table_goods.RemoveRow(row);
    }
    //=====方量=====
    public UI_Control_Table table_Volume;
    public InputField input_Volume;
    private void ShowVolume()
    {
        table_Volume.Clear();
        for (int i = 0; i < App.Instance.Data.Volume.Count; i++)
        {
            Dictionary<string, string> data_row = new Dictionary<string, string>();
            data_row.Add("id", App.Instance.Data.Volume[i].VolumeID);
            data_row.Add("name", App.Instance.Data.Volume[i].VolumeName);
            table_Volume.AddRow(data_row, null);
        }
    }
    private void AddVolume()
    {
        string str = input_Volume.text;
        if (string.IsNullOrEmpty(str))
        {
            TipsManager.Instance.Error("方量为空！");
            return;
        }
        float k = 0;
        if(!float.TryParse(str,out k))
        {
            TipsManager.Instance.Error("方量必须是数字类型！");
            return;
        }
        App.Instance.Data.AddVolume(str, str);
        Dictionary<string, string> data_row = new Dictionary<string, string>();
        data_row.Add("id", str);
        data_row.Add("name", str);
        table_Volume.AddRow(data_row, null);
    }
    private void Table_Volume_RowDeleteEvent(UI_Control_Table_Row row)
    {
        App.Instance.Data.RemoveVolume(row.datas["id"]);
        table_Volume.RemoveRow(row);
    }
    //=====物料=====
    public UI_Control_Table table_place;
    public InputField input_place;
    private void Showplace()
    {
        table_place.Clear();
        for (int i = 0; i < App.Instance.Data.Place.Count; i++)
        {
            Dictionary<string, string> data_row = new Dictionary<string, string>();
            data_row.Add("value", App.Instance.Data.Place[i]);
            table_place.AddRow(data_row, null);
        }
    }
    private void Addplace()
    {
        string str = input_place.text;
        if (string.IsNullOrEmpty(str))
        {
            TipsManager.Instance.Error("物料类型为空！");
            return;
        }
        App.Instance.Data.AddPlace(str);
        Dictionary<string, string> data_row = new Dictionary<string, string>();
        data_row.Add("value", str);
        table_place.AddRow(data_row, null);
    }
    private void Table_place_RowDeleteEvent(UI_Control_Table_Row row)
    {
        App.Instance.Data.RemovePlace(row.datas["value"]);
        table_place.RemoveRow(row);
    }
}