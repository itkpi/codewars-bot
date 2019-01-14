using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ITKPI.CodewarsBot.Api.Configuration;
using ITKPI.CodewarsBot.Api.Contracts;
using ITKPI.CodewarsBot.Api.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ITKPI.CodewarsBot.Api.Codewars
{
    public class CodewarsApiClient : ICodewarsApiClient
    {
        private readonly HttpClient _httpClient;

        public CodewarsApiClient(IOptions<CodewarsConfig> config)
        {
            _httpClient = new HttpClient();
            ConfigureClient(config.Value);
        }

        private void ConfigureClient(CodewarsConfig config)
        {
            _httpClient.BaseAddress = new Uri($"https://www.codewars.com/api/v1/users/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("Authorization", config.CodewarsApiToken);
        }

        public async Task<CodewarsResponseModel> GetCodewarsUser(string username)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(username);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CodewarsResponseModel>(responseJson);
            }

            return null;
        }
    }
}
