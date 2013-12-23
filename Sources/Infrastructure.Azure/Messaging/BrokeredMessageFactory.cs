using System.Collections.Generic;
using System.IO;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Microsoft.ServiceBus.Messaging;

namespace Infrastructure.Azure.Messaging
{
	public static class BrokeredMessageFactory
	{
		public static BrokeredMessage Create<T>(Envelope<T> envelope, IDictionary<string, string> metadata, ISerializer serializer)
		{
			return Create(serializer, envelope.Message)
				.FillMessageId(envelope.MessageId)
				.FillCorrelationId(envelope.CorrelationId)
				.FillMetadata(metadata);
		}

		static BrokeredMessage Create(ISerializer serializer, object payload)
		{
			BrokeredMessage message;
			var stream = new MemoryStream();
			try
			{
				serializer.Serialize(stream, payload);
				stream.Position = 0;

				message = new BrokeredMessage(stream, true);
			}
			catch
			{
				stream.Dispose();
				throw;
			}

			return message;
		}

		static BrokeredMessage FillCorrelationId(this BrokeredMessage message, string correlationId)
		{
			if (!string.IsNullOrWhiteSpace(correlationId))
			{
				message.CorrelationId = correlationId;
			}

			return message;
		}

		static BrokeredMessage FillMessageId(this BrokeredMessage message, string messageId)
		{
			if (!string.IsNullOrWhiteSpace(messageId))
			{
				message.MessageId = messageId;
			}

			return message;
		}


		static BrokeredMessage FillMetadata(this BrokeredMessage message, IDictionary<string, string> metadata)
		{
			if (metadata != null)
			{
				foreach (var pair in metadata)
				{
					message.Properties[pair.Key] = pair.Value;
				}
			}

			return message;
		}
	}
}