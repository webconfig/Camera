﻿using System;
using System.Collections.Generic;
using google.protobuf;
using Dos.ORM;
using Dos.Model;
using System.Net.Sockets;

public class DataRecv
{
    public void Action(int tp, byte[] data, NetworkStream _stream)
    {
        switch (tp)
        {
            case 1://登录
                Debug.Info("登录");
                LoginRequest request_login;
                NetHelp.RecvData(data, out request_login);
                LoginResponse response_login = new LoginResponse();
                //=========数据库============
                var where = new Where<WJ_Customer>();
                where.And(d => d.CustomerID.Equals(request_login.CustomerID));
                WJ_Customer model;
                model = Db.Context.From<WJ_Customer>().Where(where).First();
                if (model != null && string.Equals(request_login.Password, model.Password))
                {
                    if (request_login.CheckCode)
                    {
                        response_login.Result = WJ_Server.Program.Current.Code;
                        response_login.Url = WJ_Server.Program.Current.Url;
                    }
                    else
                    {
                        response_login.Result = "1";
                    }
                    Debug.Info("登录成功");
                }
                else
                {
                    response_login.Result = "0";
                    Debug.Info("登录失败");
                }
                ////====== 测试 ========================
                //response_login.Result = "1.0";
                //response_login.Url = "www.baidu.com";
                //Debug.Info("登录成功");
                NetHelp.Send(1, response_login, _stream);
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
                NetHelp.Send<GoodsResponse>(2, response_goods, _stream);
                break;
            case 4:
                google.protobuf.WJ_Record request_record;
                NetHelp.RecvData<google.protobuf.WJ_Record>(data, out request_record);
                Debug.Info("上传Record==:" + request_record.ID);
                RecordResponse response_record = new RecordResponse();

                System.DateTime? EndTime = null;
                int state = 1;
                if (!string.IsNullOrEmpty(request_record.EndTime))
                {
                    EndTime = Convert.ToDateTime(request_record.EndTime);
                }
                if (!string.IsNullOrEmpty(request_record.EndPhotoID))
                {//有结束图片
                    state = 2;
                }
                ////===测试===
                //response_record.records.Add(request_record.records[i].ID);

                //===数据库
                WJ_Record_Submit record_submit = new WJ_Record_Submit
                {
                    CustomerID = request_record.CustomerID,
                    WJID = request_record.WJID,
                    ID = request_record.ID,
                    WorkSpace = request_record.WorkSpace,
                    GoodsName = request_record.GoodsName,
                    BeginTime = Convert.ToDateTime(request_record.BeginTime),
                    EndTime = EndTime,
                    BeginPhotoID = request_record.BgeinPhotoID,
                    EndPhotoID = request_record.EndPhotoID,
                    longitude = request_record.longitude,
                    Latitude = request_record.Latitude,
                    Mode = request_record.Mode,
                    State = state
                };
                Db.Context.Insert<WJ_Record_Submit>(record_submit);

                var where_record = new Where<Dos.Model.WJ_Record>();
                where_record.And(d => d.CustomerID.Equals(request_record.CustomerID));
                where_record.And(d => d.WJID.Equals(request_record.WJID));
                where_record.And(d => d.ID.Equals(request_record.ID));
                var model_record = Db.Context.From<Dos.Model.WJ_Record>().Where(where_record).First();
                int num = 0;
                if (model_record == null)
                {
                    Dos.Model.WJ_Record record = new Dos.Model.WJ_Record
                    {
                        CustomerID = request_record.CustomerID,
                        WJID = request_record.WJID,
                        ID = request_record.ID,
                        WorkSpace = request_record.WorkSpace,
                        GoodsName = request_record.GoodsName,
                        BeginTime = Convert.ToDateTime(request_record.BeginTime),
                        EndTime = EndTime,
                        BeginPhotoID = request_record.BgeinPhotoID,
                        EndPhotoID = request_record.EndPhotoID,
                        longitude = request_record.longitude,
                        Latitude = request_record.Latitude,
                        Mode = request_record.Mode,
                        State = state
                    };
                    num = Db.Context.Insert<Dos.Model.WJ_Record>(record);
                    if (num == 1)
                    {
                        Debug.Info("添加Record：" + request_record.ID + "成功");
                    }
                    else
                    {
                        Debug.Info("添加Record：" + request_record.ID + "失败");
                    }
                }
                else
                {
                    model_record.EndPhotoID = request_record.EndPhotoID;
                    model_record.EndTime = EndTime;
                    num = Db.Context.Update<Dos.Model.WJ_Record>(model_record);
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
                    response_record.record_id=request_record.ID;
                }
                else
                {
                    response_record.record_id = -1;
                }
                NetHelp.Send<RecordResponse>(3, response_record, _stream);
                break;
        }
    }
}
