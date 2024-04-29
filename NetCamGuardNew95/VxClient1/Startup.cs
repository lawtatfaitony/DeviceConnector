using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using DataBaseBusiness.ModelHistory;
using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VideoGuard.ApiModels;
using VxClient;
using VxClient.Models;
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
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            services.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });
            services.AddMvc().AddRazorRuntimeCompilation(); //更改cshtml页面 刷新浏览器不更新问题
            services.AddControllersWithViews();
            //注意自定義的請求頭 Access-Control-Allow-Headers: X-Custom-Header
            services.AddCors(options =>
            {
                options.AddPolicy("any", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                    builder.WithExposedHeaders("x-custom-header");
                });
            });
            //KestrelServer 作為Http服務器啟動這個,iis的話另外啟動
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            services.Configure<UploadSetting>(Configuration.GetSection("uploadSetting"));

            services.Configure<TokenManagement>(Configuration.GetSection("tokenManagement"));

            var token = Configuration.GetSection("tokenManagement").Get<TokenManagement>();

            //Jwt Or Cookie Authentication ---------------------------------------------------------------------------------
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddPolicyScheme("scheme", "CookieOrJwt", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var bearerAuth = context.Request.Headers["Authorization"].FirstOrDefault()?.StartsWith("Bearer") ?? false;
                    if (bearerAuth)
                        return JwtBearerDefaults.AuthenticationScheme;
                    else
                        return CookieAuthenticationDefaults.AuthenticationScheme;
                };
            })
            //Cookie Mode SETTING  ref https://docs.microsoft.com/zh-cn/aspnet/core/security/authentication/?view=aspnetcore-3.1
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                string Language = LangUtilities.StandardLanguageCode(Thread.CurrentThread.CurrentCulture.Name);
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        if (context.Request.RouteValues["Language"] != null)
                        {
                            Language = context.Request.RouteValues["Language"].ToString();
                        }
                        var isBearerAuth = context.Request.Headers["Authorization"].FirstOrDefault()?.StartsWith("Bearer") ?? false;
                        if (isBearerAuth == false)
                        {
                            context.Response.Redirect(context.RedirectUri.ToString());
                        }
                        else
                        {
                            context.Response.Clear();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.FromResult(0);
                        }

                        return Task.CompletedTask;
                    }
                };
                options.ReturnUrlParameter = "returnUrl";
                options.Cookie.Name = "ConnectToken";
                options.LoginPath = new PathString($"/{Language}/Account/Login");
                options.LogoutPath = new PathString($"/{Language}/Account/Logout");
                options.AccessDeniedPath = new PathString($"/{Language}/Account/AccessDenied");
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(token.RefreshExpiration);

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

                //x.Events.OnAuthenticationFailed 是不是要处理这个事件
            });

            //SwaggerGen---------------------------------------------------------------------------------
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "AI GUARD Connector",
                        Version = "v2.1",
                        Contact = new OpenApiContact
                        {
                            Name = "AI GUARD Connector V2.1"
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

            services.AddHttpContextAccessor();

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            //SinalR
            services.AddSignalR();
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

            //启用：当hhtp请求则重定向https请求
            //app.UseHttpsRedirection();  //启用：当hhtp请求则重定向https请求
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors("any");  //启动跨域策略,必须加在 app.UseRouting();和 app.UseEndpoints（）；之间
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", "API JSON DOCUMENT v1"); ///swagger/v1/swagger.json 
                //c.RoutePrefix = "";
                c.RoutePrefix = "api";
            });

            //Log4net
            app.UseLog4net();

            //app.MapHub<ServerHub>("/ServerHub");

            string languageCode = LangUtilities.StandardLanguageCode(Thread.CurrentThread.CurrentCulture.Name); // LangUtilities.LanguageCodeFromSystem();//default
            // Enable PNA preflight requests
            app.Use(async (ctx, next) =>
            {
                if (ctx.Request.Method.Equals("options", StringComparison.InvariantCultureIgnoreCase) && ctx.Request.Headers.ContainsKey("Access-Control-Request-Private-Network"))
                {
                    ctx.Response.Headers.Add("Access-Control-Allow-Private-Network", "true");
                }
                var endpoint = ctx.GetEndpoint();//GET终结点 
                var routeData = ctx.GetRouteData();// conttext.Request.RouteValues;//GET路由数据

                if (routeData != null)
                {
                    if (routeData.Values["Language"] != null)
                    {
                        languageCode = routeData.Values["Language"].ToString();
                        LangUtilities.LanguageCode = LangUtilities.StandardLanguageCode(languageCode);
                    }
                }
                else
                {
                    var userLangs = ctx.Request.Headers["Accept-Language"].ToString();
                    var firstLang = userLangs.Split(',').FirstOrDefault();
                    //------------------------------------------------------------------------------------------------------------------
                    firstLang = LangUtilities.StandardLanguageCode(firstLang);
                    string firstLangLog = $"[RouteData=null][Request.Headers[Accept-Language]={firstLang}";
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("{0} [{1:yyyy-MM-dd HH:mm:ss fff}]", firstLangLog, DateTime.Now);
                    Console.ResetColor();
                    if (DateTime.Now < new DateTime(2023, 11, 11))  //测试初期 log 记录,维持一年观察情况 Log records at the beginning of the test, and maintain a year of observation
                        CommonBase.OperateDateLoger(firstLangLog);

                    //------------------------------------------------------------------------------------------------------------------
                    LangUtilities.LanguageCode = firstLang;
                    //switch culture
                    Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(LangUtilities.LanguageCode);
                    Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
                }
                await next();
            });


            app.UseEndpoints(endpoints =>
            {

                endpoints.MapControllerRoute(
                    name: "default",
                    constraints: new { Language = "zh-HK|zh-CN|en-US|hk|cn|en|HK|CN|EN" },
                    pattern: $"{{Language={languageCode}}}/{{controller=Home}}/{{action=Index}}/{{id?}}");

                //---------------------------------------------------------------------------
                endpoints.MapGet("/hello/{name:alpha}", async context =>
                {
                    var name = context.Request.RouteValues["name"];
                    await context.Response.WriteAsync($"Hello {name}!");
                });

                endpoints.MapHub<ServerHub>("/ServerHub");  //配置參考 https://docs.microsoft.com/zh-tw/aspnet/core/tutorials/signalr?WT.mc_id=dotnet-35129-website&view=aspnetcore-3.1&tabs=visual-studio

                endpoints.MapHub<HistAlarmHub>("/HistAlarmHub"); //警報
            });

            app.UseCors("any");  //启动跨越策略

            app.UseStatusCodePages((StatusCodeContext statusCodeContext) =>
            {
                var context = statusCodeContext.HttpContext;
                if (context.Response.StatusCode == 401)
                {
                    ResponseModalX responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.UNAUTHORIED, Success = false, Message = $"UNAUTHORIED (401)" },
                        data = null
                    };
                    context.Response.ContentType = "application/json";
                    string json = JsonConvert.SerializeObject(responseModalX);
                    byte[] bytes = UTF8Encoding.Default.GetBytes(json);
                    return context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                }
                return Task.CompletedTask;
            });
        }
    }
}
