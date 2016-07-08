//using System;
//using System.Text;
//using DotNetty.Buffers;
//using DotNetty.Transport.Channels;
//using DotNetty.Codecs;
//using System.Collections.Generic;
//using ProtoBuf;
//using System.IO;
//public class MessageEncoder : MessageToMessageEncoder<SocketModel>
//{
//    protected override void Encode(IChannelHandlerContext context, SocketModel message, List<object> output)
//    {
//        byte[] msg = Serial(message);
//        //消息体结构：消息体长度+消息体
//        byte[] data = new byte[4 + msg.Length];
//        IntToBytes(msg.Length).CopyTo(data, 0);
//        msg.CopyTo(data, 4);

//        IByteBuffer buffer = context.Allocator.Buffer(data.Length);
//        buffer.WriteBytes(data);
//        output.Add(buffer);
//    }
//    private byte[] Serial(SocketModel socketModel)//将SocketModel转化成字节数组
//    {
//        using (MemoryStream ms = new MemoryStream())
//        {
//            Serializer.Serialize<SocketModel>(ms, socketModel);
//            byte[] data = new byte[ms.Length];
//            ms.Position = 0;
//            ms.Read(data, 0, data.Length);
//            return data;
//        }
//    }
//    public static byte[] IntToBytes(int num)
//    {
//        byte[] bytes = new byte[4];
//        for (int i = 0; i < 4; i++)
//        {
//            bytes[i] = (byte)(num >> (24 - i * 8));
//        }
//        return bytes;
//    }
//}