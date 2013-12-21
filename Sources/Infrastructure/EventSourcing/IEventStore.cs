using System;
using System.Collections.Generic;
using Infrastructure.Messaging;

namespace Infrastructure.EventSourcing
{
	public interface IEventStore
	{
		IEnumerable<IEvent> Load(Guid sourceId, int minVersion);
		void Save(Guid sourceId, IEnumerable<IEvent> events, IDictionary<string, string> metadata);
	}
}
