using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //配置身份服务器与内存中的存储，密钥，客户端和资源
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())//用户信息建模
                .AddInMemoryApiResources(Config.GetApiResources())//添加api资源
                .AddInMemoryClients(Config.GetClients())//添加客户端
                .AddResourceOwnerValidator<LoginValidator>()//用户校验
                .AddProfileService<ProfileService>();
            //注册mvc服务
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //添加到HTTP管道中。
            app.UseIdentityServer();
            //添加静态资源访问
            app.UseStaticFiles();
            //添加mvc到管道中
            app.UseMvcWithDefaultRoute();

        }
    }
}
