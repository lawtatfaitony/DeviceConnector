using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using DataBaseBusiness.ModelHistory;
using DataBaseBusiness.Models;
using LanguageResource;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using VxGuardClient.Context;
namespace VxGuardClient
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
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = true;
                //options.Filters.Add<MySampleActionFilter>();
                //options.Filters.Add<GlobalResultFilter>(); //配置过滤器
                //options.Filters.Add<CustActionFilter>();
                //options.Filters.Add<AuthFilter>(); // 添加身份验证过滤器
                //options.Filters.Add<ActionFilterAttributeLogin>(); // 添加身份验证过滤器 -- 菜单操作权限
            }).AddRazorRuntimeCompilation();
              
            services.AddControllers(); 

            services.AddControllersWithViews();

            services.Configure<UploadSetting>(Configuration.GetSection("uploadSetting"));

            services.Configure<TokenManagement>(Configuration.GetSection("tokenManagement"));

            var token = Configuration.GetSection("tokenManagement").Get<TokenManagement>();

            //Jwt Or Cookie Authentication ---------------------------------------------------------------------------------
            services.AddAuthentication(options => {
                //options.AddScheme<MyAuthHandler>(MyAuthHandler.SchemeName, "default scheme");
                //options.DefaultAuthenticateScheme = MyAuthHandler.SchemeName;
                //options.DefaultChallengeScheme = MyAuthHandler.SchemeName;
            })
            //Cookie Mode SETTING
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options=>
            { 
                string language = LangUtilities.StandardLanguageCode(Thread.CurrentThread.CurrentCulture.Name);

                options.ReturnUrlParameter = "returnUrl";
                options.Cookie.Name = "AccessToken";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
               
                options.LoginPath = new PathString($"/{language}/Account/Login");
                options.LogoutPath = new PathString($"/{language}/Account/Logout");
                options.AccessDeniedPath = new PathString($"/{language}/Account/AccessDenied");
                //options.EventsType = typeof(CustomCookieAuthenticationEvents);
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        Uri redirectUri = new Uri(System.Uri.UnescapeDataString(context.RedirectUri));
                        string redirectUrl = redirectUri.ToString();
                        //if (context.Request.RouteValues["Language"] != null)
                        //{
                        //    language = context.Request.RouteValues["Language"].ToString();
                        //}
                        //options.LoginPath = new PathString($"/{language}/Account/Login");
                        //options.LogoutPath = new PathString($"/{language}/Account/Logout");
                        //options.AccessDeniedPath = new PathString($"/{language}/Account/AccessDenied");
                        //string redirectUrl = $"{url.Scheme}://{url.Host}:{url.Port}/{language}{url.PathAndQuery}";
                        //context.Response.Headers.Add("location", redirectUrl);

                        context.RedirectUri = redirectUrl;
                        context.Response.Redirect(redirectUrl);
                        return Task.CompletedTask;
                    }
                };
                //---
            })
            //JWT MODE SETTING
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(token.Secret)),
                    ValidIssuer = token.Issuer,
                    ValidAudience = token.Audience,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30) // cache refresh time span where token expired
                };
            });

            //SwaggerGen---------------------------------------------------------------------------------
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Fast Connector",
                        Version = "v1",
                        Contact = new OpenApiContact
                        { 
                            Name = "FAST CONNECTOR V2.1"
                        }
                    });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Description = "Authorization Mode Bearer",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey
                    });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                   {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference()
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        }, Array.Empty<string>()
                    }
                });
            });

            //Connection
            var conn_business_mySqlConnString = Configuration.GetConnectionString("mySqlConn_Business");
            services.AddDbContext<BusinessContext>(option =>
            {
                option.UseMySql(conn_business_mySqlConnString);
                option.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            var conn_history_mySqlConnString = Configuration.GetConnectionString("mySqlConn_History");
            services.AddDbContext<HistoryContext>(option =>
            {
                option.UseMySql(conn_history_mySqlConnString);
                option.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            services.AddScoped<IAuthenticateService, TokenAuthenticationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            //services.AddScoped<CustomCookieAuthenticationEvents>();
            services.AddCors(options =>
            {
                options.AddPolicy("any", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();// Allow Credentials cookie
                });
            });

            services.AddHttpContextAccessor();
        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

            app.UseStaticFiles();
              
            app.UseRouting();

            app.UseAuthentication();
             
            app.UseAuthorization();
             
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", "API JSON DOCUMENT v1"); ///swagger/v1/swagger.json 
                c.RoutePrefix = "";
            });

            //Log4net
            app.UseLog4net();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    constraints: new { Language = "zh-HK|zh-CN|en-US|hk|cn|en|HK|CN|EN" },
                    pattern: "{Language}/{controller=Home}/{action=Index}/{id?}");
            }); 
        } 
    }
}
