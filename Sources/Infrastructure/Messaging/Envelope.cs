using System;

namespace Infrastructure.Messaging
{
	public abstract class Envelope
	{
		public static Envelope<T> Create<T>(T message)
		{
			return new Envelope<T>(message);
		}
	}

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

		public static implicit operator Envelope<T>(T message)
		{
			return Envelope.Create(message);
		}
	}
}
