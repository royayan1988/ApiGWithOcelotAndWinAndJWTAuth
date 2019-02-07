namespace AuthServer
{
    using AuthServer.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Server.IISIntegration;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddOptions();
            services.Configure<Controllers.Audience>(Configuration.GetSection("Audience"));
            services.AddAuthentication(options => { options.DefaultAuthenticateScheme = IISDefaults.AuthenticationScheme; });
           // services.AddAuthentication(options =>
           // {
           //     options.DefaultScheme = "CustomAuth";
           // })
           //.AddCustomAuth("CustomAuth", o => { });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
