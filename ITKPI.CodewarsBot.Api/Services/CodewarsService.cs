﻿using ITKPI.CodewarsBot.Api.Contracts;
using ITKPI.CodewarsBot.Api.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ITKPI.CodewarsBot.Api.Configuration;
using Microsoft.Extensions.Options;

namespace ITKPI.CodewarsBot.Api.Services
{
    public class CodewarsService : ICodewarsService
    {
        private readonly CodewarsConfig _config;

        public CodewarsService(IOptions<CodewarsConfig> config)
        {
            _config = config.Value;
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