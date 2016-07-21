﻿using System;
using System.Diagnostics.Tracing;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Dos.ORM;
using DotNetty.Handlers.Timeout;

namespace WJ_Server
{
    class Program
    {
        static async Task RunServerAsync()
        {
            var eventListener = new ObservableEventListener();
            eventListener.LogToConsole();
            eventListener.EnableEvents(DefaultEventSource.Log, EventLevel.Verbose);

            var bossGroup = new MultithreadEventLoopGroup(1);
            var workerGroup = new MultithreadEventLoopGroup();
            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap
                    .Group(bossGroup, workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoKeepalive, true)
                    .Handler(new LoggingHandler(LogLevel.INFO))
                    .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        pipeline.AddLast(new LengthDecoder(1024*3, 0, 4, 0, 4));
                        pipeline.AddLast(new DataServerHandler());
                    }));

                IChannel bootstrapChannel = await bootstrap.BindAsync(3333);

                //==============
                Db.Context = new DbSession(DatabaseType.SqlServer, "data source=.;initial catalog=RS_PIS;user id=wj;pwd=Mc111111");
                Debug.Info("服务器启动");
                Console.ReadLine();

                await bootstrapChannel.CloseAsync();
            }
            finally
            {
                Task.WaitAll(bossGroup.ShutdownGracefullyAsync(), workerGroup.ShutdownGracefullyAsync());
                eventListener.Dispose();
            }
        }

        static void Main()
        {
            RunServerAsync().Wait();
        }
    }
}
