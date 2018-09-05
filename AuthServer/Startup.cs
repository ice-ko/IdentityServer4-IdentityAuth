using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Models;
using AuthServer.Service;
using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace AuthServer
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

            //配置身份服务器与内存中的存储，密钥，客户端和资源
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())//用户信息建模
                .AddInMemoryApiResources(Config.GetApiResources())//添加api资源
                .AddInMemoryClients(Config.GetClients())//添加客户端
                .AddResourceOwnerValidator<LoginValidator>()//用户校验
                .AddProfileService<ProfileService>();
            //QQ登录
            services.AddAuthentication().AddQQ(qqOptions =>
            {
                qqOptions.ClientId = "";
                qqOptions.ClientSecret = "";
            })
            //新浪微博登录
            .AddWeibo(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ClientId = "";
                options.ClientSecret = "";
            })
            //谷歌登录
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ClientId = "";
                options.ClientSecret = "";
            });
            //注册mvc服务
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
            
            //
            app.UseIdentityServer();
            //添加静态资源访问
            app.UseStaticFiles();
            //app.UseStaticFiles(new StaticFileOptions()
            //{
            //    FileProvider = new PhysicalFileProvider(
            //    Path.Combine(Directory.GetCurrentDirectory(), @"afiles")),
            //    RequestPath = new PathString("/staticfiles")
            //});
            //添加mvc默认路由
            app.UseMvcWithDefaultRoute();
        }
    }
}
