using Codewars_Bot.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Codewars_Bot.Services
{
    public class CodewarsConnectionService
    {
        public async Task<CodewarsResponseModel> GetCodewarsUser(string username)
        {
			var codewarsUser = new CodewarsResponseModel();

			using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri ($"https://www.codewars.com/api/v1/users/");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", Configuration.CodewarsApiToken);

                HttpResponseMessage response = await httpClient.GetAsync(username);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
					codewarsUser = JsonConvert.DeserializeObject<CodewarsResponseModel>(responseJson);
                    
                }

				return codewarsUser;
			}
        }
    }
}