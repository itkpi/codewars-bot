using Codewars_Bot.Contracts;
using Codewars_Bot.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Codewars_Bot.Infrastructure;

namespace Codewars_Bot.Services
{
    public class CodewarsService : ICodewarsService
    {
        private readonly CodewarsConfig _config;

        public CodewarsService(CodewarsConfig config)
        {
            _config = config;
        }

        public async Task<CodewarsResponseModel> GetCodewarsUser(string username)
        {
			using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri ($"https://www.codewars.com/api/v1/users/");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", _config.CodewarsApiToken);

                HttpResponseMessage response = await httpClient.GetAsync(username);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
					return JsonConvert.DeserializeObject<CodewarsResponseModel>(responseJson);
                }

				return null;
			}
        }
    }
}
