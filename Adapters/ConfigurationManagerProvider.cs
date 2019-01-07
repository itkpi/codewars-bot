using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace Codewars_Bot.Adapters
{
    /// <summary>
    /// Loads the ConfigurationManager into IConfiguration object from Microsoft.Extensions.Configuration package
    /// </summary>
    public class ConfigurationManagerProvider : ConfigurationProvider, IConfigurationSource
    {
        public override void Load()
        {
            foreach (ConnectionStringSettings connectionString in ConfigurationManager.ConnectionStrings)
            {
                Data.Add($"ConnectionStrings:{connectionString.Name}", connectionString.ConnectionString);
            }

            foreach (var settingKey in ConfigurationManager.AppSettings.AllKeys)
            {
                Data.Add(settingKey, ConfigurationManager.AppSettings[settingKey]);
            }
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return this;
        }
    }
}
