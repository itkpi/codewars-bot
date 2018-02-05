using Codewars_Bot.Contracts;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
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

		public async Task<string> ProcessMessage(Activity activity)
		{
			try
			{
				var reply = String.Empty;
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
						reply = DatabaseService.GetWeeklyRating();
						break;
					case "/total_rating":
						reply = DatabaseService.GetTotalRating();
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
						reply = ShowFaq();
						break;
					default:
						reply = await SaveNewUser(activity);
						break;
				}

				DatabaseService.AuditMessageInDatabase(JsonConvert.SerializeObject(reply));
				return reply;
			}
			catch (Exception ex)
			{
				DatabaseService.AuditMessageInDatabase($"ERROR: {ex.Message} {ex.StackTrace}");
				return String.Empty;
			}
		}

		private async Task<string> SaveNewUser(Activity activity)
		{
			if ((bool)activity.Conversation.IsGroup)
				return string.Empty;

			var regex = new Regex(@"^[a-zA-Z0-9\s_.-]+$", RegexOptions.IgnoreCase);

			if (!regex.Match(activity.Text).Success)
			{
				return $@"Логін Codewars має містити букви, цифри і знак '_'
					<br/><br/>Якщо ви хотіли дати команду боту -- перевірте правильність написання і чи в ту сторону стоїть слеш на початку.
					<br/><br/>Певні, що це таки ваш нік? Пишіть йому: @maksim36ua";
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

		private string GetWeeklyPoints(Activity activity)
		{
			return DatabaseService.GetWeeklyPoints(int.Parse(activity.From.Id));
		}

		private string DeleteUserInfo(Activity activity)
		{
			return DatabaseService.DeleteUserInfo(int.Parse(activity.From.Id));
		}

		private string GetWeeklyRatingForChannel()
		{
			var rating = DatabaseService.GetWeeklyRating();

			return rating + @"<br/><br/>Зареєструватись в клані і почати набирати бали можна тут: @itkpi_codewars_bot. 
					<br/><br/>Якщо маєте питання чи баг репорт -- пишіть йому: @maksim36ua";
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