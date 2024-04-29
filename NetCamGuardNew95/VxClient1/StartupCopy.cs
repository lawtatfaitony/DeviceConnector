using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using DataBaseBusiness.ModelHistory;
using DataBaseBusiness.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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
            services.AddMvc(options => options.EnableEndpointRouting = false);

            services.AddControllers(); 

            services.AddControllersWithViews();

            services.Configure<UploadSetting>(Configuration.GetSection("uploadSetting"));

            services.Configure<TokenManagement>(Configuration.GetSection("tokenManagement"));
            var token = Configuration.GetSection("tokenManagement").Get<TokenManagement>();

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
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
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Fast Connector",
                        Version = "v1",
                        Contact = new OpenApiContact
                        {
                            //Email = "abcdef@abcdef.com",
                            Name = "ABCD NAME",
                            Url = new Uri("http://abc.com")
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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                        name: "default",
                        template: "{Language}/{controller=Home}/{action=Index}/{id?}");
            });

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();
             
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    constraints: new { Language = "zh-HK|zh-CN|en-US|hk|cn|en|HK|CN|EN" },
                    pattern: "{Language}/{controller=Home}/{action=Index}/{id?}");
            }); 
        }
        //public static IApplicationBuilder UseMvc(this IApplicationBuilder app)
        //{
        //    if(app==null)
        //    {
        //        throw new ArgumentNullException(nameof(app)); 
        //    }
        //    return app.UseMvc(routes=> 
        //    {
        //        routes.MapRoute(
        //                name: "default",
        //                template: "{Language}/{controller=Home}/{action=Index}/{id?}"
        //                );
        //    });
        //}
    }
}
