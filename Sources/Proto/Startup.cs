using Infrastructure.Azure.Documents;
using Infrastructure.Azure.Messaging;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Microsoft.WindowsAzure.Storage.Auth;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Owin;
using ISerializer = Infrastructure.Serialization.ISerializer;

namespace Proto
{
	class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			var storageConnectionString = "";
			var serviceBusSettings = new ServiceBusSettings();
			new ServiceBusConfig(serviceBusSettings).Initialize();

			app.UseNancy(options => options.Bootstrapper =
				new CustomBootstrapper(serviceBusSettings, storageConnectionString));
		}
	}

	public class CustomBootstrapper : DefaultNancyBootstrapper
	{
		private readonly ServiceBusSettings serviceBusSettings;
		readonly string storageConnectionString;

		public CustomBootstrapper(ServiceBusSettings serviceBusSettings, string storageConnectionString)
		{
			this.serviceBusSettings = serviceBusSettings;
			this.storageConnectionString = storageConnectionString;
		}

		protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
		{
			container.Register<IMessageSender>(
				new TopicSender(serviceBusSettings, "proto/commands"));
			container.Register<DocumentStore>()
				.UsingConstructor(() => new DocumentStore("tenant", storageConnectionString));
			
			container.Register<IMetadataProvider, DummyMetadataProvider>();
			container.Register<ISerializer, JsonSerializer>();
			container.Register<ICommandBus, CommandBus>().AsSingleton();

		}
	}
}
