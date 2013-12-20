using Microsoft.ServiceBus.Messaging;

namespace Infrastructure.Azure.Messaging
{
	public interface IMessageSender
	{
		void SendAsync(BrokeredMessage message);

		void Send(BrokeredMessage message);
	}
}