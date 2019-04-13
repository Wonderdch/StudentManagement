using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace StudentManagement
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use(async (context, next) =>
            {
                context.Response.ContentType = "text/plain;charset=utf-8";

                logger.LogInformation("MW1: 传入请求");
                await next();
                logger.LogInformation("MW1: 传出响应");
            });

            app.Use(async (context, next) =>
            {
                logger.LogInformation("MW2: 传入请求");
                await next();
                logger.LogInformation("MW2: 传出响应");
            });

            app.Run(async (context) =>
            {
                logger.LogInformation("MW3: 传入请求");
                await context.Response.WriteAsync("MW3：处理请求，并生成响应");
                logger.LogInformation("MW3: 传出响应");
            });
        }
    }
}
