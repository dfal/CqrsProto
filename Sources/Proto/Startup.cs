using Infrastructure.Azure.Messaging;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
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
			var serviceBusSettings = new ServiceBusSettings();
			new ServiceBusConfig(serviceBusSettings).Initialize();

			app.UseNancy(options => options.Bootstrapper = new CustomBootstrapper(serviceBusSettings));
		}
	}

	public class CustomBootstrapper : DefaultNancyBootstrapper
	{
		private readonly ServiceBusSettings serviceBusSettings;

		public CustomBootstrapper(ServiceBusSettings serviceBusSettings)
		{
			this.serviceBusSettings = serviceBusSettings;
		}

		protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
		{
			container.Register<IMessageSender>(
				new TopicSender(serviceBusSettings, "proto/commands"));
			
			container.Register<IMetadataProvider, DummyMetadataProvider>();
			container.Register<ISerializer, JsonSerializer>();
			container.Register<ICommandBus, CommandBus>().AsSingleton();
		}
	}
}
