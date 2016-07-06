//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.UI;
///// <summary>
///// 登录
///// </summary>
//public class UI_Login : UI_Base
//{
//    public Dropdown UserName_Data;
//    public InputField UserPwd_Data;
//    public Button Btn_OK;
//    private List<WJ_Customer> Customers;
//    public override void UI_Start()
//    {
//        Btn_OK.onClick.AddListener(Click);
//        UserName_Data.onValueChanged.AddListener(OnSelect);
//        Customers = App.Instance.data.connection.Table<WJ_Customer>().ToList();
//        if (Customers != null)
//        {
//            for (int i = 0; i < Customers.Count; i++)
//            {
//                Dropdown.OptionData od = new Dropdown.OptionData();
//                od.text = Customers[i].CustomerName;
//                UserName_Data.options.Add(od);
//            }
//            int value = UnityEngine.PlayerPrefs.GetInt("WJ_Customers_ID");
//            if (value >= 0 && value < UserName_Data.options.Count)
//            {
//                UserName_Data.value = value;
//            }
//        }
//    }
//    private void OnSelect(int value)
//    {
//        UnityEngine.PlayerPrefs.SetInt("WJ_Customers_ID", value);
//    }
//    public override void UI_End()
//    {
//        Btn_OK.onClick.RemoveAllListeners();
//        UserName_Data.onValueChanged.RemoveAllListeners();
//    }

//    public void Click()
//    {
//        string name = UserName_Data.captionText.text;
//        string pwd = UserPwd_Data.text;
//        List<WJ_Customer> results = App.Instance.data.connection.Table<WJ_Customer>().Where(x => ((x.CustomerName == name) && (x.Password == pwd))).ToList();
//        if (results.Count > 0)
//        {
//            App.Instance.CurrentUser = results[0];
//            UI_Manager.Instance.Show("UI_Menue");
//        }
//        else
//        {
//            TipsManager.Instance.RunItem("密码错误！");
//        }
//    }


//}

