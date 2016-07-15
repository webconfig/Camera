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
    public string FilePath=@"D:\FtpDir\";
    public System.IO.FileStream fs;
    public bool StartWrite = false;
    public override void ChannelActive(IChannelHandlerContext context)
    {
        Console.WriteLine("Received from client: " + context.Channel.Id+ "join server");
    }
    public override void ChannelInactive(IChannelHandlerContext context)
    {
        if (fs != null)
        {
            fs.Close();
        }
        StartWrite = false;
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
        switch (tp)
        {
            case 1://开始上传
                FileStartRequest request_file;
                RecvData<FileStartRequest>(data, out request_file);

                //判断文件夹是否存在
                string localpath = FilePath + request_file.dir;
                if (System.IO.Directory.Exists(localpath))
                {
                    System.IO.Directory.CreateDirectory(localpath);
                }

                FileResponse respinse_file = new FileResponse();
                request_file.name = localpath + request_file.name;
                //打开上次的文件或新建文件
                if (System.IO.File.Exists(request_file.name))
                {
                    fs = System.IO.File.OpenWrite(request_file.name);
                    respinse_file.Result = fs.Length;
                    fs.Seek(fs.Length, SeekOrigin.Current);//移动文件流中的当前指针
                }
                else
                {
                    fs = new System.IO.FileStream(request_file.name, System.IO.FileMode.Create);
                    respinse_file.Result = 0;
                }
                StartWrite = true;
                Send<FileResponse>(1, respinse_file, context);
                Console.WriteLine("=========开始上传文件=============：" + fs.Name);
                break;
            case 2://上传中
                if (!StartWrite) { return; }
                FileSend send_data;
                RecvData<FileSend>(data, out send_data);
                fs.Write(send_data.datas, 0, send_data.datas.Length);
                break;
            case 3://完成
                FileRequest request_over;
                RecvData<FileRequest>(data, out request_over);
                StartWrite = false;
                fs.Close();
                //=====写入数据库=====

                //====================
                FileResponse response_over = new FileResponse();
                response_over.Result = 1;
                Send<FileResponse>(2, response_over, context);
                Console.WriteLine("=========传输完成=============：" + fs.Name);
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
