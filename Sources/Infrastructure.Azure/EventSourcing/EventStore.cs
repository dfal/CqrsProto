using System;
using System.Collections.Generic;
using Infrastructure.EventSourcing;

namespace Infrastructure.Azure.EventSourcing
{
	public class EventStore : IEventStore
	{
		public IEnumerable<EventData> Load(Guid sourceId, int minVersion)
		{
			throw new NotImplementedException();
		}

		public void Save(Guid sourceId, IEnumerable<EventData> events)
		{
			throw new NotImplementedException();
		}
	}
}
