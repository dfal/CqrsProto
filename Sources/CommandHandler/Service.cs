using System;
using CommandHandler.Handling;
using Infrastructure.Azure.Messaging;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Microsoft.ServiceBus.Messaging;

namespace CommandHandler
{
	class Service
	{
		static bool stopped;
		private readonly IMessageReceiver receiver;
		private readonly CommandHandlerRegistry handlerRegistry;

		public Service()
		{
			// TODO: Get settings from config file.
			var serializer = new JsonSerializer();
			var client = SubscriptionClient.Create("proto/commands", "AllCommands");

			receiver = new TopicReceiver(client, serializer);
			handlerRegistry = new CommandHandlerRegistry();
		}

		public void Start()
		{
			stopped = false;
			ReceiveMessages();
		}

		public void Stop()
		{
			stopped = true;
		}

		void ReceiveMessages()
		{
			while (!stopped)
			{
				var message = receiver.Receive<ICommand>(TimeSpan.FromSeconds(5));
				if (message == null) continue;

				handlerRegistry.Handle(message);
			}
		}
	}
}