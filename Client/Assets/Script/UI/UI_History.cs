using google.protobuf;
using System.Collections.Generic;
using UnityEngine.UI;
public class UI_History : UI_Base
{
    public Button Btn_Back,Btn_Before,Btn_Next;
    public UI_Control_Table table;
    public Text TxtTotal,TxtTitle;
    public int index = 0;

    public override void UI_Init()
    {
        Btn_Back.onClick.AddListener(Btn_Back_Click);
        Btn_Before.onClick.AddListener(Btn_Before_Click);
        Btn_Next.onClick.AddListener(Btn_Next_Click);
    }

    public override void UI_Start()
    {
        index = App.Instance.Data.HistoryData.Count-1;
        if (index < 0) { index = 0; }
        ShowData(App.Instance.Data.HistoryData[index]);
    }
    private void ShowData(LocalXmlData xd)
    {
        table.Clear();
        TxtTitle.text =string.Format("数据[{0}]", xd.dt);
        WJ_Record model;
        int jc=0, js=0;
        for (int i = 0; i < xd.AllRecords.Count; i++)
        {
            model = xd.AllRecords[i].Data;
            Dictionary<string, string> data_row = new Dictionary<string, string>();
            data_row.Add("WJID", model.WJID);
            data_row.Add("BeginTime", model.BeginTime);
            data_row.Add("EndTime", model.EndTime);
            if(model.Mode==0)
            {
                data_row.Add("Mode","计次");
                jc++;
            }
            else
            {
                data_row.Add("Mode","计时");
                js++;
            }
            if (!string.IsNullOrEmpty(model.BgeinPhotoID))
            {
                data_row.Add("PhotoMiniPathBegin", App.Instance.Data.ImgMinPath + xd.Photos[model.BgeinPhotoID].PhotoMiniPath);
            }
            if (!string.IsNullOrEmpty(model.EndPhotoID)&& model.EndPhotoID!="over")
            {
                data_row.Add("PhotoMiniPathEnd", App.Instance.Data.ImgMinPath + xd.Photos[model.EndPhotoID].PhotoMiniPath);
            }
            table.AddRow(data_row, null);
        }
        TxtTotal.text = string.Format("总共{0}条计次,{1}条计时,合计{2}条记录", jc, js, xd.AllRecords.Count);
        table.ToTop();
    }
    private void Btn_Back_Click()
    {
        UI_Manager.Instance.Back();
    }
    private void Btn_Before_Click()
    {
        if (index > 0)
        {
            index--;
            ShowData(App.Instance.Data.HistoryData[index]);
        }
        else
        {
            TipsManager.Instance.Error("已经是最后一条数据");
        }
    }
    private void Btn_Next_Click()
    {
        if (index < App.Instance.Data.HistoryData.Count - 1)
        {
            index++;
            ShowData(App.Instance.Data.HistoryData[index]);
        }
        else
        {
            TipsManager.Instance.Error("已经是最新数据");
        }
    }
}