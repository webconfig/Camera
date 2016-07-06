using SQLite4Unity3d;

public class WJ_Record
{
    public long CustomerID { get; set; }
    public string WJID { get; set; }

    public long ID { get; set; }

    public string WorkSpace { get; set; }

    public string GoodsName { get; set; }
    public string BeginTime { get; set; }
    public string EndTime { get; set; }

    public string BgeinPhotoID { get; set; }
    public string EndPhotoID { get; set; }
    public float longitude { get; set; }
    public float Latitude { get; set; }
    public int Mode { get; set; }
}
