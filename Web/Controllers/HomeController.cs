using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json.Linq;
using Web.Models;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
           
            return View();
        }

        public async Task<IActionResult> About()
        {
            //获取用户信息
            var claimIdentity = (ClaimsIdentity)HttpContext.User.Identity;
            var claimsPrincipal = claimIdentity.Claims as List<Claim>;
            //获取用户token
            var token = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            //实例化HttpClient
            var client = new HttpClient();
            //设置token
            client.SetBearerToken(token);
            //请求identity接口
            var response = await client.GetAsync("http://localhost:5001/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            var content = await response.Content.ReadAsStringAsync();
            return View(JArray.Parse(content));
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        /// <summary>
        /// 注销
        /// </summary>
        /// <returns></returns>
        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc");
        }
    }
}
