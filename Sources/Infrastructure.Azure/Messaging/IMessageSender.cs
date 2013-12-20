using System;
using Microsoft.ServiceBus.Messaging;

namespace Infrastructure.Azure.Messaging
{
	public interface IMessageSender
	{
		void SendAsync(Func<BrokeredMessage> messageFactory);

		void Send(Func<BrokeredMessage> messageFactory);
	}
}