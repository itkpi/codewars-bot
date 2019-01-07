using Autofac;
using Codewars_Bot.Contracts;
using Codewars_Bot.Infrastructure;
using Codewars_Bot.Services;
using Microsoft.Extensions.Configuration;

namespace Codewars_Bot
{
	public class MessagingModule : Module
	{
	    private readonly IConfiguration _configuration;

	    public MessagingModule(IConfiguration configuration)
	    {
	        _configuration = configuration;
	    }

		protected override void Load(ContainerBuilder builder)
		{
			base.Load(builder);

            var botConfig = new BotConfig();
		    _configuration.Bind(botConfig);
            builder.RegisterInstance(botConfig).AsSelf();

		    var codewarsConfig = new CodewarsConfig();
		    _configuration.Bind(codewarsConfig);
		    builder.RegisterInstance(codewarsConfig).AsSelf();

		    var dbConfig = new DbConfig();
		    _configuration.Bind(dbConfig);
		    builder.RegisterInstance(dbConfig).AsSelf();

            builder.RegisterType<MessageService>().As<IMessageService>().InstancePerLifetimeScope();
			builder.RegisterType<CodewarsService>().As<ICodewarsService>().InstancePerLifetimeScope();
			builder.RegisterType<DatabaseService>().As<IDatabaseService>().InstancePerLifetimeScope();
		}
	}
}
