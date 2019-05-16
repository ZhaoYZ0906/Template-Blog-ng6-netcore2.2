 using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogDemo.API.UseExceptionHandler
{
    public static class MyUseExceptionHandler
    {
        public static void MyExceptionHandlerRun(this IApplicationBuilder app, ILoggerFactory loggerFactory) {

            app.UseExceptionHandler(b => {
                b.Run(async c => {
                    c.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    c.Response.ContentType = "application/json";

                    var e = c.Features.Get<IExceptionHandlerFeature>();

                    if (e != null) {
                        var log = loggerFactory.CreateLogger("MyExceptionHandlerRun");
                        log.LogError(500, e.Error, e.Error.Message);
                    }

                    await c.Response.WriteAsync(e?.Error?.Message ?? "不知道什么错");

                });
            });


        }
    }
}
