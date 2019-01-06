using Codewars_Bot.Contracts;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codewars_Bot.Services
{
	public class MessageService : IMessageService
	{
		private IDatabaseService DatabaseService { get; set; }
		private ICodewarsService CodewarsService { get; set; }

		public MessageService(ICodewarsService codewarsService, IDatabaseService databaseService)
		{
			DatabaseService = databaseService;
			CodewarsService = codewarsService;
		}

		public async Task<List<string>> ProcessMessage(Activity activity)
		{
			try
			{
				var reply = new List<string>();
				var requestContent = new
				{
					UserId = activity.From.Id,
					UserName = activity.From.Name,
					Message = activity.Text
				};

				DatabaseService.AuditMessageInDatabase(JsonConvert.SerializeObject(requestContent));

				switch (activity.Text)
				{
					case "/weekly_rating":
						reply = DatabaseService.GetWeeklyRating(false);
						break;
					case "/total_rating":
						reply = DatabaseService.GetTotalRating();
						break;
					case "/my_weekly_points":
						reply = GetWeeklyPoints(activity);
						break;
					case "/delete_userinfo":
						reply.Add(DeleteUserInfo(activity));
						break;
					case "/weekly_rating_channel":
						reply = GetWeeklyRatingForChannel();
						break;
					case "/start":
					case "/show_faq":
						reply.Add(ShowFaq());
						break;
					default:
						var userResponse = await SaveNewUser(activity);
						reply.Add(userResponse);
						break;
				}

				DatabaseService.AuditMessageInDatabase(JsonConvert.SerializeObject(reply));
				return reply;
			}
			catch (Exception ex)
			{
				DatabaseService.AuditMessageInDatabase($"ERROR: {ex.Message} {ex.StackTrace}");
				return new List<string>();
			}
		}

		private async Task<string> SaveNewUser(Activity activity)
		{
			if ((bool)activity.Conversation.IsGroup)
				return string.Empty;

			var regex = new Regex(@"^[a-zA-Z0-9\s_.-]+$", RegexOptions.IgnoreCase);

			if (!regex.Match(activity.Text).Success)
			{
				return $@"Логін Codewars має містити букви, цифри і знак '_'. Якщо ви хотіли дати команду боту -- перевірте правильність написання і чи в ту сторону стоїть слеш на початку. Певні, що це таки ваш нік? Пишіть йому: @maksim36ua";
			}

			var userFromDb = DatabaseService.GetUserById(int.Parse(activity.From.Id));

			if (userFromDb != null)
				return $"Ви вже зареєстровані в рейтингу Codewars під ніком {userFromDb.CodewarsUsername}";

			var user = new UserModel
			{
				CodewarsUsername = activity.Text,
				TelegramUsername = activity.From.Name,
				TelegramId = int.Parse(activity.From.Id)
			};

			var codewarsUser = await CodewarsService.GetCodewarsUser(user.CodewarsUsername);

			if (codewarsUser == null)
			{
				return $"Користувач {user.CodewarsUsername} не зареєстрований на Codewars";
			}
			else
			{
				user.CodewarsFullname = codewarsUser.Name;
				user.Points = codewarsUser.Honor;
			}

			return DatabaseService.SaveUserToDatabase(user);
		}

		private List<string> GetWeeklyPoints(Activity activity)
		{
			return DatabaseService.GetWeeklyPoints(int.Parse(activity.From.Id));
		}

		private string DeleteUserInfo(Activity activity)
		{
			return DatabaseService.DeleteUserInfo(int.Parse(activity.From.Id));
		}

		private List<string> GetWeeklyRatingForChannel()
		{
			var rating = string.Concat(DatabaseService.GetWeeklyRating(true).First(), $@"
Зареєструватись в клані і почати набирати бали можна тут: @itkpi_codewars_bot. Запрошуйте друзів і гайда рубитись! Якщо маєте питання чи баг репорт -- пишіть йому: @maksim36ua");

			return new List<string> { rating };
		}

		private string ShowFaq()
		{
			return @"Вітаємо в клані ІТ КРІ на Codewars! 
			https://codewars.com -- це знаменитий сайт з задачами для програмістів, за розв'язок яких нараховуються бали.
			От цими балами ми і будемо мірятись в кінці кожного тижня. 
			Бот створений для того, щоб зробити реєстрацію в клані максимально швидкою і приємною. Щоб долучитись до рейтингу треба: 
				1) Зареєструватись на https://codewars.com 
				2) Надіслати сюди ваш нікнейм в Codewars.
			Бали оновлюються раз на годину. Також доступні команди: 
				1) /weekly_rating показує поточний рейтинг за цей тиждень. 
				2) /total_rating відображає загальну кількість балів в кожного користувача.
				3) /my_weekly_points відображає історію з кількістю балів в кінці кожного тижня.
				4) /delete_userinfo для того, щоб покинути рейтинг.
			Якщо ви поміняли логін в ТГ і/або логін Codewars -- просто застосуйте команду /delete_userinfo і зареєструйтесь повторно. Дані рейтингу буде збережено.
			Запрошуйте друзів в клан і гайда рубитись!
			P.S: якщо знайшли багу або маєте зауваження -- пишіть йому @maksim36ua";
		}
	}
}