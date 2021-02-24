using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudentManagement.Data;
using StudentManagement.Middlewares;
using StudentManagement.Models;
using StudentManagement.Security;
using StudentManagement.Security.CustomTokenProvider;

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
            // 注入HttpContextAccessor
            services.AddHttpContextAccessor();

            services.AddDbContextPool<AppDbContext>(
                options => options.UseSqlServer(_configuration.GetConnectionString("StudentDBConnection"))
            );

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedEmail = true;

                // 通过自定义的CustomEmailConfirmation名称来覆盖旧有token名称，
                // 是它与AddTokenProvider<CustomEmailConfirmationTokenProvider<ApplicationUser>>("ltmEmailConfirmation")
                // 关联在一起
                options.Tokens.EmailConfirmationTokenProvider = "ItmEmailConfirmation";
            });

            // Token 有效期修改为 10h
            services.Configure<DataProtectionTokenProviderOptions>(
                opt => opt.TokenLifespan = TimeSpan.FromHours(10));

            // 仅更改电子邮件验证令牌类型的有效时间为 10s
            services.Configure<CustomEmailConfirmationTokenProviderOptions>(
                opt => opt.TokenLifespan = TimeSpan.FromMilliseconds(10));

            services.ConfigureApplicationCookie(options =>
            {
                // 修改拒绝访问的路由地址
                options.AccessDeniedPath = "/Admin/AccessDenied";
                // 修改登录地址的路由
                //options.LoginPath = new PathString("/Admin/Login");
                // 修改注销地址的路由
                //options.LogoutPath = new PathString("Admin/LogOut");
                // 统一系统全局的 Cookie 名称
                options.Cookie.Name = "MockSchoolCookieName";
                // 登录用户 Cookie 的有效期
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                // 是否对 Cookie 启用滑动过期时间
                options.SlidingExpiration = true;
            });

            // 1.添加 Identity 服务    2.使用 AppDbContext 存储与身份认证相关的数据
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddErrorDescriber<CustomIdentityErrorDescriptor>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<CustomEmailConfirmationTokenProvider<ApplicationUser>>("ItmEmailConfirmation");

            // 使用声明式授权
            services.AddAuthorization(options =>
            {
                // 策略结合声明授权
                options.AddPolicy("DeleteRolePolicy", policy => policy.RequireClaim("Delete Role"));
                options.AddPolicy("AdminRolePolicy", policy => policy.RequireRole("Admin"));

                // 策略结合多个角色进行授权
                options.AddPolicy("SuperAdminPolicy", policy => policy.RequireRole("Admin", "User"));

                // Claim Type 不区分大小写； Claim Value 区分大小写
                //options.AddPolicy("EditRolePolicy", policy => policy.RequireClaim("Edit Role", "true"));

                //options.AddPolicy("AllowedCountryPolicy", policy => policy.RequireClaim("Country", "China", "USA", "UK"));

                //options.AddPolicy("EditRolePolicy", policy => policy.RequireAssertion(AuthorizeAccess));

                options.AddPolicy("EditRolePolicy", policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));
                options.InvokeHandlersAfterFailure = false;
            });

            services.AddAuthentication().AddMicrosoftAccount(opt =>
            {
                opt.ClientId = _configuration["Authentication:Microsoft:ClientId"];
                opt.ClientSecret = _configuration["Authentication:Microsoft:ClientSecret"];
            }).AddGitHub(opt =>
            {
                opt.ClientId = _configuration["Authentication:GitHub:ClientId"];
                opt.ClientSecret = _configuration["Authentication:GitHub:ClientSecret"];
            });

            //services.AddMvcCore().AddJsonFormatters();
            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            }).AddXmlSerializerFormatters();

            services.AddScoped<IStudentRepository, SQLStudentRepository>();

            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else if (env.IsStaging() || env.IsProduction() || env.IsEnvironment("UAT"))
            {
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseDataInitializer();

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // 授权访问
        private static bool AuthorizeAccess(AuthorizationHandlerContext context)
        {
            return context.User.IsInRole("Admin") && context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                   context.User.IsInRole("Super Admin");
        }
    }
}