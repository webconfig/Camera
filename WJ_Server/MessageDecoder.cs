//using System;
//using System.Text;
//using DotNetty.Buffers;
//using DotNetty.Transport.Channels;
//using DotNetty.Codecs;
//using System.Collections.Generic;
//using ProtoBuf;
//using System.IO;

//public class MessageDecoder : ByteToMessageDecoder
//{
//    protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
//    {
//        //byte[] data = new byte[input.ReadableBytes];
//        //input.ReadBytes(data);
//        //byte[] lenByte = new byte[4];
//        //System.Array.Copy(data, lenByte, 4);
//        //int tp= BytesToInt(lenByte, 0);
//        //byte[] DataByte = new byte[data.Length- 4];
//        //System.Array.ConstrainedCopy(data, 4, DataByte, 0, DataByte.Length);
   
//        //MemoryStream ms = new MemoryStream(data);
//        //SocketModel message = ProtoBuf.Serializer.Deserialize<SocketModel>(ms);
//        //output.Add(tp);
//        //output.Add(DataByte);
//    }
//}