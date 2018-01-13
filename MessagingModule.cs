using Autofac;
using Codewars_Bot.Contracts;
using Codewars_Bot.Services;

namespace Codewars_Bot
{
	public class MessagingModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			base.Load(builder);

			builder.RegisterType<MessageService>().As<IMessageService>().InstancePerLifetimeScope();
			builder.RegisterType<CodewarsService>().As<ICodewarsService>().InstancePerLifetimeScope();
			builder.RegisterType<DatabaseService>().As<IDatabaseService>().InstancePerLifetimeScope();
		}
	}
}