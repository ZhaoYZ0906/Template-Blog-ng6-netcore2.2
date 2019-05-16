using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BlogDemo.API.UseExceptionHandler;
using BlogDemo.Core.Interfaces.IRepository;
using BlogDemo.Core.Interfaces.UnitOfWork;
using BlogDemo.Infrastructure;
using BlogDemo.Infrastructure.Database;
using BlogDemo.Infrastructure.Dto;
using BlogDemo.Infrastructure.PropertyMapping;
using BlogDemo.Infrastructure.Repository.PostRepository;
using BlogDemo.Infrastructure.ResourcePlasticity.PlasticityValidator;
using BlogDemo.Infrastructure.Services;
using BlogDemo.Infrastructure.Validator;
using FluentValidation;
using FluentValidation.AspNetCore;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

namespace BlogDemo.API
{
    public class StartupDevelopment
    {
        private readonly IConfiguration configuration;

        public StartupDevelopment(IConfiguration configuration)
        {
            this.configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            //https配置
            services.AddHttpsRedirection(o => {
                //重定向状态嘛
                o.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                //设定端口
                o.HttpsPort = 6001;
            });

            //保护API
            services.
                AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme).
                AddIdentityServerAuthentication(option =>
                {
                    //授权地址
                    option.Authority = "https://localhost:5001";
                    option.ApiName = "restapi";
                });

            //dbcontext统一写这里
            services.AddDbContext<MyContext>(o=> {
                
                o.UseSqlServer("Data Source =.; Database = BlogDemo; User Id = sa; Password = 123");
            });

            //automapper
            services.AddAutoMapper();

            //FluentValidation
            services.AddTransient<IValidator<PostAddDto>, PostAddDtoValidator<FPostDto>>();
            services.AddTransient<IValidator<PostUpdateDto>, PostAddDtoValidator<FPostDto>>();
            services.AddTransient<IValidator<PostImageDto>, PostImageDtoValidator>();

            //Repository注入统一写这里
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IPostImageRepository, PostImageRepository>();


            //UnitOfWork注入
            services.AddScoped<IUnitOfWork, UnitOfWork>();


            //生成url必须要进行性的配置
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper,UrlHelper>(o=> {
                var actionContext = o.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
                });

            //排序框架
            var propertyMappingContainer = new PropertyMappingContainer();
            propertyMappingContainer.Register<PostPropertyMapping>();
            services.AddSingleton<IPropertyMappingContainer>(propertyMappingContainer);

            //塑形检查器：负责检查要塑形的属性是否存在
            services.AddTransient<ITypeHelperService, TypeHelperService>();

            services.AddMvc(
                options =>
                {
                    //启用406状态码
                    options.ReturnHttpNotAcceptable = true;

                    ////xml输出器
                    //options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

                    var intputFormatter = options.InputFormatters.OfType<JsonInputFormatter>().FirstOrDefault();
                    if (intputFormatter != null)
                    {
                        intputFormatter.SupportedMediaTypes.Add("application/vnd.post.create+json");
                        intputFormatter.SupportedMediaTypes.Add("application/vnd.post.update+json");
                    }

                    var outputFormatter = options.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();
                    if (outputFormatter != null)
                    {
                        outputFormatter.SupportedMediaTypes.Add("application/vnd.zyz.hateoas+json");
                    }
                })
                .AddJsonOptions(o =>
                {
                    //返回的值全部以小写开头，这个设置不会影响到head里面的数据
                    o.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                }).AddFluentValidation();

            //配置跨域策略
            services.AddCors(options =>
            {
                //名字随便起
                options.AddPolicy("AllowAngularDevOrigin",
                    //允许4200的跨域
                    builder => builder.WithOrigins("http://localhost:4200")
                        //添加一个名字叫X-Pagination的header信息，X-Pagination在控制器中添加。见posts-104行
                        .WithExposedHeaders("x-Pagination")
                        //允许header
                        .AllowAnyHeader()
                        //允许method
                        .AllowAnyMethod());
            });

            //注册全局策略（所有控制器的访问必须通过这些策略）（包括：跨域策略，认证策略和其他一些）
            services.Configure<MvcOptions>(option =>
            {
                //添加跨域策略。要在认证策略之前，添加的策略和配置的策略名称要一致
                option.Filters.Add(new CorsAuthorizationFilterFactory("AllowAngularDevOrigin"));

                //添加一个认证策略
                var policu = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                option.Filters.Add(new AuthorizeFilter(policu));

            });
        }
        

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //app.UseDeveloperExceptionPage();
            app.MyExceptionHandlerRun(loggerFactory);

            //跨域中间件，在https之前，名称和上面的跨域配置一致
            app.UseCors("AllowAngularDevOrigin");

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(); 
        }
    }
}
