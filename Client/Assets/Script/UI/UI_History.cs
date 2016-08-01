﻿using google.protobuf;
using System.Collections.Generic;
using UnityEngine.UI;
public class UI_History : UI_Base
{
    public Button Btn_Back,Btn_Before,Btn_Next;
    public UI_Control_Table table;
    public Text TxtTotal;
    public int index = 0;

    public override void UI_Start()
    {
        Btn_Back.onClick.AddListener(Btn_Back_Click);
        Btn_Before.onClick.AddListener(Btn_Before_Click);
        Btn_Before.onClick.AddListener(Btn_Next_Click);
        ShowData(App.Instance.Data.HistoryData[0]);
    }
    private void ShowData(LocalXmlData xd)
    {
        WJ_Record model;
        TxtTotal.text = string.Format("总共{0}条计次,{1}条计时,合计{2}条记录", xd.Records.Count, xd.Records_JS.Count, xd.AllRecords.Count);
        for (int i = 0; i < xd.AllRecords.Count; i++)
        {
            model = xd.AllRecords[i].Data;
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
    private void Btn_Before_Click()
    {
        if (index < App.Instance.Data.HistoryData.Count-1)
        {
            index++;
            ShowData(App.Instance.Data.HistoryData[index]);
        }
    }
    private void Btn_Next_Click()
    {
        if (index >0)
        {
            index--;
            ShowData(App.Instance.Data.HistoryData[index]);
        }
    }
}