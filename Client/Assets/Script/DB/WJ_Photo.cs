using SQLite4Unity3d;
using System;
/// <summary>
/// 钢瓶出库表
/// </summary>
public class WJ_Photo
{
    public long CustomerID { get; set; }
    public string WJID { get; set; }
    public string PhotoID { get; set; }
    public string PhotoPath { get; set; }
    public string PhotoMiniPath { get; set; }
    public string AtTime { get; set; }
}
