using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITKPI.CodewarsBot.Api.Contracts;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Telegram.Bot.Types.ReplyMarkups;

namespace ITKPI.CodewarsBot.Api
{
    public class CodewarsBot : IBot
    {
        private readonly IMessageService _messageService;
        private readonly IDatabaseService _databaseService;

        public CodewarsBot(IMessageService messageService, IDatabaseService databaseService)
        {
            _messageService = messageService;
            _databaseService = databaseService;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = new CancellationToken())
        {
            var activity = turnContext.Activity;

            if (activity.Type == ActivityTypes.Message)
            {
                var responseMessages = await _messageService.ProcessMessage(activity);

                try
                {
                    if (responseMessages.Count != 0)
                    {
                        foreach (var message in responseMessages)
                        {
                            var reply = activity.CreateReply(message);
                            
                            if (message == responseMessages.Last() && activity.Text != "/weekly_rating_channel")
                            {
                                reply.ChannelData = new
                                {
                                    reply_markup = CreateInlineKeyboard(),
                                    parse_mode = "HTML"
                                };
                            }
                            
                            await turnContext.SendActivityAsync(reply);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _databaseService.AuditMessageInDatabase($"EXCEPTION: {ex.Message} {ex.StackTrace}");
                }
            }
        }

        private static InlineKeyboardMarkup CreateInlineKeyboard()
        {
            return new InlineKeyboardMarkup(new[]
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
        }
    }
}
