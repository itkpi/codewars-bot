using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using Xunit;

using Codewars_Bot;
using Newtonsoft.Json;

namespace ITKPI.CodwarsBot.Tests.Fixture
{
    public class IntegrationTestFixture : IAsyncLifetime
    {
        private ILifetimeScope _scope;
        private DatabaseInfrastructure _dbInfrastructure;

        public async Task InitializeAsync()
        {
            var config = JsonConvert.DeserializeAnonymousType(File.ReadAllText("appsettings.json"), new
            {
                DbConnectionString = string.Empty,
                MigrationFilePath = string.Empty
            });

            InitAutofac();

            var currentDirectory = Directory.GetCurrentDirectory();
            var migrationsPath = Path.Combine(currentDirectory, config.MigrationFilePath);
            _dbInfrastructure = new DatabaseInfrastructure(config.DbConnectionString, migrationsPath);
            await _dbInfrastructure.Create();

            ConfigurationManager.AppSettings["DbConnectionString"] = _dbInfrastructure.DbConnectionString;
        }

        private void InitAutofac()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new MessagingModule());
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
