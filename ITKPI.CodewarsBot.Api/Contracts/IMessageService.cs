using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace ITKPI.CodewarsBot.Api.Contracts
{
	public interface IMessageService
	{
		Task<List<string>> ProcessMessage(Activity activity);
	}
}
