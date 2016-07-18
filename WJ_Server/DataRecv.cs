using System;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using google.protobuf;
using Dos.ORM;
using Dos.Model;

public class DataRecv
{
    public void Action(int tp, byte[] data, IChannelHandlerContext context)
    {
        switch (tp)
        {
            case 1://登录
                Console.WriteLine("==登录==");
                LoginRequest request_login;
                NetHelp.RecvData<LoginRequest>(data, out request_login);
                LoginResponse response_login = new LoginResponse();
                //=========数据库============
                var where = new Where<WJ_Customer>();
                where.And(d => d.CustomerID.Equals(request_login.CustomerID));
                WJ_Customer model;
                model = Db.Context.From<WJ_Customer>().Where(where).First();
                if (model != null && string.Equals(request_login.Password, model.Password))
                {
                    response_login.Result = 1;
                    Console.WriteLine("==登录成功==");
                }
                else
                {
                    response_login.Result = 0;
                    Console.WriteLine("==登录失败==");
                }
                ////======测试========================
                //response_login.Result = 1;
                //Console.WriteLine("==登录成功==");

                NetHelp.Send<LoginResponse>(1, response_login, context);
                break;
            case 2://获取Goods
                GoodsRequest request_goods;
                NetHelp.RecvData<GoodsRequest>(data, out request_goods);
                Console.WriteLine("==获取Goods：" + request_goods.CustomerID);
                GoodsResponse response_goods = new GoodsResponse();
                //=====数据库=======
                var where_goods = new Where<WJ_GoodsName>();
                where_goods.And(d => d.CustomerID.Equals(request_goods.CustomerID));
                List<WJ_GoodsName> model_customers = Db.Context.From<WJ_GoodsName>().ToList();

                WJ_GoodsName goodsitem;
                for (int i = 0; i < model_customers.Count; i++)
                {
                    goodsitem = model_customers[i];
                    GoodsResponse.WJ_Goods item = new GoodsResponse.WJ_Goods();
                    item.GoodsID = goodsitem.GoodsCode;
                    item.GoodsName = goodsitem.GoodsName;
                    response_goods.result.Add(item);
                }
                ////===测试==============
                //GoodsResponse.WJ_Goods item = new GoodsResponse.WJ_Goods();
                //item.GoodsID = "1";
                //item.GoodsName = "dddd";
                //response_goods.result.Add(item);
                NetHelp.Send<GoodsResponse>(2, response_goods, context);
                break;
            case 4:
                RecordRequest request_record;
                NetHelp.RecvData<RecordRequest>(data, out request_record);
                Console.WriteLine("==上传Record==:" + request_record.records.Count);

                RecordResponse response_record = new RecordResponse();
                RecordRequest.WJ_Record_Submit up_model;

                List<WJ_Record_Submit> RecordSubmits = new List<WJ_Record_Submit>();
                List<WJ_Record> Records = new List<WJ_Record>();
                for (int i = 0; i < request_record.records.Count; i++)
                {
                    up_model = request_record.records[i];
                    RecordSubmits.Add(new WJ_Record_Submit
                    {
                        CustomerID = up_model.CustomerID,
                        WJID = up_model.WJID,
                        ID = up_model.ID,
                        WorkSpace = up_model.WorkSpace,
                        GoodsName = up_model.GoodsName,
                        BeginTime = Convert.ToDateTime(up_model.BeginTime),
                        EndTime = Convert.ToDateTime(up_model.EndTime),
                        BeginPhotoID = up_model.BgeinPhotoID,
                        EndPhotoID = up_model.EndPhotoID,
                        longitude = up_model.longitude,
                        Latitude = up_model.Latitude,
                        Mode = up_model.Mode
                    });
                    Records.Add(new WJ_Record
                    {
                        CustomerID = up_model.CustomerID,
                        WJID = up_model.WJID,
                        ID = up_model.ID,
                        WorkSpace = up_model.WorkSpace,
                        GoodsName = up_model.GoodsName,
                        BeginTime = Convert.ToDateTime(up_model.BeginTime),
                        EndTime = Convert.ToDateTime(up_model.EndTime),
                        BeginPhotoID = up_model.BgeinPhotoID,
                        EndPhotoID = up_model.EndPhotoID,
                        longitude = up_model.longitude,
                        Latitude = up_model.Latitude,
                        Mode = up_model.Mode
                    });
                    response_record.records.Add(request_record.records[i].ID);
                }
                bool ok = true;
                //=====数据库
                var trans = Db.Context.BeginTransaction();
                try
                {
                    trans.Insert<WJ_Record_Submit>(RecordSubmits);
                    trans.Insert<WJ_Record>(Records);
                    trans.Commit();
                }
                catch (Exception)
                {
                    Console.WriteLine("[Error]==存储Record==:" + response_record.records.Count);
                    trans.Rollback();
                    ok = false;
                }
                finally
                {
                    trans.Close();
                }
                if (ok)
                {
                    Console.WriteLine("==成功存储Record==:" + response_record.records.Count);
                    NetHelp.Send<RecordResponse>(3, response_record, context);
                }
                else
                {
                    Console.WriteLine("==存储Record数据库错误==" + response_record.records.Count);
                    response_record.records.Clear();
                    NetHelp.Send<RecordResponse>(3, response_record, context);
                }
                break;
        }
    }
}
