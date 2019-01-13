using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace ITKPI.CodewarsBot.Api.Contracts
{
	public interface IMessageService
	{
		Task<List<string>> ProcessMessage(Activity activity);
	}
}
