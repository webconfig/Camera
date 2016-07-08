using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_History : UI_Base
{
    public Button Btn_Back;
    public UI_Control_Table table;
    public override void UI_Start()
    {
        Btn_Back.onClick.AddListener(Btn_Back_Click);
        WJ_Record model;
        for (int i = 0; i <App.Instance.Data.Records.Count; i++)
        {
            model = App.Instance.Data.Records[i];
            Dictionary<string, string> data_row = new Dictionary<string, string>();
            data_row.Add("WJID", model.WJID);
            data_row.Add("BeginTime", model.BeginTime);
            data_row.Add("EndTime", model.EndTime);
            data_row.Add("Mode", model.Mode == "0" ? "计次" : "计时");
            if (string.IsNullOrEmpty(model.BgeinPhotoID))
            {
                data_row.Add("BgeinPhotoID", App.Instance.Data.Photos[model.BgeinPhotoID].PhotoMiniPath);
            }
            if (string.IsNullOrEmpty(model.EndPhotoID))
            {
                data_row.Add("EndPhotoID", App.Instance.Data.Photos[model.EndPhotoID].PhotoMiniPath);
            }
            table.AddRow(data_row, null);
        }
    }

    public override void UI_End()
    {
        table.Clear();
        Btn_Back.onClick.RemoveAllListeners();
    }
    private void Btn_Back_Click()
    {
        UI_Manager.Instance.Back();
    }
    
}