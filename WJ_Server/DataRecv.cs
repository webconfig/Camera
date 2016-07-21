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
                Debug.Info("登录");
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
                    Debug.Info("登录成功");
                }
                else
                {
                    response_login.Result = 0;
                    Debug.Info("登录失败");
                }
                ////======测试========================
                //response_login.Result = 1;
                //Debug.Info("登录成功");
                NetHelp.Send<LoginResponse>(1, response_login, context);
                break;
            case 2://获取Goods
                GoodsRequest request_goods;
                NetHelp.RecvData<GoodsRequest>(data, out request_goods);
                Debug.Info("获取Goods：" + request_goods.CustomerID);
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
                Debug.Info("上传Record==:" + request_record.records.Count);
                RecordResponse response_record = new RecordResponse();
                RecordRequest.WJ_Record_Submit up_model;

                for (int i = 0; i < request_record.records.Count; i++)
                {
                    up_model = request_record.records[i];
                    System.DateTime? EndTime = null;
                    if(!string.IsNullOrEmpty(up_model.EndTime))
                    {
                        EndTime = Convert.ToDateTime(up_model.EndTime);
                    }
                    WJ_Record_Submit record_submit = new WJ_Record_Submit
                    {
                        CustomerID = up_model.CustomerID,
                        WJID = up_model.WJID,
                        ID = up_model.ID,
                        WorkSpace = up_model.WorkSpace,
                        GoodsName = up_model.GoodsName,
                        BeginTime = Convert.ToDateTime(up_model.BeginTime),
                        EndTime = EndTime,
                        BeginPhotoID = up_model.BgeinPhotoID,
                        EndPhotoID = up_model.EndPhotoID,
                        longitude = up_model.longitude,
                        Latitude = up_model.Latitude,
                        Mode = up_model.Mode
                    };
                    ////===测试===
                    //response_record.records.Add(request_record.records[i].ID);

                    //===数据库
                    Db.Context.Insert<WJ_Record_Submit>(record_submit);

                    var where_record = new Where<WJ_Record>();
                    where_record.And(d => d.CustomerID.Equals(up_model.CustomerID));
                    where_record.And(d => d.WJID.Equals(up_model.WJID));
                    where_record.And(d => d.ID.Equals(up_model.ID));
                    var model_record = Db.Context.From<WJ_Record>().Where(where_record).First();
                    int num = 0;
                    if (model_record == null)
                    {
                        WJ_Record record = new WJ_Record
                        {
                            CustomerID = up_model.CustomerID,
                            WJID = up_model.WJID,
                            ID = up_model.ID,
                            WorkSpace = up_model.WorkSpace,
                            GoodsName = up_model.GoodsName,
                            BeginTime = Convert.ToDateTime(up_model.BeginTime),
                            EndTime = EndTime,
                            BeginPhotoID = up_model.BgeinPhotoID,
                            EndPhotoID = up_model.EndPhotoID,
                            longitude = up_model.longitude,
                            Latitude = up_model.Latitude,
                            Mode = up_model.Mode
                        };
                        num = Db.Context.Insert<WJ_Record>(record);
                        if (num == 1)
                        {
                            Debug.Info("添加Record：" + up_model.ID + "成功");
                        }
                        else
                        {
                            Debug.Info("添加Record：" + up_model.ID + "失败");
                        }
                    }
                    else
                    {
                        model_record.EndPhotoID = up_model.EndPhotoID;
                        model_record.EndTime = EndTime;
                        num = Db.Context.Update<WJ_Record>(model_record);
                        if (num == 1)
                        {
                            Debug.Info("修改Record：" + model_record.ID + "成功");
                        }
                        else
                        {
                            Debug.Info("修改Record：" + model_record.ID + "失败");
                        }
                    }
                    if (num == 1)
                    {
                        response_record.records.Add(request_record.records[i].ID);
                    }
                }
                NetHelp.Send<RecordResponse>(3, response_record, context);
                break;
        }
    }
}
