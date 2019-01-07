using System.IO;
using System.Threading.Tasks;
using Autofac;
using Xunit;

using Codewars_Bot;
using Codewars_Bot.Infrastructure;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ITKPI.CodwarsBot.Tests.Fixture
{
    public class IntegrationTestFixture : IAsyncLifetime
    {
        private ILifetimeScope _scope;
        private DatabaseInfrastructure _dbInfrastructure;

        public async Task InitializeAsync()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var currentDirectory = Directory.GetCurrentDirectory();
            var migrationsPath = Path.Combine(currentDirectory, config["MigrationFilePath"]);
            _dbInfrastructure = new DatabaseInfrastructure(config["DBConnectionString"], migrationsPath);
            await _dbInfrastructure.Create();

            InitAutofac(config);
        }

        private void InitAutofac(IConfiguration config)
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new MessagingModule(config));
            builder.RegisterInstance(new DbConfig
            {
                DbConnectionString = _dbInfrastructure.DbConnectionString
            });

            var container = builder.Build();

            _scope = container.BeginLifetimeScope();
        }

        public T ResolveDependency<T>()
        {
            return _scope.Resolve<T>();
        }

        public async Task DisposeAsync()
        {
            await _dbInfrastructure.Drop();
        }
    }
}
