using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Codewars_Bot.Services;
using Codewars_Bot.Contracts;

namespace Codewars_Bot
{
	[BotAuthentication]
	public class MessagesController : ApiController
	{
		private IMessageService MessageService { get; set; }

		public MessagesController(IMessageService messageService)
		{
			MessageService = messageService;
		}

		public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
		{

			if (activity.Type == ActivityTypes.Message)
			{
				ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

				var responseMessage = await MessageService.ProcessMessage(activity);

				if (!string.IsNullOrEmpty(responseMessage))
				{
					Activity reply = activity.CreateReply($"{responseMessage}");
					reply.ReplyToId = new Guid().ToString();
					await connector.Conversations.ReplyToActivityAsync(reply);
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