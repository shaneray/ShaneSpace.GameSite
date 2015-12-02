using System.Reflection;
using System.Web.Http;
using System.Web.Http.Cors;

using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin.Cors;

using Owin;

using Swashbuckle.Application;
using ShaneSpace.GameSite.Domain.Data;
using ShaneSpace.GameSite.WebApi.ViewModels;
using System.Linq;
using AutoMapper;
using ShaneSpace.GameSite.Domain.Mapping;
using MediatR;
using Autofac.Features.Variance;
using System.Collections.Generic;
using ShaneSpace.GameSite.WebApi.Cqrs.Games.Command;
using ShaneSpace.Authentication.WebProxy;
using System;
using System.Configuration;
using Autofac.Integration.SignalR;
using ShaneSpace.GameSite.Domain;

namespace ShaneSpace.GameSite.WebApi.App_Start
{
    public static class StartupConfiguration
    {
        public static void ConfigureMiddleware(IAppBuilder app, HttpConfiguration config, IContainer container)
        {
            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(config);
            app.UseWebApi(config);
        }

        public static void ConfigureSwagger(HttpConfiguration config)
        {
            config.EnableSwagger(
                            c =>
                            {
                                c.SingleApiVersion("v1", "ShaneSpace.GameSite");
                            })
                            .EnableSwaggerUi(c => c.InjectJavaScript(typeof(Startup).Assembly, "ShaneSpace.GameSite.WebApi.App_Start.CustomSwaggerJavascript.js"));
        }

        public static HttpConfiguration ConfigureHttp(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            var cors = new EnableCorsAttribute("*", "*", "*"); // enable CORS for all requests
            config.EnableCors(cors);
            return config;
        }

        public static IContainer ConfigureAutofac(IAppBuilder app, HttpConfiguration config, bool signalR = false)
        {
            var builder = new ContainerBuilder();

            var executingAssembly = Assembly.GetExecutingAssembly();
            builder.RegisterApiControllers(executingAssembly);
            builder.RegisterHubs(Assembly.GetExecutingAssembly());

            // Register Context
            if (signalR)
            {
                builder.RegisterType<CoreContext>().AsSelf();
            }
            else
            {
                builder.RegisterType<CoreContext>().AsSelf().InstancePerRequest();
            }
            // Register Mediator
            builder.RegisterSource(new ContravariantRegistrationSource());
            builder.Register(t => new UserWebProxy(new Uri(ConfigurationManager.AppSettings["ShaneSpaceAuthUri"]))).AsImplementedInterfaces();
            builder.Register(t => new OAuthWebProxy(new Uri(ConfigurationManager.AppSettings["ShaneSpaceAuthUri"]))).AsImplementedInterfaces();
            
            builder.RegisterAssemblyTypes(typeof(IMediator).Assembly).AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof(CreateGameCommand).Assembly).AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof(UserMappingService).Assembly).AsImplementedInterfaces();
            builder.Register<SingleInstanceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });
            builder.Register<MultiInstanceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t => (IEnumerable<object>)c.Resolve(typeof(IEnumerable<>).MakeGenericType(t));
            });

            var container = builder.Build();

            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            return container;
        }

        public static void ConfigureAutoMapper()
        {
            var types = Assembly.GetAssembly(typeof(GameViewModel)).GetExportedTypes().ToList();

            CreateStandardMappings(types);
            CreateCustomMappings(types);
        }

        private static void CreateStandardMappings(IEnumerable<Type> types)
        {
            var maps = (types.SelectMany(t => t.GetInterfaces(), (t, i) => new { t, i })
                .Where(
                    @t1 =>
                    @t1.i.IsGenericType && @t1.i.GetGenericTypeDefinition() == typeof(IMapFrom<>) && !@t1.t.IsAbstract
                    && !@t1.t.IsInterface)
                .Select(@t1 => new { Source = @t1.i.GetGenericArguments()[0], Destination = @t1.t, SoureType = @t1.GetType() })).ToArray();

            foreach (var map in maps)
            {
                Mapper.CreateMap(map.Source, map.Destination);

                var typeMap = Mapper.FindTypeMapFor(map.Source, map.Destination);
                if (typeMap == null)
                {
                    continue;
                }

                var propertyMaps = typeMap.GetPropertyMaps().ToList();
                var destinationProperties = map.Destination.GetProperties();

                SetExplicitExpansion(destinationProperties, propertyMaps);
            }
        }

        private static void CreateCustomMappings(IEnumerable<Type> types)
        {
            var maps =
                (types.SelectMany(t => t.GetInterfaces(), (t, i) => new { t, i })
                    .Where(
                        @t1 =>
                        @t1.i.IsGenericType
                        && @t1.i.GetGenericTypeDefinition() == typeof(ICustomMapping<object, object>).GetGenericTypeDefinition()
                        && !@t1.t.IsAbstract
                        && !@t1.t.IsInterface));

            foreach (dynamic instance in maps.Select(map => new { t = Activator.CreateInstance(map.t), map.i }))
            {
                var mappingExpressionType = typeof(ICustomMapping<,>);
                var typeArguments = ((Type)instance.i).GetGenericArguments();
                var mappingInterface = mappingExpressionType.MakeGenericType(typeArguments);

                var target = Activator.CreateInstance(typeArguments[1]);
                var method = mappingInterface.GetMethod("CreateMappings");

                if (method == null)
                {
                    continue;
                }

                dynamic mapping = method.Invoke(target, new object[] { Mapper.Configuration });
                var propertyMaps = ((IEnumerable<PropertyMap>)mapping.TypeMap.GetPropertyMaps()).ToList();
                var destinationProperties = instance.t.GetType().GetProperties();

                SetExplicitExpansion(destinationProperties, propertyMaps);
            }
        }

        private static void SetExplicitExpansion(PropertyInfo[] destinationProperties, List<PropertyMap> propertyMaps)
        {
            foreach (var destinationProperty in destinationProperties)
            {
                var customAttributeDatas = CustomAttributeData.GetCustomAttributes(destinationProperty);
                if (customAttributeDatas.Any(attr => attr.AttributeType == typeof(ExpandableAttribute)))
                {
                    propertyMaps.Single(i => i.DestinationProperty.Name.Equals(destinationProperty.Name)).ExplicitExpansion =
                        true;
                }
            }
        }
    }
}
