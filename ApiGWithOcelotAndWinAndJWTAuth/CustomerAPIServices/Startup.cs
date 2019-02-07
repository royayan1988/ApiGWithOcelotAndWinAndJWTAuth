namespace CustomerAPIServices
{
    using System;
    using System.Linq;
    using System.Text;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Server.IISIntegration;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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
            services.AddAuthentication(options => { options.DefaultAuthenticateScheme = IISDefaults.AuthenticationScheme; options.DefaultChallengeScheme = "TestKey"; })
            
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

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {            
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
