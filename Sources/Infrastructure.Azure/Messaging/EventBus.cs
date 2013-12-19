using System.Collections.Generic;
using System.IO;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Microsoft.ServiceBus.Messaging;

namespace Infrastructure.Azure.Messaging
{
	public class EventBus : IEventBus
	{
		private readonly IMessageSender sender;
		private readonly IMetadataProvider metadataProvider;
		private readonly ISerializer serializer;

		public EventBus(IMessageSender sender, IMetadataProvider metadataProvider, ISerializer serializer)
		{
			this.sender = sender;
			this.metadataProvider = metadataProvider;
			this.serializer = serializer;
		}

		public void Publish(Envelope<IEvent> envelope)
		{
			var metadata = metadataProvider.GetMetadata(envelope.Message);
			sender.SendAsync(() => MessageFactory.Create(envelope, metadata, serializer));
		}
	}

	static class MessageFactory
	{
		public static BrokeredMessage Create(Envelope<IEvent> envelope, IDictionary<string, string> metadata, ISerializer serializer)
		{
			return Create(serializer, envelope.Message)
				.FillSessionId(envelope.Message.SourceId.ToString())
				.FillMessageId(envelope.MessageId)
				.FillCorrelationId(envelope.CorrelationId)
				.FillMetadata(metadata);
		}

		private static BrokeredMessage FillSessionId(this BrokeredMessage message, string sessionId)
		{
			message.SessionId = sessionId;
			return message;
		}

		private static BrokeredMessage FillCorrelationId(this BrokeredMessage message, string correlationId)
		{
			if (!string.IsNullOrWhiteSpace(correlationId))
			{
				message.CorrelationId = correlationId;
			}

			return message;
		}

		private static BrokeredMessage FillMessageId(this BrokeredMessage message, string messageId)
		{
			if (!string.IsNullOrWhiteSpace(messageId))
			{
				message.MessageId = messageId;
			}

			return message;
		}


		private static BrokeredMessage FillMetadata(this BrokeredMessage message, IDictionary<string, string> metadata)
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

		private static BrokeredMessage Create(ISerializer serializer, IEvent @event)
		{
			BrokeredMessage message;
			var stream = new MemoryStream();
			try
			{
				serializer.Serialize(stream, @event);
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
	}
}