using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using ICoaster.Model.DependencyInjection.Token;
using ICoaster.Router.WebSocketRouter;
using ICoaster.Services.Token;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using SNotiSSL;
using SNotiSSL.Config;

namespace ICoaster
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public TokenValidationParameters tokenValidationParameters;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = Configuration["JwtSignatureInfo:Audience"],
                ValidIssuer = Configuration["JwtSignatureInfo:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSignatureInfo:SecurityKey"]))//拿到SecurityKey
            };

            services.Configure<JwtTokenConfig>(Configuration.GetSection("JwtSignatureInfo"));
            services.Configure<SNotiClientConfig>(Configuration.GetSection("SNoti"));
            services.AddSingleton<SNotiClient>();
            services.AddSingleton<JwtManager>();

            services.AddCors();
            services.AddWsRouters();
            services.AddMvc()
                    .AddJsonOptions(opt =>
                    {
                        opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                        opt.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    })
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, WsRouter router, JwtManager jwtManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(builder => builder
                                    .AllowAnyOrigin()
                                    .AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .AllowCredentials());

            app.UseWebSockets();
            app.Use(async (context, next) =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    // 先检查有没有Token附在ws上
                    var url = Url.Combine(context.Request.GetDisplayUrl());
                    var reqUrl = new Url(url);
                    if(!reqUrl.QueryParams.ContainsKey("token"))
                    {
                        context.Response.StatusCode = 401;
                        return ;
                    }
                    string jwtToken = reqUrl.QueryParams["token"].ToString();
                    if(jwtManager.ValidateJwtToken(jwtToken,tokenValidationParameters) == null)
                    {
                        context.Response.StatusCode = 401;
                        return;
                    }
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await router.Route(context.Request.Path.Value, context, webSocket);
                }
                else 
                {
                    await next();
                }
            });
            app.UseMvc();
        }
    }
}
