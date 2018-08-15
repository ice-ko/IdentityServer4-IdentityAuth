using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Components.Base.Filters
{
    /// <summary>
    /// 请求返回结果过滤器
    /// </summary>
    public class ResultFilterAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.Result is ViewResult || filterContext.Result is RedirectResult)
            {
                var controller = (BaseController)filterContext.Controller;

                controller.ViewBag.UserInfo = controller.CurrentUser;
            }
            base.OnResultExecuting(filterContext);
        }
    }
}
