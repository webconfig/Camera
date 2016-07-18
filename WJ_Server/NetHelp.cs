using System;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using google.protobuf;

public  class NetHelp
{
    #region 工具方法
    public static  void Send<T>(int type, T t, IChannelHandlerContext context)
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
        Console.WriteLine("==发送数据：" + data.Length);
        Result.WriteBytes(data);
        context.WriteAndFlushAsync(Result);
    }
    public static void RecvData<T>(byte[] data, out T t)
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
    public static byte[] IntToBytes(int num)
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

