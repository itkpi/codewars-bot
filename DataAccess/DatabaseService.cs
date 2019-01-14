using Codewars_Bot.Contracts;
using Codewars_Bot.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Codewars_Bot.Configuration;

namespace Codewars_Bot.DataAccess
{
	public class DatabaseService : IDatabaseService
	{
	    private readonly DbConfig _config;

	    public DatabaseService(DbConfig config)
	    {
	        _config = config;
	    }

		public void AuditMessageInDatabase(string message)
		{
			using (SqlConnection connection = new SqlConnection(_config.DbConnectionString))
			{
				var query = $"INSERT INTO [Audit].[Messages] (Message, DateTime) VALUES (@Message, GETDATE())";
				connection.Query(query, new AuditMessageModel { Message = message });
			}
		}

		public List<string> GetWeeklyRating(bool onlyActiveUsers)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(_config.DbConnectionString))
				{
					var getWeekQuery = $"SELECT TOP 1 * FROM [Rating].[Weeks] ORDER BY WeekNumber DESC";
					var previousWeek = connection.Query<WeekModel>(getWeekQuery).First();

					var getUsersRatingQuery = $"SELECT * FROM [Rating].[WeeklyRatingUserModel] WHERE WeekNumber = {previousWeek.WeekNumber}";
					var usersWithRating = connection.Query<WeeklyRatingUserModel>(getUsersRatingQuery).ToList();

					string query = "SELECT * FROM [User].[Users]";
					var users = connection.Query<UserModel>(query).ToList();

					var currentWeekUsersRating = new List<UserModel>();

					foreach (var user in users)
					{
						var lastWeekUserEntry = usersWithRating.FirstOrDefault(q => q.CodewarsUsername == user.CodewarsUsername);
						var lastWeekPoints = lastWeekUserEntry == null ? 0 : lastWeekUserEntry.Points;

						var newUser = new UserModel
						{
							CodewarsUsername = user.CodewarsUsername,
							Points = user.Points - lastWeekPoints,
							TelegramUsername = user.TelegramUsername
						};

						currentWeekUsersRating.Add(newUser);
					}

					currentWeekUsersRating = currentWeekUsersRating.OrderByDescending(userModel => userModel.Points).ToList();

					var responseList = new List<string>();
					StringBuilder response = new StringBuilder($@"<b>Рейтинг клану IT KPI на Codewars. Тиждень: {previousWeek.WeekNumber}</b>
<b>Загальна кількість учасників: {currentWeekUsersRating.Count}</b>");

					foreach (var user in currentWeekUsersRating)
					{
						if (onlyActiveUsers && user.Points == 0)
							break;

						response.Append(FormatUserRatingString(user, currentWeekUsersRating.IndexOf(user)+1));

						if ((currentWeekUsersRating.IndexOf(user) + 1) % 100 == 0)
						{
							responseList.Add(response.ToString());
							response.Clear();
						}
					}
					responseList.Add(response.ToString());

					return responseList;
				}
			}
			catch (Exception ex)
			{
				AuditMessageInDatabase($"EXCEPTION: {ex.Message}");
				return new List<string>();
			}
		}

		public List<string> GetTotalRating()
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(_config.DbConnectionString))
				{
					string query = "SELECT * FROM [User].[Users]";
					var users = connection.Query<UserModel>(query).ToList();
					var responseList = new List<string>();

					StringBuilder response = new StringBuilder($"<b>Рейтинг клану IT KPI на Codewars</b>");

					var totalUsersRating = users.OrderByDescending(q => q.Points).ToList();
					foreach (var user in totalUsersRating)
					{
						response.Append(FormatUserRatingString(user, totalUsersRating.IndexOf(user) + 1));
						if ((totalUsersRating.IndexOf(user) + 1) % 100 == 0)
						{
							responseList.Add(response.ToString());
							response.Clear();
						}
					}
					responseList.Add(response.ToString());
					return responseList;
				}
			}
			catch (Exception ex)
			{
				AuditMessageInDatabase($"EXCEPTION: {ex.Message}");
				return new List<string>();
			}
		}

		public List<string> GetWeeklyPoints(int userId)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(_config.DbConnectionString))
				{
					StringBuilder response = new StringBuilder();
					var responseList = new List<string>();

					string query = $@"SELECT w.[WeekNumber], [EndDate], wrum.[Points] 
						FROM [Rating].[WeeklyRatingUserModel] wrum
						JOIN [Rating].[Weeks] w on w.WeekNumber = wrum.WeekNumber
						JOIN [User].[Users] u on wrum.CodewarsUsername = u.CodewarsUsername
						WHERE u.TelegramId = {userId}";
							
					var weeklyPoints = connection.Query<WeeklyPointsModel>(query).OrderBy(q => q.WeekNumber).ToList();

					foreach (var week in weeklyPoints)
					{
						response.Append($"\nWeek {week.WeekNumber} ({week.EndDate.ToString("dd.MM.yyyy")}): <b>{week.Points}</b>");
						if ((weeklyPoints.IndexOf(week) + 1) % 100 == 0)
						{
							responseList.Add(response.ToString());
							response.Clear();
						}
					}
					responseList.Add(response.ToString());

					return responseList;
				}
			}
			catch (Exception ex)
			{
				AuditMessageInDatabase($"EXCEPTION: {ex.Message}");
				return new List<string>();
			}
		}

		private string FormatUserRatingString(UserModel user, int position)
		{
			var telegramLogin = user.TelegramUsername != null
				? $"@{user.TelegramUsername}"
				: "";

			var codewarsLogin = position <= 10
				? $"<b>({user.CodewarsUsername.Replace("_", " ")}) - {user.Points}</b>"
				: $"({user.CodewarsUsername.Replace("_", " ")}) - {user.Points}";

			return $"\n{position}) {telegramLogin} {codewarsLogin}";
		}
	}
}
