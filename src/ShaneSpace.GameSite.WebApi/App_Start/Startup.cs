using Owin;
using Microsoft.Owin;
using static ShaneSpace.GameSite.WebApi.App_Start.StartupConfiguration;
using ShaneSpace.GameSite.WebApi.App_Start;
using Microsoft.Owin.Security.OAuth;
using Microsoft.AspNet.SignalR;
using Autofac.Integration.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Infrastructure;
using Autofac;

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
            var signalRContainer = ConfigureAutofac(app, config, true);
            hubConfiguration.Resolver = new AutofacDependencyResolver(signalRContainer);
            var builder = new ContainerBuilder();
            var connManager = hubConfiguration.Resolver.Resolve<IConnectionManager>();
            builder.RegisterInstance(connManager)
                .As<IConnectionManager>()
                .SingleInstance();
            builder.Update(signalRContainer);

            var builder2 = new ContainerBuilder();
            builder2.RegisterInstance(connManager)
                .As<IConnectionManager>()
                .SingleInstance();
            builder2.Update(container);
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