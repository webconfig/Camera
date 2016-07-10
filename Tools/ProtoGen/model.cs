//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: model.proto
namespace google.protobuf
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"LoginRequest")]
  public partial class LoginRequest : global::ProtoBuf.IExtensible
  {
    public LoginRequest() {}
    
    private string _CustomerID;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"CustomerID", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string CustomerID
    {
      get { return _CustomerID; }
      set { _CustomerID = value; }
    }
    private string _Password;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"Password", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string Password
    {
      get { return _Password; }
      set { _Password = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"LoginResponse")]
  public partial class LoginResponse : global::ProtoBuf.IExtensible
  {
    public LoginResponse() {}
    
    private int _Result;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"Result", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int Result
    {
      get { return _Result; }
      set { _Result = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"GoodsRequest")]
  public partial class GoodsRequest : global::ProtoBuf.IExtensible
  {
    public GoodsRequest() {}
    
    private string _time;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"time", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string time
    {
      get { return _time; }
      set { _time = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"GoodsResponse")]
  public partial class GoodsResponse : global::ProtoBuf.IExtensible
  {
    public GoodsResponse() {}
    
    private readonly global::System.Collections.Generic.List<google.protobuf.GoodsResponse.WJ_Goods> _result = new global::System.Collections.Generic.List<google.protobuf.GoodsResponse.WJ_Goods>();
    [global::ProtoBuf.ProtoMember(1, Name=@"result", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<google.protobuf.GoodsResponse.WJ_Goods> result
    {
      get { return _result; }
    }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"WJ_Goods")]
  public partial class WJ_Goods : global::ProtoBuf.IExtensible
  {
    public WJ_Goods() {}
    
    private string _GoodsID;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"GoodsID", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string GoodsID
    {
      get { return _GoodsID; }
      set { _GoodsID = value; }
    }
    private string _GoodsName;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"GoodsName", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string GoodsName
    {
      get { return _GoodsName; }
      set { _GoodsName = value; }
    }
    private string _time;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"time", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string time
    {
      get { return _time; }
      set { _time = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"RecordRequest")]
  public partial class RecordRequest : global::ProtoBuf.IExtensible
  {
    public RecordRequest() {}
    
    private readonly global::System.Collections.Generic.List<google.protobuf.RecordRequest.WJ_Photo_Submit> _photos = new global::System.Collections.Generic.List<google.protobuf.RecordRequest.WJ_Photo_Submit>();
    [global::ProtoBuf.ProtoMember(1, Name=@"photos", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<google.protobuf.RecordRequest.WJ_Photo_Submit> photos
    {
      get { return _photos; }
    }
  
    private readonly global::System.Collections.Generic.List<google.protobuf.RecordRequest.WJ_Record_Submit> _records = new global::System.Collections.Generic.List<google.protobuf.RecordRequest.WJ_Record_Submit>();
    [global::ProtoBuf.ProtoMember(2, Name=@"records", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<google.protobuf.RecordRequest.WJ_Record_Submit> records
    {
      get { return _records; }
    }
  
    private string _id;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"id", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string id
    {
      get { return _id; }
      set { _id = value; }
    }
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"WJ_Photo_Submit")]
  public partial class WJ_Photo_Submit : global::ProtoBuf.IExtensible
  {
    public WJ_Photo_Submit() {}
    
    private string _CustomerID;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"CustomerID", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string CustomerID
    {
      get { return _CustomerID; }
      set { _CustomerID = value; }
    }
    private string _WJID;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"WJID", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string WJID
    {
      get { return _WJID; }
      set { _WJID = value; }
    }
    private string _PhotoID;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"PhotoID", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string PhotoID
    {
      get { return _PhotoID; }
      set { _PhotoID = value; }
    }
    private string _PhotoPath;
    [global::ProtoBuf.ProtoMember(4, IsRequired = true, Name=@"PhotoPath", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string PhotoPath
    {
      get { return _PhotoPath; }
      set { _PhotoPath = value; }
    }
    private string _AtTime;
    [global::ProtoBuf.ProtoMember(5, IsRequired = true, Name=@"AtTime", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string AtTime
    {
      get { return _AtTime; }
      set { _AtTime = value; }
    }
    private string _SeqID;
    [global::ProtoBuf.ProtoMember(6, IsRequired = true, Name=@"SeqID", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string SeqID
    {
      get { return _SeqID; }
      set { _SeqID = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"WJ_Record_Submit")]
  public partial class WJ_Record_Submit : global::ProtoBuf.IExtensible
  {
    public WJ_Record_Submit() {}
    
    private string _SeqID;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"SeqID", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string SeqID
    {
      get { return _SeqID; }
      set { _SeqID = value; }
    }
    private string _CustomerID;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"CustomerID", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string CustomerID
    {
      get { return _CustomerID; }
      set { _CustomerID = value; }
    }
    private string _WJID;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"WJID", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string WJID
    {
      get { return _WJID; }
      set { _WJID = value; }
    }
    private string _ID;
    [global::ProtoBuf.ProtoMember(4, IsRequired = true, Name=@"ID", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string ID
    {
      get { return _ID; }
      set { _ID = value; }
    }
    private string _WorkSpace;
    [global::ProtoBuf.ProtoMember(5, IsRequired = true, Name=@"WorkSpace", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string WorkSpace
    {
      get { return _WorkSpace; }
      set { _WorkSpace = value; }
    }
    private string _GoodsName;
    [global::ProtoBuf.ProtoMember(6, IsRequired = true, Name=@"GoodsName", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string GoodsName
    {
      get { return _GoodsName; }
      set { _GoodsName = value; }
    }
    private string _BeginTime;
    [global::ProtoBuf.ProtoMember(7, IsRequired = true, Name=@"BeginTime", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string BeginTime
    {
      get { return _BeginTime; }
      set { _BeginTime = value; }
    }
    private string _EndTime;
    [global::ProtoBuf.ProtoMember(8, IsRequired = true, Name=@"EndTime", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string EndTime
    {
      get { return _EndTime; }
      set { _EndTime = value; }
    }
    private string _BgeinPhotoID;
    [global::ProtoBuf.ProtoMember(9, IsRequired = true, Name=@"BgeinPhotoID", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string BgeinPhotoID
    {
      get { return _BgeinPhotoID; }
      set { _BgeinPhotoID = value; }
    }
    private string _EndPhotoID;
    [global::ProtoBuf.ProtoMember(10, IsRequired = true, Name=@"EndPhotoID", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string EndPhotoID
    {
      get { return _EndPhotoID; }
      set { _EndPhotoID = value; }
    }
    private string _longitude;
    [global::ProtoBuf.ProtoMember(11, IsRequired = true, Name=@"longitude", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string longitude
    {
      get { return _longitude; }
      set { _longitude = value; }
    }
    private string _Latitude;
    [global::ProtoBuf.ProtoMember(12, IsRequired = true, Name=@"Latitude", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string Latitude
    {
      get { return _Latitude; }
      set { _Latitude = value; }
    }
    private string _Mode;
    [global::ProtoBuf.ProtoMember(13, IsRequired = true, Name=@"Mode", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string Mode
    {
      get { return _Mode; }
      set { _Mode = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"RecordResponse")]
  public partial class RecordResponse : global::ProtoBuf.IExtensible
  {
    public RecordResponse() {}
    
    private int _Result;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"Result", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int Result
    {
      get { return _Result; }
      set { _Result = value; }
    }
    private string _id;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"id", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string id
    {
      get { return _id; }
      set { _id = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"RecordResponse2")]
  public partial class RecordResponse2 : global::ProtoBuf.IExtensible
  {
    public RecordResponse2() {}
    
    private int _Result;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"Result", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int Result
    {
      get { return _Result; }
      set { _Result = value; }
    }
    private readonly global::System.Collections.Generic.List<string> _photos = new global::System.Collections.Generic.List<string>();
    [global::ProtoBuf.ProtoMember(2, Name=@"photos", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<string> photos
    {
      get { return _photos; }
    }
  
    private readonly global::System.Collections.Generic.List<string> _records = new global::System.Collections.Generic.List<string>();
    [global::ProtoBuf.ProtoMember(3, Name=@"records", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<string> records
    {
      get { return _records; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}