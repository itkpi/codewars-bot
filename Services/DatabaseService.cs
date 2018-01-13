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
				connection.Query(query, new AuditMessageModel{Message = message});
			}
		}

		public string GetWeeklyRating()
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
				{
					var getWeekQuery = $"SELECT TOP 1 * FROM [Rating].[Weeks] ORDER BY WeekNumber DESC";
					var previousWeek = connection.Query<WeekModel>(getWeekQuery).First();

					var getUsersRatingQuery = $"SELECT * FROM [Rating].[WeeklyRatingUserModel] WHERE WeekNumber = {previousWeek.WeekNumber}";
					var usersWithRating = connection.Query<WeeklyRatingUserModel>(getUsersRatingQuery).ToList();

					StringBuilder response = new StringBuilder($"**Рейтинг клану IT KPI на Codewars. Тиждень {previousWeek.WeekNumber}**<br/>");

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

					currentWeekUsersRating = currentWeekUsersRating.OrderByDescending(q => q.Points).ToList();
					var position = 1;

					foreach (var user in currentWeekUsersRating)
					{
						response.Append(FormatUserRatingString(user, position));
						position++;
					}

					return response.ToString();
				}
			}
			catch (Exception ex)
			{
				AuditMessageInDatabase($"EXCEPTION: {ex.Message}");
				return $"Не вдалось дістати рейтинг за тиждень";
			}
		}

		public string GetTotalRating()
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
				{
					string query = "SELECT * FROM [User].[Users]";
					var users = connection.Query<UserModel>(query).ToList();
					StringBuilder response = new StringBuilder($"**Рейтинг клану IT KPI на Codewars**<br/>");
					var position = 1;

					foreach (var user in users.OrderByDescending(q => q.Points))
					{
						response.Append(FormatUserRatingString(user, position));
						position++;
					}

					return response.ToString();
				}
			}
			catch (Exception ex)
			{
				AuditMessageInDatabase($"EXCEPTION: {ex.Message}");
				return $"Не вдалось дістати загальний рейтинг";
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

		public UserModel GetUserById(int id)
		{
			using (SqlConnection connection = new SqlConnection(Configuration.DbConnection))
			{
				string query = $"SELECT * FROM [User].[Users] WHERE TelegramId = {id}";
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
	}
}