using Codewars_Bot.Contracts;
using Codewars_Bot.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Codewars_Bot.Services
{
	public class DatabaseService : IDatabaseService
	{
		public void AuditMessageInDatabase(string message)
		{
			using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
			{
				var query = $"INSERT INTO [Audit].[Messages] (Message, DateTime) VALUES (@Message, GETDATE())";
				connection.Query(query, new AuditMessageModel { Message = message });
			}
		}

		public List<string> GetWeeklyRating(int? numberOfUsersToDisplay = null)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
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
					StringBuilder response = new StringBuilder($@"**Рейтинг клану IT KPI на Codewars. Тиждень: {previousWeek.WeekNumber}**
															<br/>**Загальна кількість учасників: {currentWeekUsersRating.Count}**<br/>");

					foreach (var user in currentWeekUsersRating)
					{
						response.Append(FormatUserRatingString(user, currentWeekUsersRating.IndexOf(user)+1));

						if (numberOfUsersToDisplay != null && currentWeekUsersRating.IndexOf(user) + 1 == numberOfUsersToDisplay.Value)
							break;

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
				using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
				{
					string query = "SELECT * FROM [User].[Users]";
					var users = connection.Query<UserModel>(query).ToList();
					var responseList = new List<string>();

					StringBuilder response = new StringBuilder($"**Рейтинг клану IT KPI на Codewars**<br/>");

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
				using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
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
						response.Append($"<br/>Week {week.WeekNumber} ({week.EndDate.ToString("dd.MM.yyyy")}): **{week.Points}**");
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

		public string DeleteUserInfo(int userId)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
				{
					string query = $@"DELETE FROM [User].[Users] WHERE TelegramId = {userId}";

					connection.Query(query);
					return "Видалення пройшло успішно";
				}
			}
			catch (Exception ex)
			{
				AuditMessageInDatabase($"EXCEPTION: {ex.Message}");
				return $"Не вдалось видалити дані: {ex.Message}";
			}
		}

		public string SaveUserToDatabase(UserModel user)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
				{
					string query = "INSERT INTO [User].[Users](CodewarsUsername,CodewarsFullname,TelegramUsername,TelegramId,DateTime,Points) values(@CodewarsUsername,@CodewarsFullname,@TelegramUsername,@TelegramId,GETDATE(),@Points); SELECT CAST(SCOPE_IDENTITY() as int)";
					var ra = connection.Query<int>(query, user).SingleOrDefault();
					return $"Реєстрація успішна! Спасибі і хай ваш код завжди компілиться з першого разу :-)";
				}
			}
			catch (Exception ex)
			{
				AuditMessageInDatabase($"EXCEPTION: {ex.Message}, CodewarsUser: {user.CodewarsUsername}");
				return $"Не вдалось створити користувача: {ex.Message}";
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

		private string FormatUserRatingString(UserModel user, int position)
		{
			var telegramLogin = user.TelegramUsername != null
				? $"@{user.TelegramUsername}"
				: "";

			var codewarsLogin = position <= 10
				? $"**({user.CodewarsUsername.Replace("_", " ")}) - {user.Points}**"
				: $"({user.CodewarsUsername.Replace("_", " ")}) - {user.Points}";

			return $"{position}) {telegramLogin} {codewarsLogin} <br/>";
		}

		//private List<string> SplitResponseMessage() { 

		//}
	}
}