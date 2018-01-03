using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Codewars_Bot.Services;

namespace Codewars_Bot
{
	[BotAuthentication]
	public class MessagesController : ApiController
	{
		public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
		{
			var databaseConnectionService = new DatabaseConnectionService();

			try
			{
				if (activity.Type == ActivityTypes.Message)
				{

					ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

					var messageService = new MessageService();
					var responseMessage = await messageService.MessageHandler(activity);

					if (!string.IsNullOrEmpty(responseMessage))
					{
						Activity reply = activity.CreateReply($"{responseMessage}");
						reply.ReplyToId = new Guid().ToString();
						databaseConnectionService.AuditMessageInDatabase(JsonConvert.SerializeObject(reply));
						await connector.Conversations.ReplyToActivityAsync(reply);
					}
				}
				else
				{
					HandleSystemMessage(activity);
				}
			}
			catch (Exception ex)
			{
				databaseConnectionService.AuditMessageInDatabase($"ERROR: {ex.Message} {ex.StackTrace}");
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