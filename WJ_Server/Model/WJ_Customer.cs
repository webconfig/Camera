﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.18408
//     Website: http://ITdos.com/Dos/ORM/Index.html
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using Dos.ORM;

namespace Dos.Model
{
    /// <summary>
    /// 实体类WJ_Customer。(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Table("WJ_Customer")]
    [Serializable]
    public partial class WJ_Customer : Entity
    {
        #region Model
        private long _CustomerID;
        private string _CustomerName;
        private string _Password;

        /// <summary>
        /// 客户ID
        /// </summary>
        [Field("CustomerID")]
        public long CustomerID
        {
            get { return _CustomerID; }
            set
            {
                this.OnPropertyValueChange("CustomerID");
                this._CustomerID = value;
            }
        }
        /// <summary>
        /// 客户名称
        /// </summary>
        [Field("CustomerName")]
        public string CustomerName
        {
            get { return _CustomerName; }
            set
            {
                this.OnPropertyValueChange("CustomerName");
                this._CustomerName = value;
            }
        }
        /// <summary>
        /// 登录密码
        /// </summary>
        [Field("Password")]
        public string Password
        {
            get { return _Password; }
            set
            {
                this.OnPropertyValueChange("Password");
                this._Password = value;
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// 获取实体中的主键列
        /// </summary>
        public override Field[] GetPrimaryKeyFields()
        {
            return new Field[] {
				_.CustomerID,
			};
        }
        /// <summary>
        /// 获取列信息
        /// </summary>
        public override Field[] GetFields()
        {
            return new Field[] {
				_.CustomerID,
				_.CustomerName,
				_.Password,
			};
        }
        /// <summary>
        /// 获取值信息
        /// </summary>
        public override object[] GetValues()
        {
            return new object[] {
				this._CustomerID,
				this._CustomerName,
				this._Password,
			};
        }
        /// <summary>
        /// 是否是v1.10.5.6及以上版本实体。
        /// </summary>
        /// <returns></returns>
        public override bool V1_10_5_6_Plus()
        {
            return true;
        }
        #endregion

        #region _Field
        /// <summary>
        /// 字段信息
        /// </summary>
        public class _
        {
            /// <summary>
            /// * 
            /// </summary>
            public readonly static Field All = new Field("*", "WJ_Customer");
            /// <summary>
            /// 客户ID
            /// </summary>
            public readonly static Field CustomerID = new Field("CustomerID", "WJ_Customer", "客户ID");
            /// <summary>
            /// 客户名称
            /// </summary>
            public readonly static Field CustomerName = new Field("CustomerName", "WJ_Customer", "客户名称");
            /// <summary>
            /// 登录密码
            /// </summary>
            public readonly static Field Password = new Field("Password", "WJ_Customer", "登录密码");
        }
        #endregion
    }
}