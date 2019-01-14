using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using ITKPI.CodewarsBot.Api.Configuration;
using ITKPI.CodewarsBot.Api.Models;
using Microsoft.Extensions.Options;

namespace ITKPI.CodewarsBot.Api.DataAccess
{
    public interface IUsersRepository
    {
        Task<UserModel> Find(int telegramId);

        Task Add(UserModel user);

        Task Delete(int telegramId);
    }

    public class UsersRepository : IUsersRepository
    {
        private readonly DbConfig _dbConfig;

        public UsersRepository(IOptions<DbConfig> dbConfig)
        {
            _dbConfig = dbConfig.Value;
        }

        public async Task<UserModel> Find(int telegramId)
        {
            using (var connection = new SqlConnection(_dbConfig.DbConnectionString))
            {
                string query = $"SELECT * FROM [User].[Users] WHERE TelegramId = {telegramId}";
                return await connection.QueryFirstOrDefaultAsync<UserModel>(query);
            }
        }

        public async Task Add(UserModel user)
        {
            using (var connection = new SqlConnection(_dbConfig.DbConnectionString))
            {
                string query =
                    @"
INSERT INTO [User].[Users](CodewarsUsername, CodewarsFullname, TelegramUsername, TelegramId, DateTime, Points)
values(@CodewarsUsername, @CodewarsFullname, @TelegramUsername, @TelegramId, GETDATE(), @Points);
SELECT CAST(SCOPE_IDENTITY() as int)
";
                await connection.ExecuteAsync(query, user);
            }
        }

        public async Task Delete(int telegramId)
        {
            using (var connection = new SqlConnection(_dbConfig.DbConnectionString))
            {
                string query = $"DELETE FROM [User].[Users] WHERE TelegramId = {telegramId}";
                await connection.ExecuteAsync(query);
            }
        }
    }
}
