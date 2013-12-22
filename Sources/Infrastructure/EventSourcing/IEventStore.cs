using System;
using System.Collections.Generic;
using Infrastructure.Messaging;

namespace Infrastructure.EventSourcing
{
	public interface IEventStore
	{
		IEnumerable<Commit> Load(Guid sourceId, int minVersion);
		void Save(Commit commit);
	}

	public class Commit
	{
		public Guid Id { get; set; }
		public Guid? ParentId { get; set; }

		public Guid SourceId { get; set; }
		public string SourceType { get; set; }

		public IEvent[] Changes { get; set; }
		public IDictionary<string, string> Metadata { get; set; }
	}
}
