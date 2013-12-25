using System;
using System.IO;
using Infrastructure.Serialization;
using Microsoft.ServiceBus.Messaging;

namespace Infrastructure.Azure.Messaging
{
	public class TopicReceiver : IMessageReceiver
	{
		private readonly SubscriptionClient client;
		private readonly JsonSerializer serializer;

		public TopicReceiver(SubscriptionClient client, JsonSerializer serializer)
		{
			this.client = client;
			this.serializer = serializer;
		}

		public T Receive<T>(TimeSpan timeout)
		{
			var message = client.Receive(timeout);
			if (message == null) return default(T);

			var payload = default(T);
			try
			{
				using (var stream = message.GetBody<Stream>())
				{
					payload = (T) serializer.Deserialize(stream);
				}

				message.Complete();
			}
			catch
			{
				message.DeadLetter();
			}

			return payload;
		}
	}
}