using Codewars_Bot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codewars_Bot.Contracts
{
	public interface ICodewarsApiClient
	{
		Task<CodewarsResponseModel> GetCodewarsUser(string username);
	}
}
