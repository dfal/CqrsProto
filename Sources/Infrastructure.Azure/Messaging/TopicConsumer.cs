﻿using System;
using System.IO;
using Infrastructure.Serialization;
using Microsoft.ServiceBus.Messaging;

namespace Infrastructure.Azure.Messaging
{
	public class TopicConsumer : IMessageConsumer
	{
		private readonly SubscriptionClient client;
		private readonly ISerializer serializer;

		public TopicConsumer(SubscriptionClient client, ISerializer serializer)
		{
			this.client = client;
			this.serializer = serializer;
		}

		public void Consume<T>(Action<T> action)
		{
			Consume(action, TimeSpan.FromSeconds(5));
		}

		public void Consume<T>(Action<T> action, TimeSpan timeout)
		{
			var message = client.Receive(timeout);
			if (message == null) return;

			try
			{
				using (var stream = message.GetBody<Stream>())
				{
					action((T) serializer.Deserialize(stream));
				}

				message.Complete();
			}
			catch
			{
				// TODO: Add handle retrying before sending to DeadLetter;
				message.DeadLetter();
			}
		}
	}
}