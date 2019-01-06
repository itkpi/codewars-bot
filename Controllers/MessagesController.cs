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
			var bot = new TelegramBotClient(Codewars_Bot.Configuration.BotApiToken);
			var inlineKeyboard = new InlineKeyboardMarkup(new[]
			{
				new []
				{
					InlineKeyboardButton.WithCallbackData("Weekly Rating", "/weekly_rating"),
					InlineKeyboardButton.WithCallbackData("Total Rating", "/total_rating")
				},
				new []
				{
					InlineKeyboardButton.WithCallbackData("My Points For This Week", "/my_weekly_points"),
					InlineKeyboardButton.WithCallbackData("Delete Me From Rating", "/delete_userinfo"),
				}
			});

			if (activity.Type == ActivityTypes.Message)
			{
				ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

				var responseMessages = await MessageService.ProcessMessage(activity);

				try
				{
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
			}
			else
			{
				HandleSystemMessage(activity);
			}

			var response = Request.CreateResponse(HttpStatusCode.OK);
			return response;
		}

		private Activity HandleSystemMessage(Activity message)
		{
			if (message.Type == ActivityTypes.DeleteUserData)
			{
				// Implement user deletion here
				// If we handle user deletion, return a real message
			}
			else if (message.Type == ActivityTypes.ConversationUpdate)
			{
				// Handle conversation state changes, like members being added and removed
				// Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
				// Not available in all channels
			}
			else if (message.Type == ActivityTypes.ContactRelationUpdate)
			{
				// Handle add/remove from contact lists
				// Activity.From + Activity.Action represent what happened
			}
			else if (message.Type == ActivityTypes.Typing)
			{
				// Handle knowing tha the user is typing
			}
			else if (message.Type == ActivityTypes.Ping)
			{
			}

			return null;
		}
	}
}
