using System;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using google.protobuf;
public class DataServerHandler : ChannelHandlerAdapter
{
    public override void ChannelActive(IChannelHandlerContext context)
    {
        Console.WriteLine("Received from client: " + context.Channel.Id+ "join server");
    }
    public override void ChannelInactive(IChannelHandlerContext context)
    {
        Console.WriteLine("Received from client: " + context.Channel.Id + "-----leave server");
    }
    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        var buffer = message as IByteBuffer;
        byte[] data = new byte[buffer.ReadableBytes];
        buffer.ReadBytes(data);
        byte[] lenByte = new byte[4];
        System.Array.Copy(data, lenByte, 4);
        int tp = BytesToInt(lenByte, 0);
        switch(tp)
        {
            case 1://登录
                Console.WriteLine("=========登录=============");
                LoginRequest request_login;
                RecvData<LoginRequest>(data, out request_login);
                LoginResponse response_login = new LoginResponse();
                if(request_login.CustomerID=="admin"&& request_login.Password=="admin")
                {
                    response_login.Result = 1;
                    Console.WriteLine("==登录成功");
                }
                else
                {
                    response_login.Result = 0;
                    Console.WriteLine("==登录失败");
                }
                Send<LoginResponse>(1, response_login, context);
                break;
            case 2://获取Goods
                Console.WriteLine("=========获取Goods=============");
                GoodsRequest request_goods;
                RecvData<GoodsRequest>(data, out request_goods);
                Console.WriteLine("==最新：" + request_goods.time);
                GoodsResponse response_goods = new GoodsResponse();
                if (String.Compare(request_goods.time, "2016-00-00 00") < 0)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        GoodsResponse.WJ_Goods item = new GoodsResponse.WJ_Goods();
                        item.GoodsID = i.ToString();
                        item.GoodsName = "经济健康";
                        item.time = System.DateTime.Now.ToString("yyyy-MM-dd HH");
                        response_goods.result.Add(item);
                    }
                }
                Send<GoodsResponse>(2, response_goods, context);
                break;
            case 3:
                RecordRequest request_record;
                RecvData<RecordRequest>(data, out request_record);
                Console.WriteLine("=========获取Record=============:" + request_record.photos.Count + ":" + request_record.records.Count);
                RecordResponse response_record = new RecordResponse();
                response_record.Result = 1;
                response_record.id = request_record.id;
                Send<RecordResponse>(3, response_record, context);
                break;
            case 4:
                RecordRequest request_record2;
                RecvData<RecordRequest>(data, out request_record2);
                Console.WriteLine("=========获取Record2=============:" + request_record2.photos.Count + ":" + request_record2.records.Count);
                RecordResponse2 response_record2 = new RecordResponse2();
                response_record2.Result = 1;
                for (int i = 0; i < request_record2.photos.Count; i++)
                {
                    response_record2.photos.Add(request_record2.photos[i].PhotoID);
                }
                for (int i = 0; i < request_record2.records.Count; i++)
                {
                    response_record2.records.Add(request_record2.records[i].ID);
                }
                Send<RecordResponse2>(4, response_record2, context);
                break;

        }
    }
    public override void ChannelReadComplete(IChannelHandlerContext context)
    {
        context.Flush();
    }
    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        Console.WriteLine("Exception: " + exception);
        context.CloseAsync();
    }

    #region 工具方法
    public void Send<T>(int type, T t, IChannelHandlerContext context)
    {
        byte[] msg;
        using (MemoryStream ms = new MemoryStream())
        {
            Serializer.Serialize<T>(ms, t);
            msg = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(msg, 0, msg.Length);
        }
        byte[] type_value = IntToBytes(type);
        byte[] Length_value = IntToBytes(msg.Length + type_value.Length);
        //消息体结构：消息体长度+消息体
        byte[] data = new byte[Length_value.Length + type_value.Length + msg.Length];
        Length_value.CopyTo(data, 0);
        type_value.CopyTo(data, 4);
        msg.CopyTo(data, 8);
        IByteBuffer Result = context.Allocator.Buffer();
        Console.WriteLine("==发送数据："+data.Length);
        Result.WriteBytes(data);
        context.WriteAndFlushAsync(Result);
    }
    public void RecvData<T>(byte[] data, out T t)
    {
        byte[] DataByte = new byte[data.Length - 4];
        System.Array.ConstrainedCopy(data, 4, DataByte, 0, DataByte.Length);
        using (MemoryStream ms = new MemoryStream())
        {
            ms.Write(DataByte, 0, DataByte.Length);
            ms.Position = 0;
            t = Serializer.Deserialize<T>(ms);
        }
    }
    public int BytesToInt(byte[] data, int offset)
    {
        int num = 0;
        for (int i = offset; i < offset + 4; i++)
        {
            num <<= 8;
            num |= (data[i] & 0xff);
        }
        return num;
    }
    public byte[] IntToBytes(int num)
    {
        byte[] bytes = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            bytes[i] = (byte)(num >> (24 - i * 8));
        }
        return bytes;
    }
    #endregion
}
