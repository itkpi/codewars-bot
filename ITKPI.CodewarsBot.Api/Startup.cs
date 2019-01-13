using System;
using System.IO;
using ITKPI.CodewarsBot.Api.Configuration;
using ITKPI.CodewarsBot.Api.Contracts;
using ITKPI.CodewarsBot.Api.Infrastructure;
using ITKPI.CodewarsBot.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ITKPI.CodewarsBot.Api
{
    public class Startup : IStartup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.Formatting = Formatting.Indented;
                    JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Formatting = Formatting.Indented,
                        NullValueHandling = NullValueHandling.Ignore,
                    };
                });

            services.Configure<CodewarsConfig>(_configuration);
            services.Configure<DbConfig>(_configuration);

            services.TryAddTransient<IMessageService, MessageService>();
            services.TryAddTransient<ICodewarsService, CodewarsService>();
            services.TryAddTransient<IDatabaseService, DatabaseService>();


            if (bool.TryParse(_configuration["RunMigration"], out var runMigration) && runMigration)
            {
                var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var migrationsPath = Path.Combine(currentDirectory, _configuration["MigrationFilePath"]);
                var dbInfrastructure = new DatabaseInfrastructure(
                    _configuration["DBConnectionString"],
                    migrationsPath,
                    _configuration["MigrationDBName"]);
                dbInfrastructure.CreateIfNotExists().Wait();

                services.AddSingleton(dbInfrastructure);

                _configuration["DbConnectionString"] = dbInfrastructure.DbConnectionString;
            }

            services.AddBot<CodewarsBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(_configuration);
            });

            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_hostingEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseBotFramework();
        }
    }
}
