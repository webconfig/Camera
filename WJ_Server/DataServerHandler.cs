using System;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

public class DataServerHandler : ChannelHandlerAdapter
{
    public override void ChannelActive(IChannelHandlerContext context)
    {
        Console.WriteLine("Received from client: " + context.Channel.Id+ "join server");
    }
    public override void ChannelInactive(IChannelHandlerContext context)
    {
        Console.WriteLine("Received from client: " + context.Channel.Id + "leave server");
    }
    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        var buffer = message as IByteBuffer;
        byte[] data = new byte[buffer.ReadableBytes];
        buffer.ReadBytes(data);
        byte[] lenByte = new byte[4];
        System.Array.Copy(data, lenByte, 4);
        int tp = BytesToInt(lenByte, 0);
        byte[] DataByte = new byte[data.Length - 4];
        System.Array.ConstrainedCopy(data, 4, DataByte, 0, DataByte.Length);

        switch(tp)
        {
            case 10:
                MemoryStream ms = new MemoryStream(DataByte);
                SocketModel Model = ProtoBuf.Serializer.Deserialize<SocketModel>(ms);
                Console.WriteLine("=============TYPE_LOGIN====:");

                SocketModel backdata = new SocketModel();
                backdata.SetType(1);
                backdata.SetMessage(new List<string>() { "yyyyyy" });
                IByteBuffer Result = context.Allocator.Buffer();//.alloc().buffer();
                Result.WriteBytes(SendMsg(10, backdata));
                context.WriteAndFlushAsync(Result);
                break;
        }
    }
    public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        Console.WriteLine("Exception: " + exception);
        context.CloseAsync();
    }
    public static int BytesToInt(byte[] data, int offset)
    {
        int num = 0;
        for (int i = offset; i < offset + 4; i++)
        {
            num <<= 8;
            num |= (data[i] & 0xff);
        }
        return num;
    }
    public byte[] SendMsg(int type, SocketModel socketModel)
    {
        byte[] msg = Serial(socketModel);
        byte[] type_value = IntToBytes(type);
        byte[] Length_value = IntToBytes(msg.Length + type_value.Length);
        //消息体结构：消息体长度+消息体
        byte[] data = new byte[Length_value.Length + type_value.Length + msg.Length];
        Length_value.CopyTo(data, 0);
        type_value.CopyTo(data, 4);
        msg.CopyTo(data, 8);
        return data;
        //client.GetStream().Write(data, 0, data.Length);
    }
    private byte[] Serial(SocketModel socketModel)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            Serializer.Serialize<SocketModel>(ms, socketModel);
            byte[] data = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(data, 0, data.Length);
            return data;
        }
    }
    public static byte[] IntToBytes(int num)
    {
        byte[] bytes = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            bytes[i] = (byte)(num >> (24 - i * 8));
        }
        return bytes;
    }
    public class TypeProtocol
    {
        public const int TYPE_LOGIN = 0;

        public const int TYPE_USER = 1;

        public const int TYPE_WIZARD = 2;

        public const int TYPE_BATTLE = 3;
    }
}
