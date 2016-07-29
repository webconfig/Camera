﻿using System.IO;
using google.protobuf;
using System.Net.Sockets;
using Dos.ORM;
using System;
using Dos.Model;

public class FileRecv
{
    public string FilePath = @"D:\FtpDir";
    public System.IO.FileStream fs;
    public bool StartWrite = false;

    public void Action(int tp, byte[] data, NetworkStream _stream)
    {
        switch (tp)
        {
            case 11://开始上传
                FileStartRequest request_file;
                NetHelp.RecvData<FileStartRequest>(data, out request_file);

                //判断文件夹是否存在
                string localpath = string.Format(@"{0}\{1}\{2}\{3}\", FilePath,
                    request_file.CustomerID.ToString(), request_file.AtTime,request_file.WJID); 
                if (!System.IO.Directory.Exists(localpath))
                {
                    System.IO.Directory.CreateDirectory(localpath);
                }

                FileResponse respinse_file = new FileResponse();
                request_file.name = localpath + request_file.name;
                //打开上次的文件或新建文件
                if (System.IO.File.Exists(request_file.name))
                {//有文件，断点续传
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
                NetHelp.Send<FileResponse>(11, respinse_file, _stream);
                Debug.Info("开始上传文件：" + fs.Name);
                break;
            case 12://上传中
                if (!StartWrite) { return; }
                FileSend send_data;
                NetHelp.RecvData<FileSend>(data, out send_data);
                fs.Write(send_data.datas, 0, send_data.datas.Length);
                break;
            case 13://完成
                google.protobuf.WJ_Photo request_over;
                NetHelp.RecvData<google.protobuf.WJ_Photo>(data, out request_over);
                StartWrite = false;
                if (fs != null)
                {
                    fs.Close();
                    Debug.Info("传输完成：" + fs.Name);
                    fs = null;
                }
                //=====写入数据库=====
                //var where = new Where<Dos.Model.WJ_Photo>();
                //where.And(d => d.CustomerID.Equals(request_over.CustomerID));
                //where.And(d => d.WJID.Equals(request_over.WJID));
                //where.And(d => d.PhotoID.Equals(request_over.PhotoID));
                //Dos.Model.WJ_Photo model = Db.Context.From<Dos.Model.WJ_Photo>().Where(where).First();
                //if (model != null)
                //{
                //    Debug.Info(string.Format("===WJ_Photo_Submit 已经存在数据：{0}---{1}---{2}",
                //        request_over.CustomerID, request_over.WJID, request_over.PhotoID));
                //}
                //else
                //{
                //    var model_photo_submit = new WJ_Photo_Submit
                //    {
                //        CustomerID = request_over.CustomerID,
                //        WJID = request_over.WJID,
                //        PhotoID = request_over.PhotoID,
                //        PhotoPath = request_over.PhotoPath,
                //        AtTime = Convert.ToDateTime(request_over.AtTime)
                //    };
                //    Db.Context.Insert<WJ_Photo_Submit>(model_photo_submit);
                //    var model_photo = new Dos.Model.WJ_Photo
                //    {
                //        CustomerID = request_over.CustomerID,
                //        WJID = request_over.WJID,
                //        PhotoID = request_over.PhotoID,
                //        PhotoPath = request_over.PhotoPath,
                //        AtTime = Convert.ToDateTime(request_over.AtTime)
                //    };
                //    Db.Context.Insert<Dos.Model.WJ_Photo>(model_photo);
                //}
                //=====================
                FileResponse response_over = new FileResponse();
                response_over.Result = 1;
                NetHelp.Send<FileResponse>(12, response_over, _stream);
                break;
        }
    }

    public void Exit()
    {
        if(fs!=null)
        {
            fs.Close();
        }
    }
}
