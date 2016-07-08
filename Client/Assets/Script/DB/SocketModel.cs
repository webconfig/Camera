using System.Collections;
using System.Collections.Generic;
using ProtoBuf;//注意要用到这个dll
[ProtoContract]
public class SocketModel
{
    [ProtoMember(1)]
    private int type;//消息类型
    [ProtoMember(2)]
    private List<string> message;//消息
    public SocketModel()
    {

    }
    public SocketModel(int type, List<string> message)
    {
        this.type = type;
        this.message = message;
    }
    public int GetType()
    {
        return type;
    }
    public void SetType(int type)
    {
        this.type = type;
    }
    public List<string> GetMessage()
    {
        return message;
    }
    public void SetMessage(List<string> message)
    {
        this.message = message;
    }
}