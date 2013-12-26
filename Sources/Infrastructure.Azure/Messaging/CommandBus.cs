using Infrastructure.Messaging;
using Infrastructure.Serialization;

namespace Infrastructure.Azure.Messaging
{
	public class CommandBus : ICommandBus
	{
		private readonly IMessageSender sender;
		private readonly IMetadataProvider metadataProvider;
		private readonly ISerializer serializer;

		public CommandBus(IMessageSender sender, IMetadataProvider metadataProvider, ISerializer serializer)
		{
			this.sender = sender;
			this.metadataProvider = metadataProvider;
			this.serializer = serializer;
		}

		public void Send(Envelope<ICommand> envelope)
		{
			var metadata = metadataProvider.GetMetadata(envelope.Message);
			sender.Send(BrokeredMessageFactory.Create(envelope, metadata, serializer));
		}
	}
}