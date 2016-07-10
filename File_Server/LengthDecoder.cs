using System;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Codecs;
public class LengthDecoder : LengthFieldBasedFrameDecoder
{
    public LengthDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength,
        int lengthAdjustment, int initialBytesToStrip) :
        base(maxFrameLength, lengthFieldOffset, lengthFieldLength, lengthAdjustment, initialBytesToStrip)
    {

    }
    //protected override IByteBuffer ExtractFrame(IChannelHandlerContext context, IByteBuffer buffer, int index, int length)
    //{
    //    return buffer;
    //}
}