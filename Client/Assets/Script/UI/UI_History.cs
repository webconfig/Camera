using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_History : UI_Base
{
    public Button Btn_Back;
    public UI_Control_Table table;
    public Text TxtTotal;
    public override void UI_Start()
    {
        Btn_Back.onClick.AddListener(Btn_Back_Click);
        ShowData(App.Instance.Data.CurrentData);
        table.ToEnd();
    }
    private void ShowData(LocalXmlData xd)
    {
        WJ_Record_Local model;
        TxtTotal.text = string.Format("总共{0}条记录", xd.Records.Count);
        for (int i = 0; i < xd.Records.Count; i++)
        {
            model = xd.Records[i];
            Dictionary<string, string> data_row = new Dictionary<string, string>();
            data_row.Add("WJID", model.WJID);
            data_row.Add("BeginTime", model.BeginTime);
            data_row.Add("EndTime", model.EndTime);
            data_row.Add("Mode", model.Mode == 0 ? "计次" : "计时");
            if (!string.IsNullOrEmpty(model.BgeinPhotoID))
            {
                data_row.Add("PhotoMiniPathBegin", App.Instance.Data.ImgMinPath + xd.Photos[model.BgeinPhotoID].PhotoMiniPath);
            }
            if (!string.IsNullOrEmpty(model.EndPhotoID))
            {
                data_row.Add("PhotoMiniPathEnd", App.Instance.Data.ImgMinPath + xd.Photos[model.EndPhotoID].PhotoMiniPath);
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