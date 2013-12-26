using System.Collections.Generic;
using CommandHandler.Handling;
using Infrastructure.Azure.EventSourcing;
using Infrastructure.Azure.Messaging;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Microsoft.ServiceBus.Messaging;
using Proto.Domain;

namespace CommandHandler
{
	class CommandHandlerService
	{
		static bool stopped;
		private TopicConsumer consumer;
		private CommandHandlerRegistry handlerRegistry;

		readonly ISerializer serializer;

		const string EventStoreConnectionString = "[ConnectionString from Azure]";
		readonly IEventStore eventStore;
		readonly ServiceBusSettings settings = new ServiceBusSettings();


		public CommandHandlerService()
		{
			serializer = new JsonSerializer();

			var eventSender = new TopicSender(settings, "proto/events");
			var eventBus = new EventBus(eventSender, new DummyMetadataProvider(), serializer);
			eventStore = new EventStore("tenant", EventStoreConnectionString, serializer, eventBus);

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
			
			var client = SubscriptionClient.CreateFromConnectionString(settings.ConnectionString, "proto/commands", "AllCommands");
			consumer = new TopicConsumer(client, serializer);
		}

		private void InitializeCommandHandlers()
		{
			handlerRegistry = new CommandHandlerRegistry();
			
			handlerRegistry.RegisterHandler(new CustomerCommandHandler(GetCustomerRepository));
		}

		IEventSourcedRepository<Customer> GetCustomerRepository()
		{
			return new EventSourcedRepository<Customer>(eventStore);
		}
	}
}