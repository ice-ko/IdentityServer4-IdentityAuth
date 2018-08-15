using System;
using System.Collections.Generic;
using System.Text;

namespace Components.Base.Models
{
    /// <summary>
    /// 用户登录信息
    /// </summary>
    public class BaseUserResult
    {
        /// <summary>
        /// 用户编号
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 登录凭证
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// 角色编号
        /// </summary>
        public int RoleId { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        public string Avatar { get; set; }
    }
}
