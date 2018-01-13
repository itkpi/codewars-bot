using Autofac;
using Autofac.Integration.WebApi;
using System.Reflection;
using System.Web.Http;

namespace Codewars_Bot
{
	public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
			var builder = new ContainerBuilder();
			var config = GlobalConfiguration.Configuration;

			builder.RegisterModule(new MessagingModule());
			builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
			builder.RegisterWebApiFilterProvider(config);
			var container = builder.Build();

			config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

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
