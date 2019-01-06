using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Xunit;

using Codewars_Bot;
using Dapper;
using Configuration = Codewars_Bot.Configuration;

namespace ITKPI.CodwarsBot.Tests
{
    public class IntegrationTestFixture : IAsyncLifetime
    {
        private ILifetimeScope _scope;

        public async Task InitializeAsync()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new MessagingModule());
            var container = builder.Build();

            _scope = container.BeginLifetimeScope();

            var currentDirectory = Directory.GetCurrentDirectory();
            var migrationsPath = Path.Combine(currentDirectory, "..\\..\\..\\DatabaseMigration.sql");
            var migration = File.ReadAllText(migrationsPath);
            var batches = migration.Split(new[] {"GO"}, StringSplitOptions.RemoveEmptyEntries);
            ConfigurationManager.AppSettings["DbConnectionString"] =
                @"Server=localhost,9123;User Id=sa;Password='SomeLongPasswordForRequ1rements!';";

            using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
            {
                await connection.ExecuteAsync(@"
USE [master];
CREATE DATABASE testDb;
");
            }

            ConfigurationManager.AppSettings["DbConnectionString"] += "Database=testDb;";

            using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
            {
                foreach (var batch in batches)
                {
                    await connection.ExecuteAsync(batch);
                }
            }
        }

        public T ResolveDependency<T>()
        {
            return _scope.Resolve<T>();
        }

        public async Task DisposeAsync()
        {
            using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
            {
                await connection.ExecuteAsync("USE [master]; DROP DATABASE testDb;");
            }
        }
    }
}
