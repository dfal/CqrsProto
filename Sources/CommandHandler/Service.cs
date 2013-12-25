using System;
using System.IO;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Microsoft.ServiceBus.Messaging;

namespace CommandHandler
{
	class Service
	{
		static bool stopped;
		private readonly JsonSerializer serializer;
		private readonly SubscriptionClient client;

		public Service()
		{
			serializer = new JsonSerializer();

			// TODO: Get settings from config file.
			client = SubscriptionClient.Create("proto/commands", "AllCommands");
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
				ConsumeMessage();
			}
		}

		void ConsumeMessage()
		{
			// TODO: Add retry mechanism;
			var message = client.Receive(TimeSpan.FromSeconds(5));
			if (message == null) return;

			try
			{
				using (var stream = message.GetBody<Stream>())
				{
					var payload = serializer.Deserialize(stream);
					var command = payload as ICommand;
				}

				message.Complete();
			}
			catch
			{
				message.DeadLetter();
			}
		}
	}
}