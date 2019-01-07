using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Dapper;

namespace Codewars_Bot.Infrastructure
{
    public class DatabaseInfrastructure
    {
        private readonly string _connectionString;
        private readonly string _migrationsPath;
        private readonly string _dbName;

        public DatabaseInfrastructure(string connectionString, string migrationsPath, string dbName = null)
        {
            _connectionString = connectionString;
            _migrationsPath = migrationsPath;
            _dbName = dbName ?? $"TestDb_{Guid.NewGuid():N}";
        }

        public string DbConnectionString => $"{_connectionString};Database={_dbName}";

        public async Task Create()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync($@"
USE [master];
CREATE DATABASE {_dbName};
");
            }

            var migration = File.ReadAllText(_migrationsPath);

            // Split into statements
            var statementBatches = migration.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
            using (var connection = new SqlConnection(DbConnectionString))
            {
                foreach (var statement in statementBatches)
                {
                    await connection.ExecuteAsync(statement);
                }
            }
        }

        public async Task Drop()
        {
            using (var connection = new SqlConnection(_connectionString))
            {

                await connection.ExecuteAsync($@"
USE [master];
ALTER DATABASE {_dbName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
"); // Force disconnect everyone
                await connection.ExecuteAsync($@"
USE [master];
DROP DATABASE IF EXISTS {_dbName};
");
            }
        }
    }
}
