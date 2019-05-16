using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BlogDemo.API
{
    public class StartupProduction
    {
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddHttpsRedirection(o => {
                //重定向状态嘛
                o.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                //设定端口
                o.HttpsPort = 5001;
            });

            services.AddHsts(o => {
                o.Preload = true;
                o.IncludeSubDomains = true;
                o.MaxAge = TimeSpan.FromDays(60);
                o.ExcludedHosts.Add("example.com");
                o.ExcludedHosts.Add("www.example.com");
            });

            services.AddMvc();
        }
        

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHsts();

            app.UseHttpsRedirection();

            app.UseMvc();
        }
    }
}
