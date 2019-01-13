using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ITKPI.CodewarsBot.Api.Configuration;
using ITKPI.CodewarsBot.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ITKPI.CodewarsBot.Api.Controllers
{
	//[BotAuthentication] TODO: HOW?
    [ApiController]
	public class MessagesController : ControllerBase
	{
	    private readonly BotConfig _config;
	    private readonly IMessageService _messageService;
	    private readonly IDatabaseService _databaseService;

		public MessagesController(IMessageService messageService, IDatabaseService databaseService, BotConfig config)
		{
		    _config = config;
		    _messageService = messageService;
			_databaseService = databaseService;
		}

		public async Task<IActionResult> Post([FromBody]Activity activity)
		{
			var bot = new TelegramBotClient(_config.BotApiToken);
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

				var responseMessages = await _messageService.ProcessMessage(activity);

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
					_databaseService.AuditMessageInDatabase($"EXCEPTION: {ex.Message} {ex.StackTrace}");
				}
			}
			else
			{
				HandleSystemMessage(activity);
			}

			return Ok();
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
