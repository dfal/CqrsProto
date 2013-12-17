using System;
using System.Collections.Generic;

namespace Infrastructure.EventSourcing
{
	public interface IEventStore
	{
		IEnumerable<EventData> Load(Guid sourceId, int minVersion);
		void Save(Guid sourceId, IEnumerable<EventData> events);
	}

	public sealed class EventData
	{
		public string SourceId { get; set; }
		public int SourceVersion { get; set; }
		public string SourceType { get; set; }
		public byte[] Payload { get; set; }
		public string CorrelationId { get; set; }

		public string EventType { get; set; }
	}
}
