using CommandHandler.Handling;
using Infrastructure.Azure.Messaging;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Microsoft.ServiceBus.Messaging;

namespace CommandHandler
{
	class CommandHandlerService
	{
		static bool stopped;
		private TopicConsumer consumer;
		private CommandHandlerRegistry handlerRegistry;

		public CommandHandlerService()
		{
			InitializeConsumer();
			InitializeCommandHandlers();
		}

		public void Start()
		{
			stopped = false;
			while (!stopped)
			{
				consumer.Consume<ICommand>(handlerRegistry.Handle);
			}
		}

		public void Stop()
		{
			stopped = true;
		}

		private void InitializeConsumer()
		{
			// TODO: Get settings from config file.
			var serializer = new JsonSerializer();
			var client = SubscriptionClient.Create("proto/commands", "AllCommands");
			consumer = new TopicConsumer(client, serializer);
		}

		private void InitializeCommandHandlers()
		{
			handlerRegistry = new CommandHandlerRegistry();
			// Register command handlers here;
		}
	}
}