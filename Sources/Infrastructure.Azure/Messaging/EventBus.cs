using Infrastructure.Messaging;
using Infrastructure.Serialization;

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
			sender.SendAsync(BrokeredMessageFactory.Create(envelope, metadata, serializer));
		}
	}
}