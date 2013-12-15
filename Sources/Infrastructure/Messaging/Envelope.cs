using System;

namespace Infrastructure.Messaging
{
	public class Envelope<T>
	{
		public Envelope(T message)
		{
			Message = message;
		}
		
		public T Message { get; private set; }

		public string MessageId { get; set; }

		public string CorrelationId { get; set; }

		public TimeSpan Delay { get; set; }
	}
}
