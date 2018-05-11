using Codewars_Bot.Contracts;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Codewars_Bot.Logging;

namespace Codewars_Bot.Services
{
	public class MessageService : IMessageService
	{
		private readonly ILog _log;
		private readonly IDatabaseService _databaseService;
		private readonly ICodewarsService _codewarsService;

		public MessageService(ICodewarsService codewarsService, IDatabaseService databaseService, ILog log)
		{
			_log = log;
			_databaseService = databaseService;
			_codewarsService = codewarsService;
		}

		public async Task<List<string>> ProcessMessage(Activity activity)
		{
			try
			{
				List<string> reply;
				var requestContent = new
				{
					UserId = activity.From.Id,
					UserName = activity.From.Name,
					Message = activity.Text
				};

				_log.Info(JsonConvert.SerializeObject(requestContent));

				switch (activity.Text)
				{
					case "/weekly_rating":
						reply = GetWeeklyRating();
						break;
					case "/total_rating":
						reply = GetTotalRating();
						break;
					case "/my_weekly_points":
						reply = GetWeeklyPoints(activity);
						break;
					case "/delete_userinfo":
						reply = DeleteUserInfo(activity);
						break;
					case "/weekly_rating_channel":
						reply = GetWeeklyRatingForChannel();
						break;
					case "/start":
					case "/show_faq":
						reply = new List<string> { ShowFaq() };
						break;
					default:
						var userResponse = await SaveNewUser(activity);
						reply = new List<string> { userResponse };
						break;
				}

				_log.Info(JsonConvert.SerializeObject(reply));
				return reply;
			}
			catch (Exception ex)
			{
				_log.Error(ex);
				return new List<string>();
			}
		}

