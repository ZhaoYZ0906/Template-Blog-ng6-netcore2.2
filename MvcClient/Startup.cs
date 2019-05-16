using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MvcClient
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


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //用于创建和验证JSON Web令牌的Microsoft.IdentityModel.Tokens.SecurityTokenHandler。
            //关闭mvc端修改token的功能，保证只有idp端的修改。应该是这样
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
            {

                options.DefaultScheme = "Cookies";//随便写，但是要和AddCookie("Cookies",   options.SignInScheme = "Cookies";这两个字符串一致
                options.DefaultChallengeScheme = "oidc";//写oidc 但是要和.AddOpenIdConnect("oidc",这个字符串一致
            })
                .AddCookie("Cookies", options =>
                {
                    //假如没有权限则跳转到这个地址
                    options.AccessDeniedPath = "/Authorization/AccessDenied";
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";
                    //idp地址
                    options.Authority = "https://localhost:5001";
                    //启用https
                    options.RequireHttpsMetadata = true;
                    options.ClientId = "mvcclient";
                    //请求的东西
                    options.ResponseType = "code id_token";
                    options.Scope.Clear();
                    //这个要和idp.config对应方法（也就是mvcclient）中的 AllowedScopes = { IdentityServerConstants.StandardScopes.OpenId }
                    //请求的东西一直
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("restapi");

                    //是否允许保存tokens
                    options.SaveTokens = true;
                    options.ClientSecret = "49C1A7E1-0C79-4A89-A3D6-A37998FB86B0";
                    //是否可以从用户断点请求额外的信息
                    options.GetClaimsFromUserInfoEndpoint = true;
                });
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
