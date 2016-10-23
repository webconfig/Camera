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
    
    private long _CustomerID;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"CustomerID", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public long CustomerID
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
    private bool _CheckCode;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"CheckCode", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public bool CheckCode
    {
      get { return _CheckCode; }
      set { _CheckCode = value; }
    }
    private string _Code = "";
    [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name=@"Code", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string Code
    {
      get { return _Code; }
      set { _Code = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"LoginResponse")]
  public partial class LoginResponse : global::ProtoBuf.IExtensible
  {
    public LoginResponse() {}
    
    private string _Result;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"Result", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string Result
    {
      get { return _Result; }
      set { _Result = value; }
    }
    private string _Url = "";
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"Url", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string Url
    {
      get { return _Url; }
      set { _Url = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"GoodsRequest")]
  public partial class GoodsRequest : global::ProtoBuf.IExtensible
  {
    public GoodsRequest() {}
    
    private long _CustomerID;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"CustomerID", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public long CustomerID
    {
      get { return _CustomerID; }
      set { _CustomerID = value; }
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
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"VolumeResponse")]
  public partial class VolumeResponse : global::ProtoBuf.IExtensible
  {
    public VolumeResponse() {}
    
    private readonly global::System.Collections.Generic.List<google.protobuf.VolumeResponse.Volume> _result = new global::System.Collections.Generic.List<google.protobuf.VolumeResponse.Volume>();
    [global::ProtoBuf.ProtoMember(1, Name=@"result", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<google.protobuf.VolumeResponse.Volume> result
    {
      get { return _result; }
    }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"Volume")]
  public partial class Volume : global::ProtoBuf.IExtensible
  {
    public Volume() {}
    
    private string _VolumeID;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"VolumeID", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string VolumeID
    {
      get { return _VolumeID; }
      set { _VolumeID = value; }
    }
    private string _VolumeName;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"VolumeName", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string VolumeName
    {
      get { return _VolumeName; }
      set { _VolumeName = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"WJ_Record")]
  public partial class WJ_Record : global::ProtoBuf.IExtensible
  {
    public WJ_Record() {}
    
    private long _SeqID;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"SeqID", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public long SeqID
    {
      get { return _SeqID; }
      set { _SeqID = value; }
    }
    private long _CustomerID;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"CustomerID", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public long CustomerID
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
    private long _ID;
    [global::ProtoBuf.ProtoMember(4, IsRequired = true, Name=@"ID", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public long ID
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
    private float _longitude;
    [global::ProtoBuf.ProtoMember(11, IsRequired = true, Name=@"longitude", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    public float longitude
    {
      get { return _longitude; }
      set { _longitude = value; }
    }
    private float _Latitude;
    [global::ProtoBuf.ProtoMember(12, IsRequired = true, Name=@"Latitude", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    public float Latitude
    {
      get { return _Latitude; }
      set { _Latitude = value; }
    }
    private int _Mode;
    [global::ProtoBuf.ProtoMember(13, IsRequired = true, Name=@"Mode", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int Mode
    {
      get { return _Mode; }
      set { _Mode = value; }
    }
    private float _Volum = default(float);
    [global::ProtoBuf.ProtoMember(14, IsRequired = false, Name=@"Volum", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    [global::System.ComponentModel.DefaultValue(default(float))]
    public float Volum
    {
      get { return _Volum; }
      set { _Volum = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"RecordResponse")]
  public partial class RecordResponse : global::ProtoBuf.IExtensible
  {
    public RecordResponse() {}
    
    private long _record_id;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"record_id", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public long record_id
    {
      get { return _record_id; }
      set { _record_id = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"FileStartRequest")]
  public partial class FileStartRequest : global::ProtoBuf.IExtensible
  {
    public FileStartRequest() {}
    
    private string _name;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"name", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string name
    {
      get { return _name; }
      set { _name = value; }
    }
    private long _CustomerID;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"CustomerID", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public long CustomerID
    {
      get { return _CustomerID; }
      set { _CustomerID = value; }
    }
    private string _AtTime;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"AtTime", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string AtTime
    {
      get { return _AtTime; }
      set { _AtTime = value; }
    }
    private string _WJID;
    [global::ProtoBuf.ProtoMember(4, IsRequired = true, Name=@"WJID", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string WJID
    {
      get { return _WJID; }
      set { _WJID = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"WJ_Photo")]
  public partial class WJ_Photo : global::ProtoBuf.IExtensible
  {
    public WJ_Photo() {}
    
    private long _CustomerID;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"CustomerID", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public long CustomerID
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
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"FileResponse")]
  public partial class FileResponse : global::ProtoBuf.IExtensible
  {
    public FileResponse() {}
    
    private long _Result;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"Result", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public long Result
    {
      get { return _Result; }
      set { _Result = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"FileSend")]
  public partial class FileSend : global::ProtoBuf.IExtensible
  {
    public FileSend() {}
    
    private byte[] _datas;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"datas", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public byte[] datas
    {
      get { return _datas; }
      set { _datas = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"Heart")]
  public partial class Heart : global::ProtoBuf.IExtensible
  {
    public Heart() {}
    
    private long _time;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"time", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public long time
    {
      get { return _time; }
      set { _time = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}