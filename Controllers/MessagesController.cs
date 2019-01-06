using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Codewars_Bot.Contracts;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using System.Linq;
using System.IO;

namespace Codewars_Bot
{
	[BotAuthentication]
	public class MessagesController : ApiController
	{
		private IMessageService MessageService { get; set; }
		private IDatabaseService DatabaseService { get; set; }

		public MessagesController(IMessageService messageService, IDatabaseService databaseService)
		{
			MessageService = messageService;
			DatabaseService = databaseService;
		}

		public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
		{
			try
			{
				ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

				var bot = new TelegramBotClient(Codewars_Bot.Configuration.BotApiToken);
				var inlineKeyboard = new InlineKeyboardMarkup(new[]
				{
				new []
				{
					InlineKeyboardButton.WithCallbackData("Weekly Rating", "/weekly_rating"),
					InlineKeyboardButton.WithCallbackData("Total Rating", "/total_rating"),
				},
				new []
				{
					InlineKeyboardButton.WithCallbackData("My Points For This Week", "/my_weekly_points"),
					InlineKeyboardButton.WithCallbackData("Delete Me From Rating", "/delete_userinfo"),
				}
			});

				var responseMessages = await MessageService.ProcessMessage(activity);

				if (activity.Text == "/picture")
				{

					var stream = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "1.png"), FileMode.Open);

					var photo = new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream);
					await bot.SendPhotoAsync(new ChatId(activity.Conversation.Id), photo, caption: responseMessages.First());
				}

				if (responseMessages.Count != 0)
				{
					foreach (var message in responseMessages)
					{
						if (message == responseMessages.Last() && activity.Text != "/weekly_rating_channel")
						{
							await bot.SendTextMessageAsync(new ChatId(activity.Conversation.Id), message,
									replyMarkup: inlineKeyboard, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
							continue;
						}

						await bot.SendTextMessageAsync(new ChatId(activity.Conversation.Id), message, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
					}
				}
			}
			catch (Exception ex)
			{
				DatabaseService.AuditMessageInDatabase($"EXCEPTION: {ex.Message} {ex.StackTrace}");
			}

			var response = Request.CreateResponse(HttpStatusCode.OK);
			return response;
		}
	}
}