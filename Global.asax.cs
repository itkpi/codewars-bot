using System;
using System.IO;
using Autofac;
using Autofac.Integration.WebApi;
using System.Reflection;
using System.Web.Http;
using Codewars_Bot.Adapters;
using Codewars_Bot.Configuration;
using Codewars_Bot.Infrastructure;
using Microsoft.Extensions.Configuration;
using Environment = Codewars_Bot.Configuration.Environment;

namespace Codewars_Bot
{
	public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
			var builder = new ContainerBuilder();
			var httpConfig = GlobalConfiguration.Configuration;

            var appConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile($"appsettings.{Environment.Name()}.json", true)
                .AddEnvironmentVariables("CODEWARSBOT_")
                .Add(new ConfigurationManagerProvider())
                .Build();


            builder.RegisterModule(new MessagingModule(appConfig));
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterWebApiFilterProvider(httpConfig);

            if (bool.TryParse(appConfig["RunMigration"], out var runMigration) && runMigration)
            {
                var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var migrationsPath = Path.Combine(currentDirectory, appConfig["MigrationFilePath"]);
                var dbInfrastructure = new DatabaseInfrastructure(appConfig["DBConnectionString"], migrationsPath, "CodewarsBot_Local");

                dbInfrastructure.CreateIfNotExists().Wait();

                builder.RegisterInstance(new DbConfig
                {
                    DbConnectionString = dbInfrastructure.DbConnectionString
                });
            }

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
