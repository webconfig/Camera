using System;
using google.protobuf;
using System.Net.Sockets;
using Dos.Model;
using Dos.ORM;
using System.Collections.Generic;

public class DataRecv
{
    public void Action(int tp, byte[] data, Client client)
    {
        switch (tp)
        {
            case 1://登录
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
                    response_login.Result = "1";
                    ClientManager.GetInstance().EndClient(request_login.CustomerID, request_login.Password, request_login.Code);
                    client.CustomerID = request_login.CustomerID;
                    client.pwd = request_login.Password;
                    client.code = request_login.Code;
                    Debug.Info(string.Format("[客户端id：{0},挖机号{1}]-->登录成功", request_login.CustomerID, request_login.Code));
                }
                else
                {
                    response_login.Result = "0";
                    Debug.Info(string.Format("[客户端id：{0},挖机号{1}]-->登录失败", request_login.CustomerID, request_login.Code));
                }
                //====== 测试 ========================
                //response_login.Result = "1.0";
                //response_login.Url = "www.baidu.com";
                //Debug.Info("登录成功");

                NetHelp.Send(1, response_login, client._stream);
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
                NetHelp.Send<GoodsResponse>(2, response_goods, client._stream);
                break;
            case 4:
                google.protobuf.WJ_Record request_record;
                NetHelp.RecvData<google.protobuf.WJ_Record>(data, out request_record);
                Debug.Info(string.Format("[客户端id：{0},挖机号{1}]-->上传Record:{2}", client.CustomerID, client.code, request_record.ID));
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
                ////=== 测试 ===
                //response_record.record_id = request_record.ID;
                //decimal k = request_record.Volum > 0 ? Convert.ToDecimal(request_record.Volum) : 0;
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
                    Volume = request_record.Volum > 0 ? Convert.ToDecimal(request_record.Volum) : 0,
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
                    #region 添加
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
                        State = state,
                        Volume = request_record.Volum > 0 ? Convert.ToDecimal(request_record.Volum) : 0,
                    };
                    num = Db.Context.Insert<Dos.Model.WJ_Record>(record);
                    if (num == 1)
                    {
                        Debug.Info(string.Format("[客户端id：{0},挖机号{1}]-->添加Record:{2}成功", client.CustomerID, client.code, request_record.ID));
                    }
                    else
                    {
                        Debug.Info(string.Format("[客户端id：{0},挖机号{1}]-->添加Record:{2}失败", client.CustomerID, client.code, request_record.ID));
                    }
                    #endregion
                }
                else
                {
                    #region 修改
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
                    #endregion
                }
                if (num == 1)
                {
                    response_record.record_id = request_record.SeqID;
                }
                else
                {
                    response_record.record_id = -1;
                }
                NetHelp.Send<RecordResponse>(3, response_record, client._stream);
                break;
        }
    }
}
