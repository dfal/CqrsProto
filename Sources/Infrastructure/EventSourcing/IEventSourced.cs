using System;
using System.Collections.Generic;
using Infrastructure.Messaging;

namespace Infrastructure.EventSourcing
{
	public interface IEventSourced
	{
		Guid Id { get; }

		int Version { get; }

		IEvent[] Flush();

		void Restore(IEnumerable<IEvent> history);
	}
}