		private List<string> GetWeeklyRating()
		{
			var week = _databaseService.GetLastWeek();

			if (week == null)
				return new List<string>();

			var currentWeekUsersRating = _databaseService.GetWeeklyRating(week);
			currentWeekUsersRating = currentWeekUsersRating.OrderByDescending(userModel => userModel.Points).ToList();

			var responseList = new List<string>();
			StringBuilder response = new StringBuilder($@"**Рейтинг клану IT KPI на Codewars. Тиждень: {week.WeekNumber}**
															<br/>**Загальна кількість учасників: {currentWeekUsersRating.Count}**<br/>");

			foreach (var user in currentWeekUsersRating)
			{
				response.Append(FormatUserRatingString(user, currentWeekUsersRating.IndexOf(user) + 1));

				if ((currentWeekUsersRating.IndexOf(user) + 1) % 100 == 0)
				{
					responseList.Add(response.ToString());
					response.Clear();
				}
			}
			responseList.Add(response.ToString());

			return responseList;
		}

		private List<string> GetTotalRating()
		{
			var users = _databaseService.GetTotalRating();
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

		private async Task<string> SaveNewUser(Activity activity)
		{
			if (activity.Conversation.IsGroup.GetValueOrDefault())
				return string.Empty;

			var regex = new Regex(@"^[a-zA-Z0-9\s_.-]+$", RegexOptions.IgnoreCase);

			if (!regex.Match(activity.Text).Success)
			{
				return $@"Логін Codewars має містити букви, цифри і знак '_'
					<br/><br/>Якщо ви хотіли дати команду боту -- перевірте правильність написання і чи в ту сторону стоїть слеш на початку.
					<br/><br/>Певні, що це таки ваш нік? Пишіть йому: @maksim36ua";
			}

			var userFromDb = _databaseService.GetUserById(int.Parse(activity.From.Id));

			if (userFromDb != null)
				return $"Ви вже зареєстровані в рейтингу Codewars під ніком {userFromDb.CodewarsUsername}";

			var user = new UserModel
			{
				CodewarsUsername = activity.Text,
				TelegramUsername = activity.From.Name,
				TelegramId = int.Parse(activity.From.Id)
			};

			var codewarsUser = await _codewarsService.GetCodewarsUser(user.CodewarsUsername);

			if (codewarsUser == null)
			{
				return $"Користувач {user.CodewarsUsername} не зареєстрований на Codewars";
			}

			user.CodewarsFullname = codewarsUser.Name;
			user.Points = codewarsUser.Honor;

			if (_databaseService.SaveUserToDatabase(user))
				return "Реєстрація успішна! Спасибі і хай ваш код завжди компілиться з першого разу :-)";

			return "Не вдалось створити користувача";
		}

		private List<string> GetWeeklyPoints(Activity activity)
		{
			var weeklyPoints = _databaseService.GetWeeklyPoints(int.Parse(activity.From.Id));
			StringBuilder response = new StringBuilder();
			List<string> responseList = new List<string>();
			foreach (var week in weeklyPoints)
			{
				response.Append($"<br/>Week {week.WeekNumber} ({week.EndDate:dd.MM.yyyy}): **{week.Points}**");
				if ((weeklyPoints.IndexOf(week) + 1) % 100 == 0)
				{
					responseList.Add(response.ToString());
					response.Clear();
				}
			}
			responseList.Add(response.ToString());

			return responseList;
		}

		private List<string> DeleteUserInfo(Activity activity)
		{
			if (_databaseService.DeleteUserInfo(int.Parse(activity.From.Id)))
				return new List<string> { "Видалення пройшло успішно" };

			return new List<string> { "Не вдалось видалити дані" };
		}

		private List<string> GetWeeklyRatingForChannel()
		{
			var numberOfUsersToDisplay = 50;
			var week = _databaseService.GetLastWeek();

			if (week == null)
				return new List<string>();

			var currentWeekUsersRating = _databaseService.GetWeeklyRating(week);

			currentWeekUsersRating = currentWeekUsersRating.OrderByDescending(userModel => userModel.Points).ToList();

			StringBuilder response = new StringBuilder($@"**Рейтинг клану IT KPI на Codewars. Тиждень: {week.WeekNumber}**
															<br/>**Загальна кількість учасників: {currentWeekUsersRating.Count}**<br/>");

			foreach (var user in currentWeekUsersRating)
			{
				response.Append(FormatUserRatingString(user, currentWeekUsersRating.IndexOf(user) + 1));

				if (currentWeekUsersRating.IndexOf(user) + 1 == numberOfUsersToDisplay)
					break;
			}

			var rating = string.Concat(response.ToString(), $@"<br/>Зареєструватись в клані і почати набирати бали можна тут: @itkpi_codewars_bot. Запрошуйте друзів і гайда рубитись! Якщо маєте питання чи баг репорт -- пишіть йому: @maksim36ua");

			return new List<string> { rating };
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

		private string ShowFaq()
		{
			return @"Вітаємо в клані ІТ КРІ на Codewars! 
			<br/><br/>https://codewars.com -- це знаменитий сайт з задачами для програмістів, за розв'язок яких нараховуються бали.
			<br/><br/>От цими балами ми і будемо мірятись в кінці кожного тижня. 
			<br/><br/>Бот створений для того, щоб зробити реєстрацію в клані максимально швидкою і приємною. Щоб долучитись до рейтингу треба: 
				<br/>1) Зареєструватись на https://codewars.com 
				<br/>2) Надіслати сюди ваш нікнейм в Codewars.
			<br/><br/>Бали оновлюються раз на годину. Також доступні команди: 
				<br/>1) /weekly_rating показує поточний рейтинг за цей тиждень. 
				<br/>2) /total_rating відображає загальну кількість балів в кожного користувача.
				<br/>3) /my_weekly_points відображає історію з кількістю балів в кінці кожного тижня.
				<br/>4) /delete_userinfo для того, щоб покинути рейтинг.
			<br/><br/>Якщо ви поміняли логін в ТГ і/або логін Codewars -- просто застосуйте команду /delete_userinfo і зареєструйтесь повторно. Дані рейтингу буде збережено.
			<br/><br/>Запрошуйте друзів в клан і гайда рубитись!
			<br/><br/>P.S: якщо знайшли багу або маєте зауваження -- пишіть йому @maksim36ua";
		}
	}
}