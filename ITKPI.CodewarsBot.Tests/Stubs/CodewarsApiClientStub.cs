using System.Threading.Tasks;
using Codewars_Bot.Contracts;
using Codewars_Bot.Models;

namespace ITKPI.CodewarsBot.Tests.Stubs
{
    public class CodewarsApiClientStub : ICodewarsApiClient
    {
        public Task<CodewarsResponseModel> GetCodewarsUser(string username)
        {
            return Task.FromResult(new CodewarsResponseModel()
            {
                Name = "SomeCodewarsName",
                Honor = 9911
            });
        }
    }
}
