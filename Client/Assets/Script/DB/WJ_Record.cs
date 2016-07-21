using SQLite4Unity3d;
[System.Serializable]
public class WJ_Record
{
    public string SeqID;
    public long CustomerID;
    public string WJID;

    public long ID;

    public string WorkSpace;

    public string GoodsName;
    public string BeginTime;
    public string EndTime;

    public string BgeinPhotoID;
    public string EndPhotoID;
    public float longitude;
    public float Latitude;
    public int Mode;


    //=================
    public float time;
    public System.DateTime Time_T;
    public System.DateTime BeginTime_T;
    public System.DateTime EndTime_T;
}
