using System.Configuration;

namespace Codewars_Bot
{
    public static class Configuration
    {
		public static string BotApiToken => ConfigurationManager.AppSettings["BotApiToken"];
		public static string DbConnection => ConfigurationManager.AppSettings["DbConnectionString"];
        public static string CodewarsApiToken => ConfigurationManager.AppSettings["CodewarsApiToken"];
    }
}