using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITKPI.CodewarsBot.Api.DataAccess;
using ITKPI.CodewarsBot.Api.Contracts;
using ITKPI.CodewarsBot.Api.Models;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace ITKPI.CodewarsBot.Api.Services
{
	public class MessageService : IMessageService
	{
        private readonly IUsersRepository _usersRepository;
        private readonly ICodewarsApiClient _codewarsClient;
        private readonly IDatabaseService _databaseService;

		public MessageService(ICodewarsApiClient codewarsClient, IDatabaseService databaseService, IUsersRepository usersRepository)
		{
            _usersRepository = usersRepository;
            _databaseService = databaseService;
			_codewarsClient = codewarsClient;
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

                _databaseService.AuditMessageInDatabase(JsonConvert.SerializeObject(requestContent));

                switch (activity.Text)
                {
                    case "/weekly_rating":
                        reply = _databaseService.GetWeeklyRating(false);
                        break;
                    case "/total_rating":
                        reply = _databaseService.GetTotalRating();
                        break;
                    case "/my_weekly_points":
                        reply = GetWeeklyPoints(activity);
                        break;
                    case "/delete_userinfo":
                        reply.Add(await DeleteUserInfo(activity));
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

                _databaseService.AuditMessageInDatabase(JsonConvert.SerializeObject(reply));
                return reply;
            }
			catch (Exception ex)
			{
				_databaseService.AuditMessageInDatabase($"ERROR: {ex.Message} {ex.StackTrace}");
				return new List<string>();
			}
		}

		private async Task<string> SaveNewUser(Activity activity)
		{
			if (activity.Conversation.IsGroup.GetValueOrDefault())
				return string.Empty;

            var codewarsUserName = activity.Text;
            var telegramId = int.Parse(activity.From.Id);

            if (!UserModel.IsValidCodewarsUserName(codewarsUserName))
			{
				return @"Логін Codewars має містити букви, цифри і знак '_'
					Якщо ви хотіли дати команду боту -- перевірте правильність написання і чи в ту сторону стоїть слеш на початку.
					Певні, що це таки ваш нік? Пишіть йому: @maksim36ua";
			}

            var user = await _usersRepository.Find(telegramId);

            if (user != null)
            {
                return $"Ви вже зареєстровані в рейтингу Codewars під ніком {user.CodewarsUsername}";
            }

            var codewarsUser = await _codewarsClient.GetCodewarsUser(codewarsUserName);

            if (codewarsUser == null)
            {
                return $"Користувач {codewarsUserName} не зареєстрований на Codewars";
            }

            user = UserModel.Create(telegramId,
                activity.From.Name,
                codewarsUserName,
                codewarsUser.Name,
                codewarsUser.Honor);

            try
            {
                await _usersRepository.Add(user);
                return $"Реєстрація успішна! Спасибі і хай ваш код завжди компілиться з першого разу :-)";
            }
            catch (Exception e)
            {
                _databaseService.AuditMessageInDatabase($"EXCEPTION: {e.Message}, CodewarsUser: {user.CodewarsUsername}");
                return $"Не вдалось створити користувача: {e.Message}";
            }
		}

		private List<string> GetWeeklyPoints(Activity activity)
		{
			return _databaseService.GetWeeklyPoints(int.Parse(activity.From.Id));
		}

		private async Task<string> DeleteUserInfo(Activity activity)
        {
            try
            {
                var telegramId = int.Parse(activity.From.Id);
                await _usersRepository.Delete(telegramId);
                return "Видалення пройшло успішно";
            }
            catch (Exception e)
            {
                _databaseService.AuditMessageInDatabase($"EXCEPTION: {e.Message}");
                return $"Не вдалось видалити дані: {e.Message}";
            }
		}

		private List<string> GetWeeklyRatingForChannel()
		{
			var rating = string.Concat(_databaseService.GetWeeklyRating(true).First(), $@"
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
