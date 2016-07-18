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
    public DataRecv DataServer;
    public FileRecv FileServer;
    public override void ChannelActive(IChannelHandlerContext context)
    {
        Console.WriteLine("Received from client: " + context.Channel.Id+ "join server");
        DataServer = new DataRecv();
        FileServer = new FileRecv();
    }
    public override void ChannelInactive(IChannelHandlerContext context)
    {
        Console.WriteLine("Received from client: " + context.Channel.Id + "-----leave server");
        FileServer.Exit();
    }
    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        var buffer = message as IByteBuffer;
        int bytes_length = buffer.ReadableBytes;

        byte[] data = new byte[bytes_length];
        buffer.ReadBytes(data);
        byte[] lenByte = new byte[4];
        System.Array.Copy(data, lenByte, 4);
        int tp = NetHelp.BytesToInt(lenByte, 0);
        if(tp==0)
        {
            Heart request_heart;
            NetHelp.RecvData<Heart>(data, out request_heart);
            Console.WriteLine("==心跳数据：" + request_heart.time);
            NetHelp.Send<Heart>(0, request_heart, context);
        }
        else if (tp < 10)
        {//传输数据
            DataServer.Action(tp, data, context);
        }
        else if (tp < 20)
        {//传输图片
            FileServer.Action(tp, data, context);
        }
        return;

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
}
