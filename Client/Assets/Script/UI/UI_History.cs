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
        System.DateTime dt = System.DateTime.Now.AddDays(-1);
        string sql = string.Format("select * from WJ_Record where BeginTime>'{0}' order by BeginTime",dt.ToString("yyyy-MM-dd HH:mm:ss"));
        List<WJ_Record> dts = App.Instance.data.connection.Query<WJ_Record>(sql);
        for (int i = 0; i < dts.Count; i++)
        {
            Dictionary<string, string> data_row = new Dictionary<string, string>();
            data_row.Add("WJID", dts[i].WJID);
            data_row.Add("BeginTime", dts[i].BeginTime);
            data_row.Add("EndTime", dts[i].EndTime);
            data_row.Add("Mode", dts[i].Mode == 0 ? "计次" : "计时");

            if (dts[i].BgeinPhotoID!="0")
            {
                List<WJ_Photo> datas = App.Instance.data.connection.Query<WJ_Photo>(
                    string.Format("select * from WJ_Photo where PhotoID='{0}'",dts[i].BgeinPhotoID));
                if (datas != null && datas.Count > 0)
                {
                    data_row.Add("BgeinPhotoID", datas[0].PhotoPath);

                }
            }
            if (dts[i].EndPhotoID!="0")
            {
                List<WJ_Photo> datas = App.Instance.data.connection.Query<WJ_Photo>(
                    string.Format("select * from WJ_Photo where PhotoID='{0}'", dts[i].EndPhotoID));
                if (datas != null && datas.Count > 0)
                {
                    data_row.Add("EndPhotoID", datas[0].PhotoPath);

                }
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