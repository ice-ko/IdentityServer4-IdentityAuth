using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";
                    //授权端服务地址
                    options.Authority = "http://localhost:5000/";
                    //是否https请求
                    options.RequireHttpsMetadata = false;
                    //客户端ID名称
                    options.ClientId = "mvc";
                    options.ClientSecret = "secret";
                    //返回的类型详解请看：https://www.cnblogs.com/cgzl/p/9253667.html
                    options.ResponseType = "code id_token";
                    //添加自定义用户信息
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    //是否存储token
                    options.SaveTokens = true;
                    //用于设置在从令牌端点接收的id_token创建标识后，处理程序是否应转到用户信息端点
                    options.GetClaimsFromUserInfoEndpoint = true;
                    //访问名称api
                    options.Scope.Add("api1");
                    //避免claims被默认过滤掉，如果不想让中间件过滤掉nbf和amr, 把nbf和amr从被过滤掉集合里移除。可以使用下面这个方方式:
                    options.ClaimActions.Remove("nbf");
                    options.ClaimActions.Remove("amr");
                    //删除某些Claims
                    options.ClaimActions.DeleteClaim("sid");
                    options.ClaimActions.DeleteClaim("idp");
                });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            //添加用户验证中间件
            app.UseAuthentication();
            //
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
