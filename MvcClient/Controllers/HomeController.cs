using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MvcClient.Models;
using Newtonsoft.Json;

namespace MvcClient.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public async Task< IActionResult> Index()
        {
            var mytoken=   await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);

            ViewBag.mytoken = mytoken;
            return View();
        }

        public async Task<IActionResult> Contact()
        {
            //设置一个地址，指向6001
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:6001")
            };
            //把之前的清理掉
            httpClient.DefaultRequestHeaders.Clear();
            //定义媒体类型（mediatype）
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.zyz.hateoas+json")
            );

            //获取accesstoken-展示-获取BearerToken
            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            ViewData["accessToken"] = accessToken;
            httpClient.SetBearerToken(accessToken);

            //访问api/posts
            var res = await httpClient.GetAsync("api/Posts").ConfigureAwait(false);

            //成功，数据返回view
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                //解析成json对象
                var objects = JsonConvert.DeserializeObject<dynamic>(json);
                ViewData["json"] = objects;
                return View();
            }

            //未验证重定向到下面这个
            if (res.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("AccessDenied", "Authorization");
            }

            //都不是则返回错误
            throw new Exception($"Error Occurred: ${res.ReasonPhrase}");
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
    }
}
