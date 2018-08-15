using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.Base.Filters
{
    /// <summary>
    /// 登录认证特性
    /// </summary>
    public class AuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as BaseController;
            if (filterContext.Filters.Count < 5)
            {
                if (controller == null)
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", controller = "Account" }));
                    return;
                }
                if (controller.CurrentUser == null)
                {
                    if (filterContext.HttpContext.Request.IsHttps)
                    {
                        filterContext.Result = new RedirectResult("/404.html");
                    }
                    else
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", controller = "Account" }));
                    }
                }
                else
                {
                    if (!controller.IsLogin)
                    {
                        string reutrnUrl = string.Empty;
                        foreach (var item in filterContext.RouteData.Values.Values)
                        {
                            reutrnUrl += "/" + item;
                        }
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(
                            new
                            {
                                controller = "Account",
                                action = "Index",
                                returnUrl = reutrnUrl
                            }));
                    }
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
