using System;

namespace Infrastructure.Azure.Messaging
{
	public interface IMessageReceiver
	{
		T Receive<T>(TimeSpan timeout);
	}
}