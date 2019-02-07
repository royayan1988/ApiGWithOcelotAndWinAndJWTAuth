namespace APIGateway
{    
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Ocelot.DependencyInjection;
    using Ocelot.Middleware;
    using System;
    using System.Text;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.Extensions.Logging;
    using NLog.Extensions.Logging;
    using APIGateway.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using System.Linq;
    using Microsoft.AspNetCore.Server.IISIntegration;

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
            builder.SetBasePath(env.ContentRootPath)
                   .AddJsonFile("appsettings.json")
                   //add configuration.json
                   .AddJsonFile("configuration.json", optional: false, reloadOnChange: true)
                   .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            var audienceConfig = Configuration.GetSection("Audience");

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(audienceConfig["Secret"]));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = audienceConfig["Iss"],
                ValidateAudience = true,
                ValidAudience = audienceConfig["Aud"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true,
            };

            services.AddAuthentication(options => { options.DefaultAuthenticateScheme = IISDefaults.AuthenticationScheme; })
            //    options =>
            //{
            //    options.DefaultScheme = "CustomAuth";
            //    //options.DefaultAuthenticateScheme = "CustomScheme";
            //    //options.DefaultChallengeScheme = "CustomScheme";
            //})
            //.AddCustomAuth("CustomAuth", o => { })
            .AddJwtBearer("TestKey", x =>
             {
                 x.RequireHttpsMetadata = false;
                 x.TokenValidationParameters = tokenValidationParameters;
                 x.Events = new JwtBearerEvents()
                 {
                     OnMessageReceived = async (context) => {
                         var bearer = context.HttpContext.Request.Headers["bearer"].FirstOrDefault();
                         if (!String.IsNullOrEmpty(bearer))
                         {
                             context.Token = bearer;
                         }
                     },
                 };
             });

            services.AddOcelot(Configuration);
        }

        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            //nlog logging
            loggerFactory.AddNLog();
            loggerFactory.ConfigureNLog("nlog.config");
            app.UseAuthentication();
            await app.UseOcelot();
        }
    }
}
