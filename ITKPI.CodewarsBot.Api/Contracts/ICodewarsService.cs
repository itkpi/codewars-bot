using System.Threading.Tasks;
using ITKPI.CodewarsBot.Api.Models;

namespace ITKPI.CodewarsBot.Api.Contracts
{
	public interface ICodewarsService
	{
		Task<CodewarsResponseModel> GetCodewarsUser(string username);
	}
}
