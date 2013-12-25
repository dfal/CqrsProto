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

		public static implicit operator Envelope<T>(T message)
		{
			return message.ToEnvelope();
		}
	}

	public static class EnvelopeExtensions
	{
		public static Envelope<T> ToEnvelope<T>(this T message)
		{
			return new Envelope<T>(message)
			{
				MessageId = Guid.NewGuid().ToString()
			};
		}
	}
}
