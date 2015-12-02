using Owin;
using Microsoft.Owin;
using static ShaneSpace.GameSite.WebApi.App_Start.StartupConfiguration;
using ShaneSpace.GameSite.WebApi.App_Start;
using Microsoft.Owin.Security.OAuth;
using Microsoft.AspNet.SignalR;
using Autofac.Integration.SignalR;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(Startup))]
namespace ShaneSpace.GameSite.WebApi.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = ConfigureHttp(app);
            config.Filters.Add(new GlobalExceptionHandler());
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                Provider = new QueryStringOAuthBearerProvider()
            });
            var container = ConfigureAutofac(app, config);
            
            ConfigureSwagger(config);
            ConfigureMiddleware(app, config, container);
            ConfigureAutoMapper();

            var hubConfiguration = new HubConfiguration();
            hubConfiguration.EnableDetailedErrors = true;
            hubConfiguration.Resolver = new AutofacDependencyResolver(ConfigureAutofac(app, config, true));
            app.MapSignalR(hubConfiguration);
        }
    }

    public class QueryStringOAuthBearerProvider : OAuthBearerAuthenticationProvider
    {
        public override Task RequestToken(OAuthRequestTokenContext context)
        {
            var value = context.Request.Query.Get("access_token");

            if (!string.IsNullOrEmpty(value))
            {
                context.Token = value;
            }

            return Task.FromResult<object>(null);
        }
    }
}