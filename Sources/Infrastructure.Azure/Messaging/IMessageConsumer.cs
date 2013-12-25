using System;

namespace Infrastructure.Azure.Messaging
{
	public interface IMessageConsumer
	{
		void Consume<T>(Action<T> action, TimeSpan timeout);
	}
}