using Codewars_Bot.Contracts;
using Codewars_Bot.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Codewars_Bot.Logging;

namespace Codewars_Bot.Services
{
	public class DatabaseService : IDatabaseService
	{
	    private readonly ILog _log;

	    public DatabaseService(ILog log)
	    {
	        _log = log;
	    }

	    public WeekModel GetLastWeek()
	    {
	        try
	        {
	            using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
	            {
	                var getWeekQuery = $"SELECT TOP 1 * FROM [Rating].[Weeks] ORDER BY WeekNumber DESC";
	                return connection.Query<WeekModel>(getWeekQuery).First();
	            }
	        }
	        catch (Exception ex)
	        {
	            _log.Error(ex);
	            return null;
	        }
	    }

		public List<UserModel> GetWeeklyRating(WeekModel week)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
				{
					var getUsersRatingQuery = $"SELECT * FROM [Rating].[WeeklyRatingUserModel] WHERE WeekNumber = {week.WeekNumber}";
					var usersWithRating = connection.Query<WeeklyRatingUserModel>(getUsersRatingQuery).ToList();

					string query = "SELECT * FROM [User].[Users]";
					var users = connection.Query<UserModel>(query).ToList();

					var currentWeekUsersRating = new List<UserModel>();

					foreach (var user in users)
					{
						var lastWeekUserEntry = usersWithRating.FirstOrDefault(q => q.CodewarsUsername == user.CodewarsUsername);
						var lastWeekPoints = lastWeekUserEntry?.Points ?? 0;

						var newUser = new UserModel
						{
							CodewarsUsername = user.CodewarsUsername,
							Points = user.Points - lastWeekPoints,
							TelegramUsername = user.TelegramUsername
						};

						currentWeekUsersRating.Add(newUser);
					}

				    return currentWeekUsersRating;
				}
			}
			catch (Exception ex)
			{
				_log.Error(ex);
				return new List<UserModel>();
			}
		}

		public List<UserModel> GetTotalRating()
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
				{
					string query = "SELECT * FROM [User].[Users]";
					return connection.Query<UserModel>(query).ToList();
				}
			}
			catch (Exception ex)
			{
                _log.Error(ex);
				return new List<UserModel>();
			}
		}

		public List<WeeklyPointsModel> GetWeeklyPoints(int userId)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
				{
					string query = $@"SELECT w.[WeekNumber], [EndDate], wrum.[Points] 
						FROM [Rating].[WeeklyRatingUserModel] wrum
						JOIN [Rating].[Weeks] w on w.WeekNumber = wrum.WeekNumber
						JOIN [User].[Users] u on wrum.CodewarsUsername = u.CodewarsUsername
						WHERE u.TelegramId = {userId}";
							
					return connection.Query<WeeklyPointsModel>(query).OrderBy(q => q.WeekNumber).ToList();
				}
			}
			catch (Exception ex)
			{
				_log.Error(ex.Message);
				return new List<WeeklyPointsModel>();
			}
		}

		public bool DeleteUserInfo(int userId)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
				{
					string query = $@"DELETE FROM [User].[Users] WHERE TelegramId = {userId}";

					connection.Query(query);
					return true;
				}
			}
			catch (Exception ex)
			{
			    _log.Error(ex.Message);
				return false;
			}
		}

		public bool SaveUserToDatabase(UserModel user)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
				{
					string query = "INSERT INTO [User].[Users](CodewarsUsername,CodewarsFullname,TelegramUsername,TelegramId,DateTime,Points) values(@CodewarsUsername,@CodewarsFullname,@TelegramUsername,@TelegramId,GETDATE(),@Points); SELECT CAST(SCOPE_IDENTITY() as int)";
					var ra = connection.Query<int>(query, user).SingleOrDefault();
					return true;
				}
			}
			catch (Exception ex)
			{
				_log.Error(ex, $"CodewarsUser: {user.CodewarsUsername}");
				return false;
			}
		}

		public UserModel GetUserById(int userId)
		{
			using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
			{
				string query = $"SELECT * FROM [User].[Users] WHERE TelegramId = {userId}";
				return connection.QueryFirstOrDefault<UserModel>(query);
			}
		}
	}
}