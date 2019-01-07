using Autofac;
using Autofac.Integration.WebApi;
using System.Reflection;
using System.Web.Http;
using Codewars_Bot.Adapters;
using Codewars_Bot.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Codewars_Bot
{
	public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
			var builder = new ContainerBuilder();
			var httpConfig = GlobalConfiguration.Configuration;

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables("CODEWARSBOT_")
                .Add(new ConfigurationManagerProvider())
                .Build();

			builder.RegisterModule(new MessagingModule(config));
			builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
			builder.RegisterWebApiFilterProvider(httpConfig);
			var container = builder.Build();

            httpConfig.DependencyResolver = new AutofacWebApiDependencyResolver(container);

			GlobalConfiguration.Configure(WebApiConfig.Register);
        }

		public static ILifetimeScope FindContainer()
		{
			var config = GlobalConfiguration.Configuration;
			var resolver = (AutofacWebApiDependencyResolver)config.DependencyResolver;
			return resolver.Container;
		}
	}
}
