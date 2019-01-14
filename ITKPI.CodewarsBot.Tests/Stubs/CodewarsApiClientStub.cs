using System.Threading.Tasks;
using ITKPI.CodewarsBot.Api.Contracts;
using ITKPI.CodewarsBot.Api.Models;

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
